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
            if (username.Length < 3)
            {
                UiHelper.showError(lblError, "Username must be at least 3 characters.");
                return;
            }

            if (password.Length < 8)
            {
                UiHelper.showError(lblError, "Password must be at least 8 characters.");
                return;
            }

            if (password != confirm)
            {
                UiHelper.showError(lblError, "Passwords do not match.");
                return;
            }

            UiHelper.clearError(lblError);


            // --- TODO: database step (next task) ---
            //   1. Check SQLite for an existing row with this username.
            //        If found: showError("That username is already taken."); return;
            //   2. Hash the password (bcrypt or argon2 — never store plaintext).
            //   3. INSERT a user row: username, password_hash, created_at = now.
            //   4. On success: return the user to the login screen (see below)

            //Login Success
            returnToLogin();
           
        }

        private void btnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            returnToLogin();
        }


        //Open the loogin window 
        //Ensure the new window is displayed before this one is closed 
        private void returnToLogin()
        {
            LoginWIndow loginWindow = new LoginWIndow();
            loginWindow.Show();
            this.Close();
        }







    }
}
