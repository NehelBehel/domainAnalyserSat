# Stage 3 — Domain parser

Planning document for the parsing layer of the domain import feature.
Written as a specification, not as code: signatures, algorithms and reasoning.
The implementation is to be typed by hand.

---

## Why Stage 3 comes first

The parser is the only part of the import feature that depends on nothing else.
It touches no database, no XAML, no `appState`. That means it can be built and
tested before any of the UI wiring exists.

Everything else in the import feature is waiting on it:

- Stage 4 (preview) displays a `parseResult`.
- Stage 5 (commit) writes `parseResult.domains` to SQLite.
- Stage 6 (pipeline stepper) shows `parseResult.validCount`.

Building it first means the parsing logic can be proven correct while the only
moving part is the parser itself. If it were built last, a wrong domain count
could be a parser bug, a column-mapping bug, or a database bug, and all three
would have to be eliminated one at a time.

---

## Files added

Two new files in `domainAnalyserSat/`, alongside the existing sources:

| File | Contains | Modelled on |
|---|---|---|
| `parseResult.cs` | `parseResult` — the data produced by one parse | `Domain.cs`, `Session.cs` |
| `domainParser.cs` | `domainParser` — the static parsing logic | `trademarkCheck.cs` |

Both use `namespace domainAnalyserSat`.

No `.csproj` change is needed. The project is SDK-style, so every `.cs` file in
the project folder is compiled automatically — this is why `trademarkCheck.cs`
started building the moment it was created.

**No existing file changes in Stage 3.** The parser is additive. The first call
into it happens in Stage 4.

---

## Access modifiers

Make **both types `public`**.

`domainParser.parse` returns a `parseResult`. C# will not allow a public method
to return a type less accessible than itself — that is compile error CS0050,
"inconsistent accessibility". So the two must match.

This is the same reason `user.cs` carries the comment *"made public to be
accessed so userepo can return"* — `UserRepo.GetByUsername` returns a `User`,
so `User` had to be public too. Same rule, same fix.

---

## Part 1 — `parseResult`

A plain data class. One instance describes the outcome of parsing one file.

### Members

| Member | Type | Meaning |
|---|---|---|
| `domains` | `List<string>` | Clean, valid, deduplicated domains in original file order |
| `headers` | `string[]` | The header row split into columns; empty when the file has no header |
| `totalRows` | `int` | Data rows examined (excludes the header and blank lines) |
| `validCount` | `int` | Rows that produced a domain — equals `domains.Count` |
| `invalidCount` | `int` | Rows rejected by validation |
| `duplicateCount` | `int` | Rows that were valid but already seen |

Initialise `domains` to a new empty list and `headers` to an empty array at
declaration, so a caller never receives null. This is the same defensive pattern
as `public string domain { get; set; } = string.Empty;` in `Domain.cs`.

### Why a class instead of returning `List<string>`

Three reasons, in order of importance:

1. **It prevents silent data loss.** If the parser returned only a list, a file
   of 5,000 rows that produced 4,812 domains would look identical to a file that
   genuinely contained 4,812. The user would never learn that 188 rows were
   discarded. Reporting the counts turns a silent failure into a visible one.

2. **The UI needs the counts anyway.** The preview summary and the pipeline
   stepper's `count` property both display them. Without the object they would
   have to be recomputed or passed around as loose `out` parameters.

3. **It is a legitimate object.** The scoring engine is where OOP is
   demonstrated properly, but `parseResult` is a real class with real state,
   created and returned by the parser, and is honest to describe as such.

### The counting invariant

```
totalRows == validCount + invalidCount + duplicateCount
```

Every row examined lands in exactly one of the three buckets. This is worth
knowing because it is directly testable — if the three do not sum to
`totalRows`, there is a branch in `parse` that falls through without counting.

Blank lines are skipped **before** `totalRows` is incremented, so they do not
count as rows at all. A trailing newline at the end of a file is extremely
common and should not be reported as an invalid row.

---

## Part 2 — `domainParser`

A static class. No instance state, no fields except what a method needs locally.

### Why static, and why it does not read the file

`trademarkCheck` is static because it holds one shared list of terms. The parser
is static for a different reason: it holds nothing at all. Every method is a
pure transformation — same input always produces the same output.

**File reading stays in the caller, not in the parser.** `parse` takes a
`string[]` of lines, not a path. Two reasons:

- **Testability.** A parser that takes an array can be tested by handing it a
  literal array. A parser that opens files needs a file on disk for every test
  case, which makes hand-tracing for the folio much harder.
- **Error handling belongs with the UI.** A missing or locked file is a problem
  the user must be told about, via `UiHelper.showError`. The parser has no
  access to the error label and should not be deciding how failures are
  displayed.

So `File.ReadAllLines` and its `try/catch (IOException)` live in
`importView.xaml.cs` in Stage 4. `File.ReadAllLines` is appropriate here because
the scale target is ~5,000 rows — a few hundred kilobytes, which is trivial to
hold in memory. Streaming would be the right call at a scale this project does
not target.

### Method summary

| Method | Signature | Purpose |
|---|---|---|
| `detectDelimiter` | `static char detectDelimiter(string line)` | Guess the separator from a sample line |
| `splitLine` | `static string[] splitLine(string line, char delimiter)` | Split one row into columns |
| `normalise` | `static string normalise(string raw)` | Clean one cell into a bare domain |
| `isValid` | `static bool isValid(string domain)` | Test a normalised domain against the rules |
| `parse` | `static parseResult parse(string[] lines, char delimiter, bool hasHeader, int domainColumn)` | Drive the whole process |

The first four are helpers for the fifth. They are public rather than private so
they can be exercised individually while testing.

---

### `detectDelimiter(string line)`

Count occurrences of `,`, `\t` and `;` in the supplied line. Return whichever
character occurred most often. If all three counts are zero, return `'\0'`.

`'\0'` is the sentinel for "no delimiter — treat the whole line as one column".
This is the normal case for a `.txt` file with one domain per line, so it is not
an error condition.

**Why the first non-blank line is the right sample:** it is either the header or
the first data row, and in a well-formed file both contain the full set of
separators. Sampling more lines would be more robust against ragged files, but
adds complexity for a case the "Auto-detect" option already lets the user
override manually.

**Called only when the user leaves the dropdown on Auto-detect.** If they picked
comma, tab or semicolon explicitly, Stage 4 passes that character straight
through and never calls this method. The user's explicit choice always wins over
the guess.

---

### `splitLine(string line, char delimiter)`

If `delimiter` is `'\0'`, return a single-element array containing the whole
line. Otherwise return `line.Split(delimiter)`.

Deliberately simple. It does **not** handle quoted fields containing the
delimiter — a CSV cell like `"Smith, John"` would split into two. This is an
accepted limitation: the mapped column holds a domain name, and domain names
cannot contain commas, tabs or semicolons. A full RFC 4180 CSV reader would be
correct in general but is not needed for the one column that matters here.

Be ready to state that limitation and the reason for it, rather than being
caught by it.

---

### `normalise(string raw)`

Turns a raw cell into a bare domain. **The order of these steps matters** — each
one assumes the previous has already run.

1. `Trim()` — remove surrounding whitespace.
2. `ToLowerInvariant()` — fold case.
3. Strip a leading and trailing `"` — CSV quoting.
4. `Trim()` again — quotes may have been wrapping padded whitespace.
5. Strip a leading `https://`, otherwise a leading `http://`.
6. Strip a leading `www.`.
7. Cut everything from the first `/` onwards — removes any path.
8. Strip a single trailing `.` — the DNS root dot.

**Why that order:**

- Quotes before scheme: a cell of `"http://x.com"` would fail the scheme test
  while the leading quote is still attached.
- `https://` before `http://`: `https://` also starts with `http`, so testing
  the shorter prefix first would leave a stray `s://`.
- Scheme before `www.`: in `http://www.x.com` the `www.` is not at the start
  until the scheme is gone.
- Path cut before validation: `test.co.uk/path` contains a `/`, which no
  validation rule permits, so it would be rejected as invalid rather than
  cleaned into `test.co.uk`.

**Why `ToLowerInvariant` and not `ToLower`:** `ToLower` uses the current culture.
Under a Turkish locale, uppercase `I` lowercases to a dotless `ı`, so `IBM.COM`
would normalise differently on a Turkish machine than an Australian one. The SRS
requires deterministic, reproducible output — same input, same result, every
time — and a culture-dependent method breaks that guarantee. `trademarkCheck`
already uses `ToLowerInvariant` for exactly this reason.

**Known gap:** a port suffix such as `example.com:8080` is not stripped and will
be rejected as invalid. Domain lists effectively never carry ports, so this is
left alone rather than adding an unused branch.

---

### `isValid(string domain)`

Runs against an already-normalised string, so it can assume lowercase input.

**Whole-string rules:**

| Rule | Reason |
|---|---|
| Not empty | An empty cell is not a domain |
| Contains at least one `.` | A domain needs a name and a TLD |
| Total length ≤ 253 characters | DNS limit |
| Does not start or end with `.` | `.com` and `example.` are malformed |
| No `..` anywhere | Implies an empty label |

**Then split on `.` and check every label:**

| Rule | Reason |
|---|---|
| Length between 1 and 63 | DNS limit per label |
| Only `a–z`, `0–9`, `-` | Already lowercased, so no uppercase test is needed |
| Does not start or end with `-` | `-bad-.com` is not registrable |

**Finally, check the last label (the TLD):**

| Rule | Reason |
|---|---|
| At least 2 characters | No single-letter TLDs exist |
| All alphabetic | Excludes `192.168.0.1`, which passes every other rule |

That last rule is the one that stops IP addresses being imported as domains,
which is worth knowing since it is not obvious from the others.

**Rejecting is not the same as crashing.** An invalid row increments
`invalidCount` and is skipped. A malformed file should produce a report, not an
exception.

---

### `parse(string[] lines, char delimiter, bool hasHeader, int domainColumn)`

The driver. Pseudocode:

```
result = new parseResult
if lines is empty:
    return result

start = 0
if hasHeader and lines has at least one line:
    result.headers = splitLine(lines[0], delimiter)
    start = 1

seen = new HashSet<string>

for i = start to end of lines:
    line = lines[i]

    if line is null or whitespace:
        continue                        // skipped, not counted

    result.totalRows = result.totalRows + 1

    parts = splitLine(line, delimiter)

    if domainColumn >= parts.Length:    // ragged row, column missing
        result.invalidCount = result.invalidCount + 1
        continue

    candidate = normalise(parts[domainColumn])

    if not isValid(candidate):
        result.invalidCount = result.invalidCount + 1
        continue

    if not seen.Add(candidate):         // Add returns false if already present
        result.duplicateCount = result.duplicateCount + 1
        continue

    result.domains.Add(candidate)
    result.validCount = result.validCount + 1

return result
```

**The ragged-row guard is essential.** Real exported CSVs contain short rows. If
the mapped column is index 2 and a row only has two fields, indexing `parts[2]`
throws `IndexOutOfRangeException` and the whole import dies on one bad line.
Checking the length turns a crash into a counted rejection.

**`seen.Add` does test-and-insert in one call.** It returns `false` if the value
was already in the set, so there is no need for a separate `Contains` check
followed by an `Add`. One lookup instead of two.

**Why a `HashSet` alongside the `List`:** the list preserves file order, which
the preview needs. The set answers "have I seen this?" in constant time.
`List.Contains` is a linear scan — at 5,000 domains that averages around 12.5
million string comparisons across the run, versus roughly 5,000 hash lookups.
Two collections is the deliberate trade: a small amount of extra memory to avoid
quadratic behaviour.

**Deduplication happens after normalisation**, which is what makes
`Example.com`, `www.example.com` and `HTTP://EXAMPLE.COM` collapse into one
entry. Deduplicating the raw cells instead would let all three through as
distinct rows.

**Order of the checks is deliberate:** validity is tested before duplication. A
row that is both malformed and repeated counts once, as invalid. Reversing them
would mean a repeated piece of junk was reported as a duplicate, which is
misleading.

**When `hasHeader` is false**, `headers` stays empty. Stage 4 should then label
the column selector `Column 1`, `Column 2`, … generated from the number of
fields in the first data row, since there are no real names to show.

---

## Testing Stage 3 before any UI exists

The parser can be exercised without touching XAML. `btnBrowse_Click` already
works and already produces a valid path, so add three temporary lines at the end
of `setSelectedFile`:

1. `File.ReadAllLines(path)` into a local array.
2. Call `domainParser.parse(...)` with hardcoded arguments — delimiter `','`,
   `hasHeader: true`, `domainColumn: 0`.
3. `MessageBox.Show` the four counts and the first few entries of `domains`.

This is the same throwaway-verification approach as the existing
`btnSkipLogin_Click`, which currently `MessageBox`es four `trademarkCheck`
results. **Delete these lines once Stage 4 exists** — the real caller replaces
them.

### Test file and expected output

Save as `test-import.csv`:

```
domain,category,priority
Example.com,Technology,High
www.example.com,Tech,Low
http://test.co.uk/path,Retail,High
not a domain,Junk,Low
-bad-.com,Junk,Low
"quoted.com",Finance,High

third.io,Media,Low
```

Parsed with delimiter `,`, `hasHeader: true`, `domainColumn: 0`:

| Field | Expected | Why |
|---|---|---|
| `headers` | `domain`, `category`, `priority` | Row 1 consumed as header |
| `totalRows` | 7 | 8 data lines minus the blank one, which is skipped uncounted |
| `validCount` | 4 | |
| `invalidCount` | 2 | `not a domain` (space), `-bad-.com` (leading hyphen on label) |
| `duplicateCount` | 1 | `www.example.com` normalises onto `example.com` |
| `domains` | `example.com`, `test.co.uk`, `quoted.com`, `third.io` | File order preserved |

Check `7 == 4 + 2 + 1`. If the invariant fails, a branch in `parse` is falling
through without counting.

This one file covers case folding, `www.` stripping, scheme stripping, path
stripping, quote stripping, whole-string validation, label validation,
deduplication across different surface forms, and blank-line handling.

### Further cases worth running

| File | Checks |
|---|---|
| Plain `.txt`, one domain per line, no header | `detectDelimiter` returns `'\0'`; single-column path |
| Semicolon-delimited | Auto-detect picks `;` over `,` |
| CSV with the domain in column 2, not column 1 | `domainColumn` is honoured |
| Rows with fewer fields than `domainColumn` | Counted invalid, no crash |
| Completely empty file | Returns an empty result, no exception |
| Header-only file, no data rows | `totalRows` is 0, `headers` populated |
| 5,000 rows | Confirms performance is acceptable |

The empty file and the header-only file are the two most likely to be forgotten
and the two most likely to throw.

---

## Questions to be able to answer

- Why does the parser not open the file itself?
- Why return an object rather than a list of strings?
- Why keep both a `List` and a `HashSet`? What is the cost of using only the list?
- Why must `normalise` strip the scheme before the `www.`?
- Why `ToLowerInvariant` rather than `ToLower`?
- Why is a row checked for validity before it is checked for duplication?
- What happens to a row with fewer columns than the mapped index, and why is that
  not an exception?
- What does the parser deliberately not handle, and why was that acceptable?

---

## After Stage 3

Stage 2 (XAML names and handlers), then Stage 4 (preview) calls `parse` for
real, holds the returned object in a `parseResult? lastResult` field, and enables
the import button. Stage 5 commits `lastResult.domains` through the existing
`domainRepo.addDomains`, which needs no changes.
