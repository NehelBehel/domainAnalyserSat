using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
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

        //trade marks terms 
        //Harcoded currently due to scope rescrictions - future updates should either store in a large greater dataset (externally) or check through an api
        //Claude supplied the list 
        private static readonly string[] trademarkTerms =
        {
             // Tech & software
            "google", "apple", "microsoft", "amazon", "facebook", "meta", "instagram", "whatsapp",
            "youtube", "netflix", "spotify", "twitter", "tiktok", "snapchat", "linkedin", "pinterest",
            "reddit", "twitch", "discord", "zoom", "slack", "dropbox", "adobe", "oracle", "ibm",
            "intel", "nvidia", "cisco", "samsung", "huawei", "xiaomi", "sony", "panasonic", "dell",
            "lenovo", "asus", "nokia", "motorola", "qualcomm", "salesforce", "paypal", "stripe",
            "ebay", "alibaba", "tencent", "baidu", "uber", "lyft", "airbnb", "tesla", "spacex",
            "openai", "anthropic", "github", "gitlab", "wordpress", "shopify", "cloudflare", "mozilla",
            "firefox", "android", "windows", "xbox", "playstation", "nintendo",

            // Retail & ecommerce
            "walmart", "target", "costco", "kroger", "tesco", "aldi", "lidl", "ikea", "homedepot",
            "lowes", "bestbuy", "macys", "nordstrom", "zara", "uniqlo", "primark", "sephora", "ulta",

            // Food & beverage
            "cocacola", "pepsi", "nestle", "mcdonalds", "burgerking", "kfc", "subway", "starbucks",
            "dunkin", "wendys", "dominos", "pizzahut", "chipotle", "redbull", "monster", "gatorade",
            "heineken", "budweiser", "corona", "guinness", "hershey", "cadbury", "oreo", "kelloggs",
            "kraft", "danone", "unilever", "doritos", "pringles", "nutella",

            // Automotive
            "toyota", "honda", "ford", "chevrolet", "bmw", "mercedes", "audi", "volkswagen", "porsche",
            "ferrari", "lamborghini", "maserati", "nissan", "mazda", "subaru", "hyundai", "jeep",
            "dodge", "chrysler", "cadillac", "lexus", "jaguar", "landrover", "volvo", "bentley",
            "rollsroyce", "bugatti", "harley", "ducati", "kawasaki", "yamaha",

            // Fashion & luxury
            "nike", "adidas", "puma", "reebok", "underarmour", "newbalance", "converse", "vans",
            "gucci", "prada", "versace", "armani", "chanel", "dior", "louisvuitton", "hermes",
            "burberry", "fendi", "balenciaga", "rolex", "omega", "cartier", "tiffany", "ralphlauren",
            "tommyhilfiger", "calvinklein", "levis", "lacoste",

            // Entertainment & media
            "disney", "pixar", "marvel", "warnerbros", "universal", "paramount", "hbo", "espn",
            "cnn", "bbc", "hulu", "pandora", "ticketmaster", "lego", "mattel", "hasbro", "barbie",
            "pokemon",

            // Finance
            "visa", "mastercard", "amex", "americanexpress", "jpmorgan", "goldmansachs", "citibank",
            "hsbc", "barclays", "wellsfargo", "bankofamerica", "capitalone", "coinbase", "binance",
            "robinhood", "fidelity", "vanguard", "blackrock",

            // Travel & airlines
            "delta", "united", "americanairlines", "southwest", "emirates", "qatarairways",
            "lufthansa", "ryanair", "easyjet", "britishairways", "marriott", "hilton", "hyatt",
            "expedia", "tripadvisor",

            // Consumer goods, health & industrial
            "gillette", "colgate", "crest", "dove", "pampers", "huggies", "loreal", "maybelline",
            "revlon", "pfizer", "bayer", "philips", "bosch", "dyson", "gopro", "fitbit", "garmin",
            "canon", "nikon", "kodak", "shell", "chevron",

            // Sports leagues & events
            "nfl", "nba", "mlb", "nhl", "fifa", "uefa", "olympics", "wimbledon", "nascar", "ufc",
            "wwe", "premierleague"
        };




    

      

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
          //  using (SqliteCommand drop = new SqliteCommand("DROP TABLE IF EXISTS domains;", connection))
             //   drop.ExecuteNonQuery();



            string createDomains = @"
               CREATE TABLE IF NOT EXISTS domains(
                    domainId INTEGER PRIMARY KEY,
                    sessionId   INTEGER NOT NULL,
                    domain     TEXT NOT NULL,   
                    createdAt TEXT NOT NULL,
                    FOREIGN KEY (sessionId) REFERENCES sessions(sessionId)

            );";


          

            using SqliteCommand command2 = new SqliteCommand(createDomains, connection);
            command2.ExecuteNonQuery();

            //trademark checks 
            //create tables to store trademarked domain data 
            //case sensitive + unique to avoid duplicates 
            string createTrademarks = @"
                    CREATE TABLE IF NOT EXISTS trademarks(
                    trademarkId INTEGER PRIMARY KEY,
                    term TEXT NOT NULL UNIQUE COLLATE NOCASE


               );";


            using SqliteCommand command3 = new SqliteCommand(createTrademarks, connection);
            command3.ExecuteNonQuery();

            //seed trademark dadata
            using SqliteTransaction seedData = connection.BeginTransaction();
            //ignore skips terms that cause an error 
            using SqliteCommand seed = new SqliteCommand ("INSERT OR IGNORE INTO trademarks (term) VALUES (@term);", connection, seedData);

            //Declare the term parameter, loop into the array . 
            //Assign the value , insert and then commita after all inserts are done

            SqliteParameter pTerm = seed.Parameters.Add("@term", SqliteType.Text);
            foreach (string term in trademarkTerms)
            {
                pTerm.Value = term;
                seed.ExecuteNonQuery();
            }


            seedData.Commit();
          
           


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
