using System.Windows.Input;
using QUAN_LY.Utilities; 
using QUAN_LY.Model;

namespace QUAN_LY.ViewModel
{
    // Lớp ViewModel chính cho MainWindow, quản lý việc chuyển đổi View
    public class PageModel : BaseViewModel
    {
        private object _currentViewModel;
        // Thuộc tính mà ContentControl trong MainWindow.xaml sẽ bind tới
        public object CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        // Command để chuyển đổi View
        public ICommand NavigateCommand { get; }

        public PageModel()
        {
            // Khởi tạo HomeVM làm View mặc định khi ứng dụng mở
            CurrentViewModel = new HomeViewModel();

            // Khởi tạo Command
            NavigateCommand = new RelayCommand(ExecuteNavigate);
        }

        // Logic chuyển đổi ViewModel
        private void ExecuteNavigate(object parameter)
        {
            string viewName = parameter as string;

            if (viewName == null) return;

            // Kiểm tra và tạo ViewModel tương ứng
            switch (viewName)
            {
                case "Home":
                    CurrentViewModel = new HomeViewModel();
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
                default:
                    // Có thể thêm ViewModel cho Logout (nếu cần) hoặc chỉ đơn giản là thoát
                    break;
            }
        }
    }

   
}