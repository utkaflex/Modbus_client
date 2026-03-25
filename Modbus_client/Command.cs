using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Modbus_client
{
    public class Command : ICommand
    {
        private Action _execute;

        public event EventHandler CanExecuteChanged;

        public Command(Action execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
