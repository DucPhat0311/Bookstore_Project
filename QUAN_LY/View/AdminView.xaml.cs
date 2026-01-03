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
            this.Loaded += AdminView_Loaded;
        }

        private void AdminView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminViewModel vm)
            {
                vm.StatusMessage = "Sẵn sàng";
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminViewModel vm && vm.SelectedAdmin != null)
            {
                vm.PasswordInput = ((PasswordBox)sender).Password;

                // GỌI VALIDATION THỦ CÔNG
                if (vm.SelectedAdmin.AdminId == 0) // Thêm mới
                {
                    vm.SelectedAdmin.Password = vm.PasswordInput;
                    vm.SelectedAdmin.ValidateProperty();
                }
            }
        }
    }
}