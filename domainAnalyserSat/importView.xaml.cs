using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace domainAnalyserSat
{
   
   

    public partial class importView : UserControl
    {


        private string selectedFilePath;//Full path of the chosen file 


        public string Data { get; set; }
        public importView()
        {
            InitializeComponent();


           










        }

        //Allows the user to browse files and make selection 
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {

            //Open file dialogue box 
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Domain lists (*.csv;*.txt)|*.csv;*.txt|All files (*.*)|*.*" //claude generated this filter 
            };
           
            if (dialog.ShowDialog() == true)
            {
                setSelectedFile(dialog.FileName);
               
            }
            

            
        }

        private void btnStartImport_Click(object sender, RoutedEventArgs e)
        {
            //check if user is signed in 
            if (appState.currentUserId == 0)
            {
                
                UiHelper.showError(lblError, "No user is signed in. Please log in before importing a file.");
                return;
            }

            //check for file selctions
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                UiHelper.showError(lblError, "No file selected. Please select a file to import.");
                return;
            }


            //Parse the selected file 
            //curent values are placeholders until fed from the UI

            string[] lines = System.IO.File.ReadAllLines(selectedFilePath);
            parseResults result = domainParser.parse(lines, ',', hasHeader: true, domColumn: 0);

            

            //name session name afte the source file 
            //couldnt figure this part out - claude helped 
            string sessionName = selectedFilePath =="" ? "Manual Entry" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : System.IO.Path.GetFileNameWithoutExtension(selectedFilePath) + "" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //create session only if this is the first import of the sesion

            if (appState.currentSessionId == 0)
            {
                appState.currentSessionId = sessionRepo.createSession(appState.currentSessionId, sessionName);
            }


            //write the domains
            int inserted = domainRepo.addDomains(appState.currentSessionId, result.domains);

            //progress the stepper to the next step 
        }

        //Show the chosen file
        //Put in its own method as it needs to be called by both file input locatiions (drag + manual selection)
        //TO DO: Add reading/ parsing of the file 
        private void setSelectedFile(string path)
        {

            selectedFilePath = path; //keep full path for later use 

            txtSelectedFile.Text = System.IO.Path.GetFileName(path); //take the full path and return file name 

            // --- TEMP Stage-3 parser test — DELETE once Stage 4 exists --- (claude helped )
            string[] lines = System.IO.File.ReadAllLines(path);
            parseResults r = domainParser.parse(lines, ',', hasHeader: true, domColumn: 0);
            MessageBox.Show(
                $"total={r.totalRows}  valid={r.validCount}  invalid={r.invalidCount}  dup={r.duplicateCount}\n\n"
                + string.Join("\n", r.domains));



        }


        //Allows the user to drag filees into desingated zone 

        private void importField_DragOver(object sender, DragEventArgs e)
        {
           

            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) //if user is dragging file, set to true, otherwise false 
                ? DragDropEffects.Copy //true- windows will show + copy cursor signalling to the user 
                : DragDropEffects.Move; //no drop cursor is shwon 

        }
        
        
        private void importField_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length >0)
            {
                //if data returns string, test pass,  result is stored into var files 
                //if null or not string, test returns false 






                    //grab file path
                    string selectedFilePath = files[0];

                    //Display 
                    MessageBox.Show(System.IO.Path.GetFileName(selectedFilePath));

                

            }





        }








        
    }











}
