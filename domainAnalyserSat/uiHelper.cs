using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace domainAnalyserSat
{
    
    //Satic utility class to display error messages on the UI 
    //This is shared across windows 

    public static class UiHelper
    {
        public static void showError(TextBlock target, string message)
        {
            target.Text = message;
            target.Visibility = System.Windows.Visibility.Visible;
        }

        public static void clearError(TextBlock target)
        {
            target.Text = string.Empty;
            target.Visibility = System.Windows.Visibility.Collapsed;


        }







}
}
