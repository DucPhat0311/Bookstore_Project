using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Utilities;
using QUAN_LY.View;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace QUAN_LY.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAdminService _adminService;

        public LoginViewModel(IAdminService adminService)
        {
            _adminService = adminService;
            LoginCommand = new RelayCommand(ExecuteLogin, CanLogin); 
        }


        private string _userNameError;
        public string UserNameError
        {
            get => _userNameError;
            set { _userNameError = value; OnPropertyChanged(); }
        }

        
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();

               
                if (!string.IsNullOrEmpty(ErrorMessage)) ErrorMessage = "";

              
                ValidateUserName();
            }
        }

       
        private void ValidateUserName()
        {
            if (string.IsNullOrEmpty(Username))
            {
                UserNameError = "Tên đăng nhập không được để trống";
            }
            else if (Username.Contains(" "))
            {
                UserNameError = "Tên đăng nhập không được chứa khoảng trắng";
            }
            else if (Username.Length < 3)
            {
                UserNameError = "Tên đăng nhập quá ngắn (tối thiểu 3 ký tự)";
            }
            
            else if (!Username.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                UserNameError = "Tên đăng nhập chứa ký tự không hợp lệ";
            }
            else
            {
                UserNameError = "";
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; set; }

       
        private bool CanLogin(object parameter)
        {
            return !IsLoading && string.IsNullOrEmpty(UserNameError) && !string.IsNullOrEmpty(Username);
        }

        private async void ExecuteLogin(object parameter)
        {
            if (IsLoading) return;

            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ thông tin";
                return;
            }

           
            if (!string.IsNullOrEmpty(UserNameError)) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var admin = await _adminService.LoginAsync(Username, password);

                if (admin != null)
                {
                    App.CurrentUser = admin;
                    var mainWindow = App.Current.Services.GetRequiredService<MainWindowView>();
                    var mainVM = App.Current.Services.GetRequiredService<PageModel>();
                    mainWindow.DataContext = mainVM;
                    mainWindow.Show();

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
                    ErrorMessage = "Sai tên đăng nhập hoặc mật khẩu";
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