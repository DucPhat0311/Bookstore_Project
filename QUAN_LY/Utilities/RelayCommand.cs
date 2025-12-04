using System;
using System.Windows.Input;

namespace QUAN_LY.Utilities
{
    // Lớp triển khai ICommand cho MVVM
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        // Constructor cho Command luôn được thực thi
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        // Constructor cho Command có điều kiện thực thi
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Sự kiện xảy ra khi trạng thái CanExecute thay đổi
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Kiểm tra xem Command có thể thực thi được không
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Thực thi Command
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}