using System.Windows.Input;
using QUAN_LY.Utilities; // Import ViewModelBase và RelayCommand

namespace QUAN_LY.ViewModel
{
    // Lớp ViewModel chính cho MainWindow, quản lý việc chuyển đổi View
    public class PageModel : ViewModelBase
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
            CurrentViewModel = new HomeVM();

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
                    CurrentViewModel = new HomeVM();
                    break;
                case "Order":
                    CurrentViewModel = new OrderVM();
                    break;
                case "BookManagement":
                    CurrentViewModel = new Book_ManagementVM();
                    break;
                case "Inventory":
                    CurrentViewModel = new InventoryVM();
                    break;
                case "RevenueStatistics":
                    CurrentViewModel = new Revenue_StatisticsVM();
                    break;
                default:
                    // Có thể thêm ViewModel cho Logout (nếu cần) hoặc chỉ đơn giản là thoát
                    break;
            }
        }
    }

    // ----------- Các ViewModels Con (Đã Xóa Phần Demo Bị Trùng Lặp) -----------
    // Các lớp HomeVM, OrderVM, v.v. đã được xóa khỏi file này để tránh lỗi CS0101.
    // Giả định các lớp này tồn tại trong các file riêng (ví dụ: HomeVM.cs, OrderVM.cs).
}