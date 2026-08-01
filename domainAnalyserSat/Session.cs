using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace domainAnalyserSat
{
    class Session

        

    {
        

        public long sessionId { get; set; } //sqlite integer column type to 64 bit long 
        public long userId { get; set; }

        public string name { get; set; } = string.Empty; //ensure non null by storing empty object 


        public DateTime createdAt { get; set; }






        
    }
}
