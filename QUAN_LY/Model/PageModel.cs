using System.Windows;
using System.Windows.Input;
using QUAN_LY.Utilities;
using QUAN_LY.Model;
using QUAN_LY.View;
using System.Linq; 

namespace QUAN_LY.ViewModel
{
    public class PageModel : BaseViewModel
    {

        private string _selectedView = "Home"; // Mặc định là Home
        public string SelectedView
        {
            get => _selectedView;
            set { _selectedView = value; OnPropertyChanged(); }
        }

        private object _currentViewModel;
        public object CurrentViewModel
        {
            get { return _currentViewModel; }
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        
        private string _userInitials;
        public string UserInitials
        {
            get => _userInitials;
            set { _userInitials = value; OnPropertyChanged(); }
        }

        private string _userFullName;
        public string UserFullName
        {
            get => _userFullName;
            set { _userFullName = value; OnPropertyChanged(); }
        }

        
        public Visibility DashboardVisibility =>
            (App.CurrentUser?.Role == App.Roles.SaleStaff) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility AdminTabVisibility =>
            (App.CurrentUser?.Role == App.Roles.SaleStaff) ? Visibility.Collapsed : Visibility.Visible;

        public ICommand NavigateCommand { get; }

        public PageModel()
        {
            NavigateCommand = new RelayCommand(ExecuteNavigate);

          
            LoadUserInfo();

           
            if (App.CurrentUser != null && App.CurrentUser.Role == App.Roles.SaleStaff)
            {
                CurrentViewModel = new PosViewModel();
                SelectedView = "SellBook";
            }
            else
            {
                CurrentViewModel = new HomeViewModel();
                SelectedView = "Home";
            }
        }

       
        private void LoadUserInfo()
        {
            if (App.CurrentUser != null)
            {
                UserFullName = App.CurrentUser.Name; 
                UserInitials = GetInitials(App.CurrentUser.Name);
            }
            else
            {
                UserInitials = "G";
                UserFullName = "Guest";
            }
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "AD";

            
            var parts = fullName.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper(); 

            if (parts.Length >= 2)
            {
                
                var first = parts[0].Substring(0, 1);
                var last = parts[parts.Length - 1].Substring(0, 1);
                return (first + last).ToUpper();
            }

            return "AD";
        }

       
        private void ExecuteNavigate(object parameter)
        {
            string viewName = parameter as string;
            if (viewName == null) return;

            
            if (App.CurrentUser?.Role == App.Roles.SaleStaff)
            {
                if (viewName == "Home" || viewName == "RevenueStatistics" || viewName == "Inventory" || viewName == "Admin")
                {
                    MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            SelectedView = viewName;

            switch (viewName)
            {
                case "Home":
                    CurrentViewModel = new HomeViewModel();
                    break;
                case "SellBook":
                    CurrentViewModel = new PosViewModel();
                    break;
                case "Order":
                    CurrentViewModel = new OrderViewModel();
                    break;
                case "Book":
                    CurrentViewModel = new BookViewModel();
                    break;
                case "Inventory":
                    CurrentViewModel = new InventoryViewModel();
                    break;
                case "RevenueStatistics":
                    CurrentViewModel = new RevenueStatisticsViewModel();
                    break;
                case "Admin":
                    CurrentViewModel = new AdminViewModel();
                    break;
            }
        }
    }
}