using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace domainAnalyserSat
{
    public class User //made public to be accessed so userepo can return 
    {
       public long userId { get; set; }
        public string username { get; set; } = string.Empty;

        public string passwordHash { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public DateTime? lastLogin { get; set; } //ensure this is null until users first lgoin










    }
}
