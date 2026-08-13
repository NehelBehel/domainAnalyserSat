using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows.Xps.Serialization;

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
                s.Substring(0, slash); //remove the path of the url
            }

            if (s.EndsWith('.'))
            {
                s.Substring(0, s.Length - 1); //remove the trailing dot if it exists
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
                if (part.Length < 1 || parts.Length > 63)
                {
                    return false;
                }

                //check if the part only contains letter + number + hyphen , if not return false 

                foreach (char c in part)
                {
                    if (c >= 'a' && c <= 'z' || (c >= '0' && c <= '9') || (c == '-'))
                    {
                        return true;

                    }
                    else
                    {
                        return false;
                    }


                }

                if (parts.StartsWith("-") || parts.EndsWith("-"))
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
    }



}