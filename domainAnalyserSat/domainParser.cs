using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Windows.Controls.Ribbon;
using System.Windows.Media.Animation;
using System.Windows.Xps.Serialization;
using System.Xml.XPath;

namespace domainAnalyserSat
{

    //Parses the domains
    //Only holds the logic 


    //purely fuctional, no reason to instatiate this class , thus use static 
    public static class domainParser
    {
        //First - attempt to guess the delimeter using the first line of the file 
        //only called when the user selects 'autoselect' in the delimter mapping
        public static char delimeterGuess(string line) //line is the domain line 
        {
            int commaCount = 0;
            int tabCount = 0;
            int semiCount = 0;

            //count the number of characters in the line, check if they match with the delim types 

            foreach (char c in line)
            {
                if (c == ',') commaCount++; //match is found, increment the count
                else if (c == '\t') tabCount++;
                else if (c == ';') semiCount++;



            }

            //nothing is found 
            if (commaCount == 0 && tabCount == 0 && semiCount == 0)
            {
                return '\0'; //return null characrer - claude fix since empty char is not valid 
            }

            if (commaCount >= tabCount && commaCount >= semiCount)
            {
                return ','; //comma is the most common 
            }
            else if (tabCount >= commaCount && tabCount >= semiCount)
            {
                return '\t'; //tab is the most common 
            }
            else
            {
                return ';'; //semi colon is the most common 
            }

        }

        //Turn one row into column.
        //If the delimter returns null, the whole row should be one line - accounts for txt files per line inputs 

        public static string[] splitLine(string line, char delimiter)
        {
            //delimiter is null, return the whoe line as coloumn
            if (delimiter == '\0')
            {
                return new string[] { line };

            }


            return line.Split(delimiter); //split the line
        }

        //normliase the string into its various sections 


        public static string normlaise(string line)
        {
            string s = line.Trim();
            s = s.ToLowerInvariant();

            //remove quotations marks 
          if  (s.StartsWith("\"")) s = s.Substring(1);              // strip leading quote
            if (s.EndsWith("\"")) s = s.Substring(0, s.Length - 1);  // strip trailing quote
            s = s.Trim();


            if (s.StartsWith("https://"))
            {
                s = s.Substring(8); //remove the https://

            }
            else if (s.StartsWith("http://"))
            {
                s = s.Substring(7); //remove the http://
            }

            if (s.StartsWith("www."))
            {
                s = s.Substring(4); //remove the www.
            }

            int slash = s.IndexOf('/'); //find the index of the first slash to remove path of url

            if (slash >= 0)
            {
               s=  s.Substring(0, slash); //remove the path of the url
            }

            if (s.EndsWith('.'))
            {
              s=   s.Substring(0, s.Length - 1); //remove the trailing dot if it exists
            }


            return s;

        }

        //validae the domain
        public static bool isValid(string domain)
        {
            //existance check 
            if (string.IsNullOrEmpty(domain))
            {
                return false;
            }

            //check if domain contains . , means tld is present 

            if (!domain.Contains('.'))
            {
                return false;
            }


            //dns max length is 253, range check to see if exceeded 

            if (domain.Length > 253)
            {
                return false;
            }
            //domain contains spaces, invalid
            if (domain.Contains(" "))
            {
                return false;
            }

            if (domain.Contains(".."))
            {
                return false;
            }

            //for each domain, split by . to store the name + tld 

            string[] parts = domain.Split('.');


            foreach (string part in parts)
            {
                //dns each laben must be betweeen 1-63 char
                if (part.Length < 1 || part.Length > 63)
                {
                    return false;
                }

                //check if the part only contains letter + number + hyphen , if not return false 

                foreach (char c in part)
                {
                    bool ok = (c >= 'a' && c <= 'z' || (c >= '0' && c <= '9') || (c == '-'));
                    if (!ok)
                    {
                        return false;
                    }
                    
                    


                }

                if (part.StartsWith("-") || part.EndsWith("-"))
                {
                    return false;

                }



            }

            //tld check

            string tld = parts[parts.Length - 1]; //gets the last item in the array which is the tld 
            if (tld.Length < 2)
            {
                return false;
            }

            //check if tld is purely alphabetic, if not return false
            foreach(char c in tld)
            {
                if (!(c >= 'a' && c <= 'z'))
                {
                    return false;
                }
            }

            //everthing is valid, return true 
            return true;

        
        
        }
   
    
    public static parseResults parse(string[] lines, char delimiter, bool hasHeader, int domColumn)
        {
            parseResults result  = new parseResults();


            //if lines are null or empty, return empty rather than crashing 
            if (lines ==null || lines.Length == 0)
            {
                return result;
            }

            //index of first data row 
            //If hasHeader = true, set start = 1 and split row 0 into result.headers
            int start = 0;
            if (hasHeader && lines.Length >=1)
            {
                result.headers = splitLine(lines[0], delimiter); //split the header row into columns 
                start = 1; //start from the second row
            }

            //check for duplicate domains 
            HashSet<string> seenDomains = new HashSet<string>(); //claude recommend Hashet as it is faster than list for checking duplicates

            //loop through line, check if empty if so skip and continue 

            for (int i = start; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrWhiteSpace(line)) //skip
                {
                    continue;
                }


                result.totalRows++; //increment the total rows 


                //valid line found 
                string[] parts = splitLine(line, delimiter);

                if(domColumn >= parts.Length)
                {
                    result.invalidCount++;
                    continue;

        

                }

                //ensure the row meets the mapped amt of columns 
                string potential = normlaise(parts[domColumn]);
                if (!isValid(potential))
                {
                    result.invalidCount++;
                    continue;

                }

                //Duplicate found, increment 
                if (!seenDomains.Add(potential))
                {
                    result.duplicateCount++;
                        continue;
                }

                //passed 
                result.domains.Add(potential);
                result.validCount++;

            }


            return result;  








        }







    
    
    }

   

}