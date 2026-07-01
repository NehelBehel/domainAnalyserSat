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

    public partial class LoginWIndow : Window
    {
        public LoginWIndow()
        {
            InitializeComponent();
            txtUsername.Focus();

        }

        // Helper to surface a validation / authentication message.
        private void showError(string message)
        {
            lblError.Text = message;
            lblError.Visibility = Visibility.Visible;
        }
        //btnLogin Event 
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usename = txtUsername.Text.Trim();
            string password = pwdPassword.Password;

            if (txtUsername.Text == "" || pwdPassword.Password == "") //Check if username or password is empty
            {
                showError("Please enter both username and password.");
                return;


                // TODO: verifyCredentials(username, password)
                //   1. Look up the user row in SQLite by username.
                //   2. Hash the entered password and compare to password_hash.
                //   3. On success: update last_login, open dashboardWindow, close this window.
                //   4. On failure: showError("Incorrect username or password.");
            }


        }

        // btnCreateAccount_Click — Event
        // Opens the account-creation flow.
        private void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            // TODO: open the create-account window.
        }



    }





}       
    
