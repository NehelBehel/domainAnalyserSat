using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace domainAnalyserSat
{
    public static class domainrRepo
    {
        //insert all the domains from that session 
       

        //int return : counts the number of rows insertted 
        //session id serves as foreign key which matches domain to session 
        public static int addDomains(long sessionId, IEnumerable<string>  domains) //IEnumerable since string is iterated as char (conversion errror) - claude fix 
        {
            using SqliteConnection connection = Database.GetConnection(); //create connection - using will close the connection when the method ends 
            using SqliteTransaction transaction = connection.BeginTransaction();
            using SqliteCommand command = new SqliteCommand(
                "INSERT INTO domains (sessionId, domain, createdAt) VALUES (@sessionId, @domain, @createdAt);",
                connection, transaction);

            //delcare the 3 parameters 
            SqliteParameter pSession = command.Parameters.Add("@sessionId", SqliteType.Integer);
            SqliteParameter pDomain = command.Parameters.Add("@domain", SqliteType.Text);
            SqliteParameter pCreated = command.Parameters.Add("@createdAt", SqliteType.Text);

            string currDT = DateTime.UtcNow.ToString("o"); //timestamps the inserts 

            int count = 0; //count of the no.of rows inserted 

           foreach(string domain in domains)
            {

                pDomain.Value = domain;
                pSession.Value = sessionId;
                pCreated.Value = currDT;
                command.ExecuteNonQuery(); //runs the insert but returns no rows 
                count++; //increment 




            }


            transaction.Commit(); //commit the transcations 
            return count;


            
                



        }






    }
}
