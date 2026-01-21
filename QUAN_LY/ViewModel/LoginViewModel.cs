using QUAN_LY.Model;
using QUAN_LY.Services;
using QUAN_LY.Utilities; // Chứa RelayCommand
using QUAN_LY.View;
using System;
using System.Windows;
using System.Windows.Input;

namespace QUAN_LY.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AdminServiceSQL _adminService;

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        // Thêm biến này để khóa nút khi đang xử lý
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            _adminService = new AdminServiceSQL();
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(object obj)
        {
            // Chỉ cho bấm khi đã nhập User và không đang Loading
            return !string.IsNullOrWhiteSpace(Username) && !IsLoading;
        }

        private async void ExecuteLogin(object obj)
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty; // Xóa lỗi cũ

                // Gọi Service (Lúc này Password đã được gán từ Code Behind)
                var admin = await _adminService.LoginAsync(Username, Password);

                if (admin != null)
                {
                    // 1. Lưu session
                    App.CurrentUser = admin;

                    // 2. Mở màn hình chính TRƯỚC
                    MainWindowView mainWindow = new MainWindowView();
                    mainWindow.Show();

                    // 3. Tìm và đóng màn hình Login SAU
                    // (Dùng cách này an toàn hơn Application.Current.Windows[0])
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is LoginWindowView)
                        {
                            window.Close();
                            break;
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng!";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi hệ thống: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}