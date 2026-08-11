using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO.Packaging;
using System.Text;

namespace domainAnalyserSat
{
    static class trademarkCheck
    {
        //Flag domains containg a trademarked term
        //Databse is local only, thus terms are limited to only the seeded terms
        //Future expansion: API to check trademarks terms online 


        private const int minTermLenth = 3; //terms less than this usually hold little value as a domain 

        private static List<String> terms = new List<string>(); //rather than checking 5000 domains individually, load into a list and check at once 

        //load all the usable domains 
        //Public so it can be called from the import screen
        public static void load()
        {
            //required to clear the terms 
            terms.Clear();

            //connect to databse 
            using SqliteConnection connection = Database.GetConnection();
            using SqliteCommand command = new SqliteCommand(

                "SELECT term FROM trademarks WHERE LENGTH(term) >= @minLength", connection //removes the terms below the min length first 


                );

          command.Parameters.AddWithValue("@minLength", minTermLenth);          
            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                terms.Add(reader.GetString(0).ToLowerInvariant());//lowercase the terms 
                    
            }









        }
        
        //strip terms of any punctuations or whitespace to spot potential trademark
        //For example: Mi-cro-soft would be flagged as Microsoft

        public static string strip(string domain)
        {
            string label = domain.Trim().ToLowerInvariant(); //lowercase the domain + remove whitepsaces 

            //remove tld from the domain 

            int dot = label.IndexOf('.'); //find the index of the dot , tld will trail this dot so remove it 
            if (dot > 0) //only remve if the dot was actually found 
            {
                label = label.Substring(0, dot); //Keep the portion of domain before dot (removes tld)

               


            }

            return label.Replace("-", ""); //replace the hypens with empty string 
        }



        
        //Return true if 

        public static  bool isFlagged(string domain)
        {
            string label = strip(domain);

            foreach (string term in terms) //check if stripped domains contains any of the trademarked names 
            {
                if (label.Contains(term)) //trademakred term is found inside stippred domain 
                {
                    return true;
                }



               

            }




            return false; //safe



        }





    }
}
