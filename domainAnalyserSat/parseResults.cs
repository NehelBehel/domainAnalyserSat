using System;
using System.Collections.Generic;
using System.Text;

namespace domainAnalyserSat
{
    //Parsed file results are stored here 


    public class parseResults
    {
        public List<string> domains { get; set; } = new List<string>();//List is resizeable, arrry is not,



        public string[] headers { get; set; } = Array.Empty<string>();

        public int totalRows { get; set; }
        public int validCount { get; set; }
        public int invalidCount { get; set; }
        public int duplicateCount { get; set; }




    }



    



    
}
