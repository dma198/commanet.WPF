using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;

namespace commanet.WPF
{
    public class Command : ICommand
    {
        private Action<object?>? OnExecute;
        private Action? OnBeforeExecute;
        private Action? OnAfterExecute;
        private Func<object, bool>? OnCanExecute;
        private Action<Exception>? OnExecuteException;
        public Command(Action<object?>? Execute, Func<object, bool>? CanExecute, 
                       Action? BeforeExecute, Action? AfterExecute,  Action<Exception>? ExecuteException)
        {
            OnExecute = Execute;
            OnCanExecute = CanExecute;
            OnBeforeExecute = BeforeExecute;
            OnAfterExecute = AfterExecute;
            OnExecuteException = ExecuteException;
        }
        #region ICommand Interface

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (OnCanExecute == null) return true;
            return OnCanExecute(parameter);
        }

        private volatile bool execLock=false;
        public void Execute(object? parameter)
        {
            if(execLock)return;
            execLock=true;
            new Task(()=>{
                if(OnBeforeExecute != null)
                    Application.Current.Dispatcher.Invoke(OnBeforeExecute);
                try
                {
                    OnExecute?.Invoke(parameter);
                }
                catch(Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(OnExecuteException, ex);
                }
                if(OnAfterExecute != null)
                    Application.Current.Dispatcher.Invoke(OnAfterExecute);
                execLock=false;
            }).Start();
        }
        #endregion
    }
}
