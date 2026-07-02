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
   
    public partial class createAccount : Window
    {
        public createAccount()
        {
            InitializeComponent();
            txtUsername.Focus();

        }
    
        private void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = pwdPassword.Password;
            string confirm = pwdConfirm.Password;


            //Loal Valdiation 
            if (username.Length ==0 || password.Length ==0 || confirm.Length ==0)
            {
                //Show error 
                UiHelper.showError(lblError, "Please fill in all fields.");
                return;

            }

        }

        private void btnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            //Close this window and return to the login window
            LoginWIndow loginWindow = new LoginWIndow();
            loginWindow.Show();
            this.Close();
        }










    }
}
