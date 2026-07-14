using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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



        private void btnNavDashboard_Click(object sender, RoutedEventArgs e)
        {
            contentHost.Content = new workSpaceOV();
            setActiveNav(btnNavDashboard); //set the dashboard nav item to active)

        }
           private void btnNavWatchlist_Click(object sender, RoutedEventArgs e) { }

           private void btnNavSession_Click(object sender, RoutedEventArgs e) { }
           private void btnNewAnalysis_Click(object sender, RoutedEventArgs e) { }
           private void btnTopImport_Click(object sender, RoutedEventArgs e) { }
           private void btnTopAnalysis_Click(object sender, RoutedEventArgs e) { }
           private void btnTopValidate_Click(object sender, RoutedEventArgs e) { }





        private void setActiveNav(Button active)
        {

            //reset all navigation items to default state
            //mark the curr active nav item 
            btnNavDashboard.Style = (Style)FindResource("sidebarNavBtnActive");
            btnNavWatchlist.Style = (Style)FindResource("sidebarNavBtnActive");
            btnNavSession.Style = (Style)FindResource("sidebarNavBtnActive");
            active.Style = (Style)FindResource("sidebarNavBtnActive");

        }
    }
}
