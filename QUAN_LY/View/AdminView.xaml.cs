// QUAN_LY/View/AdminView.xaml.cs
using QUAN_LY.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace QUAN_LY.View
{
    public partial class AdminView : UserControl
    {
        public AdminView()
        {
            InitializeComponent();
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminViewModel vm && vm.SelectedAdmin != null)
            {
                vm.SelectedAdmin.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}