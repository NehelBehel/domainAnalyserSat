using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Microsoft.Data.Sqlite;

namespace domainAnalyserSat
{
    //Centralisd access point for the Sqlite DB. 
    //Locate file, create scheme and then hand out open connections 


    class Database

    {
        //Path to the database file


        //Checks for the local app data folder 
        //**Was hardcoded but realised since every save location is diferent, claude suggested using the Environment.SpecialFolder.LocalApplicationData
        private static readonly string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),  "domainAnalyserSat");

        //static readonly ensures no other class can change the path, and it is computed at runtime 
        private static readonly string dbPath = Path.Combine(folderPath, "satDB.db");
        
        //Tells Sqlite which file to pen 
        public static string connectionString = $"Data Source={dbPath}";

        //Called on app start 

        public static void Initialise()
        {
            Directory.CreateDirectory(folderPath); //Ensure the folder exists 

            //Creates connection object for the file - currently only in memory
            using SqliteConnection connection = new SqliteConnection(connectionString);

            connection.Open(); //creates file if not already exististng 

            //Pk auto assigns ids no autoincrement needed 
            //Collate nocase is used to make username case insensitive


            string createUsers = @"
                CREATE TABLE IF NOT EXISTS users(
                    userID INTEGER PRIMARY KEY, 
                    username TEXT NOT NULL UNIQUE COLLATE NOCASE,
                    passwordHash TEXT NOT NULL,
                    lastlogin TEXT, 
                    createdAt TEXT NOT NULL
                


            );";


            //wraps sqltext with the connection 
            using SqliteCommand command = new SqliteCommand(createUsers, connection);
            //Executes the command against the database
            command.ExecuteNonQuery();


            //user session table 

            string createSessions = @"
                    CREATE TABLE IF NOT EXISTS sessions(
                             sessionId INTEGER PRIMARY KEY,
                             userId    INTEGER NOT NULL,
                             name      TEXT,
                             createdAt TEXT NOT NULL,
                             FOREIGN KEY (userId) REFERENCES users(userID)
                );";

            using SqliteCommand command1 = new SqliteCommand(createSessions, connection);
            command1.ExecuteNonQuery();

            string createDomains = @"
               CREATE TABLE IF NOT EXISTS domains(
                    sessionId INTEGER PRIMARY KEY,
                    userId    INTEGER NOT NULL,
                    name      TEXT,
                    createdAt TEXT NOT NULL,
                    FOREIGN KEY (userId) REFERENCES users(userID)

            );";


            using SqliteCommand command2 = new SqliteCommand(createDomains, connection);
            command2.ExecuteNonQuery();
            


        }

        //Returns an open connection to the database
        public static SqliteConnection GetConnection()
        {
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }
                                                                                                                                                                                                                


    }
}
