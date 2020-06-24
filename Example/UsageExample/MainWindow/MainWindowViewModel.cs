using System;
using System.Windows;

using commanet.WPF;

namespace UsageExample
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string test = "Hello commanet.WPF";
        
        public string Test 
        {
            get => test;
            set{SetAndNotify(value);}
        }

        [DependsOf(nameof(Test))]
        public string Test2 {get;set;} = "Hello 2";

        [DependsOf(nameof(Test))]
        public string Test3 {get;set;} = "Hello 3";


        public Command TestCommand {get; set;}

        public void TestExecute(object _)
        {           
           System.Threading.Thread.Sleep(10000);
        }
        public void TestBeforeExecute()
        {
            Test2="Second property notified 1";
            Test3="Third property notified 1";
            Test="Doing Heavy Operation ...";
        }

        public void TestAfterExecute()
        {
            Test2="Second property notified 2";
            Test3="Third property notified 2";
            Test="Heavy Operation Completed";
        }

         public void TestException(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }


        private bool canExecuteCommand=true;
        public bool TestCanExecute(object _)
        {
            return canExecuteCommand;
        }
    }
}
