using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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

        
        //btnLogin Event 
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = pwdPassword.Password;

            if (txtUsername.Text == "" || pwdPassword.Password == "") //Check if username or password is empty
            {
                UiHelper.showError(lblError, "Please fill in all fields.");
                return;


                // TODO: verifyCredentials(username, password)
                //   1. Look up the user row in SQLite by username.
                //   2. Hash the entered password and compare to password_hash.
                //   3. On success: update last_login, open dashboardWindow, close this window.
                //   4. On failure: showError("Incorrect username or password.");
            }
            
            UiHelper.clearError(lblError);

            //
            User? user = UserRepo.GetByUsername(username);

            if (user == null|| !PasswordHasher.Verify(password, user.passwordHash))
            {
                UiHelper.showError(lblError, "Incorrect usernme or password ");
                return;



            }

            //Success - add an update for the last login time 

            //Todo: Open dashboard window 
            MessageBox.Show("Success");
           

            
            
            

        }
        

        private void inputChanged(object sender, RoutedEventArgs e)
        {
            UiHelper.clearError(lblError);
           

        }
        // btnCreateAccount_Click — Event
        // Opens the account-creation flow.
        private void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            // TODO: open the create-account win    dow.
            createAccount createAccountWindow = new createAccount();
            createAccountWindow.Show();
            this.Close();
        }

       

    }





}       
    
