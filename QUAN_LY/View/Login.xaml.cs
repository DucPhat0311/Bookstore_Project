using System.Windows;
using System.Windows.Controls;

namespace QUAN_LY.View
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            // Kiểm tra đăng nhập đơn giản
            if (username == "admin" && password == "admin123")
            {
                // Tạo và hiển thị MainWindow
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Đóng cửa sổ login hiện tại
                CloseLoginWindow();
            }
            else
            {
                txtError.Text = "Tên đăng nhập hoặc mật khẩu không đúng!";
                txtError.Visibility = Visibility.Visible;
            }
        }

        private void CloseLoginWindow()
        {
            // Tìm và đóng cửa sổ chứa trang Login
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Content is Login)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}