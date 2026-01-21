using QUAN_LY.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace QUAN_LY
{
    public partial class LoginWindowView : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindowView()
        {
            InitializeComponent();

            
            _viewModel = new LoginViewModel();
            this.DataContext = _viewModel;

            
            txtUsername.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
           
            _viewModel.Password = txtPassword.Password;

          
            if (_viewModel.LoginCommand.CanExecute(null))
            {
                _viewModel.LoginCommand.Execute(null);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

       
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }
    }
}