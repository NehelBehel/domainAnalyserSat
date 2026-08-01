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

       //Show the chosen file
       //Put in its own method as it needs to be called by both file input locatiions (drag + manual selection)
       //TO DO: Add reading/ parsing of the file 
        private void setSelectedFile(string path)
        {

            selectedFilePath = path; //keep full path for later use 

            txtSelectedFile.Text = System.IO.Path.GetFileName(path); //take the full path and return file name 

           
             


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
