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
        //holds the most recent parse 
        private parseResults? lastResult;


        //read the item the user picked and return the delimiter character 

        private char resolveDelim(string[] lines)
        {
           switch (cmbDelimiter.SelectedIndex)
            {
                case 0: return ',';
                case 1: return '\t';
                case 2: return ';';
                default: //index is none of the above- this catches auto detect delimter and nothing is selcted at once 
                    foreach(string x in lines )
                        if (!string.IsNullOrWhiteSpace(x))
                    {
                            return domainParser.delimeterGuess(x);

                    }

                    return '\0'; //no delimter 
            }

           



        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            //check for a selected file
            if (string.IsNullOrEmpty(selectedFilePath)) //no file chosen 
            {
                UiHelper.showError(lblError, "No file selected.");
                return;
            }

            UiHelper.clearError(lblError); //clear error
           
            string[] lines = System.IO.File.ReadAllLines(selectedFilePath); //read the whole file into arrray 
            char delimiter = resolveDelim(lines);
            bool hasHeader = chkHeader.IsChecked== true;


            int domainColumn;
            if (cmbDomainCol.SelectedIndex >= 0)
            {
                domainColumn = cmbDomainCol.SelectedIndex; //uses selevted column
            }
            else
                domainColumn = 0; //default to first column


            //
            lastResult = domainParser.parse(lines, delimiter, hasHeader, domainColumn);
            

            //add the show coulm picker method  logic here 
            

        
        }
        //count the file columns, if only 1 , hide the picker
        //if the file columns >1 display with the column names 
        private void showColumnPicker(string[] lines, char delimier, bool hasHeader)
        {
            string? firstLine = null; //can be null

            foreach(string x in lines) //grab the first non blank line 
                if(!string.IsNullOrWhiteSpace(x))
                {
                    firstLine = x; //assign to the first line found 
                    break;//Stop at the first find 

                }

            string[] cols;

            if (firstLine == null)
            {
                cols = new string[0];
            }

            else
            {
                cols = domainParser.splitLine(firstLine, delimier);
            }

            //txt entry (txt) - no picker 

            if (cols.Length <= 1)
            {
                cmbDomainCol.Visibility = Visibility.Collapsed;
            } 
            
           
           //save the user seclections 
           int previous = cmbDomainCol.SelectedIndex;
            cmbDomainCol.Items.Clear();

            //loop for number of columns 
          for(int i = 0 ; i < cols.Length; i++)
            {

                //Add one option to the cmbobox list 
                //if the file has a header, add the header name
                //If no header, add a generic column + column number 

                cmbDomainCol.Items.Add(hasHeader ? cols[i] : "Column" + (i + 1)); //claude suggested use of  a ternary selection 

                    
            }

            //Check for the users previous selection 
            //If the user resets the selection, the users selection would be lost 
            //validate whether the previous selection is still valid through range check 

           
            if (previous >= 0 && previous < cols.Length)
            {
               
                cmbDomainCol.SelectedIndex = previous;
            }
            else
            {
               
                cmbDomainCol.SelectedIndex = 0;

            }

            cmbDomainCol.Visibility = Visibility.Visible; //set the column selections to visible



                

        }










    }











}
