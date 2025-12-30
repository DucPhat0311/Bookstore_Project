using System.Windows;
using System.Windows.Input;

namespace QUAN_LY
{
    public partial class LoginWindowView : Window
    {
        public LoginWindowView()
        {
            InitializeComponent();

            // Focus vào username textbox
            txtUsername.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            // Kiểm tra đăng nhập đơn giản
            if (username == "admin" && password == "admin123")
            {
                // Tạo và hiển thị MainWindow
                MainWindowView mainWindow = new MainWindowView();
                mainWindow.Show();

                // Đóng cửa sổ login
                this.Close();
            }
            else
            {
                txtError.Text = "Tên đăng nhập hoặc mật khẩu không đúng!";
                txtError.Visibility = Visibility.Visible;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Cho phép kéo window
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }


    }
}