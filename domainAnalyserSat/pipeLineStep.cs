using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace domainAnalyserSat
{

    public enum stepState {Locked, Active, Complete}; //One of three states enforced by the step. Locked = 0 as default 

    public class pipeLineStep : Control // Inheritence from control - Allows the appearance to be defined by a controltemplate in app.xaml 
    {
        //State of the pipneline
        public string stepNumber
        {

            get => (string)GetValue(stepNumberProperty);
            set => SetValue(stepNumberProperty, value);



        }

        //Use dependecny property to allow step number to bind to my templatebinding + style triggers can be accsesed 
        //Claude fix : previous use of auto property didnt allow for access to wpf properties 

        public static readonly DependencyProperty stepNumberProperty =  
            DependencyProperty.Register(nameof(stepNumber), typeof(string), typeof(pipeLineStep), new PropertyMetadata("")); //define intial value 



        //Tells the user what the stage is, 
        public string label
        {
            get => (string)GetValue(labelProperty);
            set => SetValue(labelProperty, value);
        }
        public static readonly DependencyProperty labelProperty =
            DependencyProperty.Register(nameof(label), typeof(string), typeof(pipeLineStep), //match property label not the control type Label
                new PropertyMetadata(""));




        //informs of the data for each stage 
        public string count
        {
            get => (string)GetValue(countProperty);
            set => SetValue(countProperty, value);
        }

        public static readonly DependencyProperty countProperty =
            DependencyProperty.Register(nameof(count), typeof(string), typeof(pipeLineStep),
                new PropertyMetadata(""));
        //enforces the order of the pipleine by displaying the current accessiblity/ progress of that stage 
        public stepState state
        {
            get => (stepState)GetValue(stateProperty);
            set => SetValue(stateProperty, value);


        }

        public static readonly DependencyProperty stateProperty =

            DependencyProperty.Register(nameof(state), typeof(stepState), typeof(pipeLineStep),
                new PropertyMetadata(stepState.Locked)); //string defaul wtih throw error, use enum default locked 
        



    }
}
