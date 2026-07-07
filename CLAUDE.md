# CLAUDE.md — Domain Investment Analyser (VCE SAT 2026)

> Project context file for Claude Code. This captures the project's scope, architecture,
> conventions, and current state so any session starts fully informed.
>
> **Staleness note:** The "Current State of the Codebase" section reflects the last known
> working session. Always verify it against the actual files on disk before relying on it —
> the code is the source of truth, this document is the map.

---

## How to work on this project (READ FIRST)

This is a **VCE Software Development Unit 3 Outcome 2 SAT** (School-Assessed Task). It is
assessed individually, and the student (Neil) attends **weekly authentication interviews with
his teacher, Mr. Toet, that require full personal ownership of every line of code.**

Because of that, operate in an **advisory, read-only mode**:

- **Do not write, edit, or create files.** Stay in Plan Mode. Analyse the code, explain how it
  works, and propose changes as clearly-described suggestions the student will type himself.
- **Explain the reasoning, not just the fix.** For every suggestion, give the "why" in enough
  depth that the student could defend it in an interview. Assume the questions "why did you do
  it this way?" and "what would break if you didn't?" will be asked.
- **Match the student's existing conventions exactly** (naming, structure, style below) rather
  than introducing your own.
- **Don't introduce new libraries or patterns** without flagging the trade-off first. This
  project deliberately keeps its dependency surface small.
- Prefer complete, technically-grounded explanations over brief summaries.

---

## Project overview

**Domain Investment Analyser** — an offline C#/WPF desktop application that imports domain-name
lists (CSV/TXT), scores them with a deterministic weighted engine, ranks the results, and
exports reports. It helps a domain investor evaluate which domains are worth acquiring.

- **Platform:** Windows 10/11 desktop
- **Scale target:** up to ~5,000 domain records
- **Operation:** fully offline — no external APIs, no live market data, no network calls

---

## Hard scope boundaries (do NOT cross these)

These are fixed constraints from the SRS. Suggesting anything outside them is out of scope and
should be actively avoided:

- **Fully offline.** No external APIs, no live market/registrar data, no network calls.
- **Deterministic weighted scoring only.** No AI, no ML, no probabilistic models. Scores must
  be reproducible: same input + same weights → same output, every time.
- **Canonical four-factor scoring model** (see below) plus a risk penalty. No other factors.
- **Bundled libraries only.** The end user installs nothing beyond the app itself.
- **Local-only accounts.** No cloud auth, no OAuth. Local SQLite account store with bcrypt
  password hashing.

**Explicitly out of scope (never suggest these):** AI-discovered keywords, AI portfolio health
index, AI strategic/confidence ratings, AI-mapped anything, estimated resale value, comparable
historical sales, live keyword search volume. Some earlier UI mock-ups (Stitch-generated) show
these — they are being stripped/reframed and must not drive implementation.

> **Design-system tell:** the violet tertiary colour `#8B5CF6` is reserved for AI-styled
> features. Since AI features are out of scope, **violet should not appear anywhere in the
> current build.** If you see it, that's a scope leak.

---

## Tech stack & environment

- **Language / UI:** C# with WPF
- **Target framework:** `net10.0-windows`
- **Project namespace:** `domainAnalyserSat`
- **Nullable reference types:** enabled
- **Implicit usings:** enabled
- **Database:** SQLite via `Microsoft.Data.Sqlite` (bundled native engine — satisfies the
  no-installation constraint). DB file lives in `%LocalAppData%\DomainInvestmentAnalyser\`,
  NOT in the install directory.
- **Password hashing:** `BCrypt.Net-Next`
- **IDE:** Visual Studio
- **Verification tool:** DB Browser for SQLite
- **Version control:** Git / GitHub, VisualStudio `.gitignore` template. `.db`, `.db-shm`,
  `.db-wal` are excluded from tracking.

---

## Architecture & file structure

Layered separation between UI (windows), a static UI helper, models, security, and data access:

```
domainAnalyserSat/
├── App.xaml / App.xaml.cs          # Entry point + centralised theme resources + DB bootstrap
├── LoginWindow.xaml / .cs
├── CreateAccountWindow.xaml / .cs
├── Models/
│   └── User.cs
├── Security/
│   └── PasswordHasher.cs           # BCrypt wrapper (Hash / Verify)
├── Data/
│   ├── Database.cs                 # Connection + schema creation (CREATE TABLE)
│   └── UserRepository.cs           # CRUD for users (parameterised queries)
└── UiHelper.cs                     # Static ShowError / ClearError for centralised error display
```

**Key design decisions (interview-relevant):**
- Parameterised queries **everywhere** — SQL-injection defence.
- `UNIQUE COLLATE NOCASE` on the username column — case-insensitive uniqueness.
- Database stored in `%LocalAppData%` so a read-only install directory doesn't break writes.
- `UiHelper` is a **static helper**, chosen over an inheritance hierarchy, to keep the window
  classes clean. OOP is demonstrated meaningfully later in the **scoring engine**, not forced
  onto helper/utility classes.

---

## Naming conventions (match these exactly)

- **C# / namespace:** `domainAnalyserSat`, PascalCase types, camelCase locals.
- **WPF controls:** type-prefix camelCase — e.g. `txtUsername`, `pwdPassword`, `pwdConfirm`,
  `btnLogin`, `btnCreateAccount`, `lblError`.
- **Event handlers:** `<controlName>_Click` etc. **These must match between XAML and code-behind
  exactly** (a mismatch is a build error — see Known Issues).
- Naming must stay consistent across the folio too (data dictionary, IPO charts, pseudocode,
  object descriptions, mock-ups). Inconsistency across artefacts is an examiner flag.

---

## Design system — "Terminal Prime" (dark-only)

| Role | Value |
|---|---|
| Base surfaces | `#0C0F10` / `#111415` |
| Teal primary | `#2DD4BF` (hover `#57F1DB`) |
| Sunset orange secondary | `#F97316` / `#EC6A06` |
| Violet tertiary | `#8B5CF6` — **reserved for AI features → out of scope → must not appear** |
| Errors / negatives | Muted red |
| UI text font | Inter |
| Quantitative / scores / IDs font | JetBrains Mono |
| Corner radii | 4px |
| Borders | 1px, low-contrast |
| Tables | Zebra-striped, high-density, numeric columns right-aligned |

Theme lives centrally in `App.xaml` as brushes/fonts/control styles:
`TerminalTextBox`, `TerminalPassBox` (note: the key is `TerminalPassBox`, **not**
`TerminalPasswordBox`), `PrimaryButton`, `SecondaryButton`, `LinkButton`.

---

## Scoring model (canonical — do not add factors)

Four weighted factors plus a risk penalty, combined deterministically:

1. **Length** — shorter domains score higher (length sensitivity).
2. **Brandability** — pronounceability / memorability signal.
3. **TLD quality** — authority of the top-level domain.
4. **Keyword / SEO demand** — exact-match search demand.
5. **Risk penalty** — subtracted (e.g. trademark/risk flags).

Weights are seeded per-account at creation, loaded into memory at session start, edited in
memory, and written back to SQLite on save. This is where OOP should be demonstrated properly.

---

## Current state of the codebase (verify against disk — may be stale)

**Done:**
- WPF project scaffolded (App.xaml, solution, Git repo with VisualStudio `.gitignore`).
- Centralised Terminal Prime theme in `App.xaml`.
- `LoginWindow.xaml` and `CreateAccountWindow.xaml` built with the naming convention above.
- Static `UiHelper` with `ShowError` / `ClearError`.
- Backend layer: `Models/User.cs`, `Security/PasswordHasher.cs`, `Data/Database.cs`,
  `Data/UserRepository.cs`.
- Account creation flow wired end-to-end (INSERT with hashed password, uniqueness enforced).
- Persistence confirmed: `ExecuteNonQuery` auto-commits to disk.

**Known issues / open items:**
- **Handler mismatch (build error):** `CreateAccountWindow.xaml` declares
  `btnReturnToLogin_Click` but the code-behind defines `btnBackToLogin_Click`. Until this is
  reconciled the project won't compile — and Visual Studio will silently run the last good
  build, masking new changes. Verify a clean compile after any change.
- **Login verification flow: not yet implemented.** This is the immediate next task.

---

## Immediate next tasks (in order)

1. **Fix the `CreateAccountWindow` handler mismatch** so the solution compiles cleanly.
2. **Login verification flow** in `LoginWindow.xaml.cs`, requiring:
   - a `GetByUsername(string username)` method on `UserRepository` (add if absent),
   - a `PasswordHasher.Verify(entered, storedHash)` check,
   - a `last_login` timestamp UPDATE on successful authentication,
   - `UiHelper.ShowError` / `ClearError` on `lblError` for empty-field and bad-credential cases.
3. **Scoring engine** — the OOP centrepiece (four factors + risk penalty).
4. Remaining windows: dashboard, analysis results, domain evaluation, export, session mgmt.

---

## Verification practices

- After any change, **confirm the project actually compiled** — VS masks failures by running
  the last successful build.
- Use **DB Browser for SQLite** to inspect the DB directly.
- Test uniqueness by attempting a duplicate username.
- Keep folio artefacts (naming, weights, screen names) consistent with the code.
