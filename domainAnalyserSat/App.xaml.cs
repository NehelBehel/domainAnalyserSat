using System.Configuration;
using System.Data;
using System.Windows;

namespace domainAnalyserSat
{
  
    //starts on login screen - initialise before any window opens 

    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) //override the onstartup method to run db initialisation 
        {
            Database.Initialise(); //build db first- then intialise window 
            base.OnStartup(e); //Claude fix- needed to overrides our startup so login form can be opened by WPF's onstartup 
        }




    }

}
