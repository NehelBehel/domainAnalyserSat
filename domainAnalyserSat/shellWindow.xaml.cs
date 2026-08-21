using System;
using System.Collections.Generic;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Input;

namespace domainAnalyserSat
{
    /// <summary>
    /// Interaction logic for shellWindow.xaml
    /// </summary>
    public partial class shellWindow : Window
    {
        public shellWindow()
        {
            InitializeComponent();

            //land on dashboard as default 
            contentHost.Content = new workSpaceOV(); //load dashboard around the shellwindows 
            setActiveNav(btnNavDashboard); //set the dashboard nav item to active)

        }



        //Begin a new session, shows the import screen and updates the progress pipleline at the top 


        private void btnNewAnalysis_Click(object sender, RoutedEventArgs e)
        {
            appState.currentSessionId = 0; //new session default to 0
            contentHost.Content = new importView();


            pipelineStepper.Visibility = Visibility.Visible;
            clearSideBarActive();


            //reset the stepper so that a second analysis doesnt start on previous setp
            stepImport.state = stepState.Active;
            stepImport.count = "";
            stepValidate.state = stepState.Locked;
            stepAnalyse.state = stepState.Locked;



            

        }

        private void stepImport_Click(object sender, MouseButtonEventArgs e ) //
        {
            
           

            //import view 
            contentHost.Content = new importView();


        }


      
        private void btnNavDashboard_Click(object sender, RoutedEventArgs e)
        {
            contentHost.Content = new workSpaceOV();
            pipelineStepper.Visibility = Visibility.Collapsed; //
            setActiveNav(btnNavDashboard);
        }


        private void btnNavWatchlist_Click(object sender, RoutedEventArgs e)
        {
            pipelineStepper.Visibility = Visibility.Collapsed;
            setActiveNav(btnNavWatchlist);
            // contentHost.Content = new watchlistView();   when built
        }

        private void btnNavSession_Click(object sender, RoutedEventArgs e)
        {
            pipelineStepper.Visibility = Visibility.Collapsed;
            setActiveNav(btnNavSession);
            // contentHost.Content = new sessionView();      // when built
        }





        private void clearSideBarActive()
        {
            //Reset sidebar style back to defaults 


            foreach (var btn in new[] {btnNavDashboard, btnNavWatchlist, btnNavSession })
            {
                btn.Style = (Style)FindResource("sidebarNavBtn");
            }
        }



      
        
         
           private void btnTopImport_Click(object sender, RoutedEventArgs e) { }
           private void btnTopAnalysis_Click(object sender, RoutedEventArgs e) { }
           private void btnTopValidate_Click(object sender, RoutedEventArgs e) { }








        private void setActiveNav(Button active)
        {

            //reset all navigation items to default state
            //mark the curr active nav item 
            btnNavDashboard.Style = (Style)FindResource("sidebarNavBtn");
            btnNavWatchlist.Style = (Style)FindResource("sidebarNavBtn");
            btnNavSession.Style = (Style)FindResource("sidebarNavBtn");
            active.Style = (Style)FindResource("sidebarNavBtnActive");
            //(Style)FindResource("sidebarNavBtnActive");


        }





        public void markImportComplete(int count)
        {
            pipelineStepper.Visibility = Visibility.Visible; //ensure the stepper is showing first 
            stepImport.state = stepState.Complete; //
            stepImport.count = count.ToString();
            stepValidate.state = stepState.Active; //unlock and set active 




        }






    }
}
