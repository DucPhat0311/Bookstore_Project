using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using QUAN_LY.ViewModel; 

namespace QUAN_LY
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private bool IsMaximized = false;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (!IsMaximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;
                    IsMaximized = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    IsMaximized = true;
                }
            }
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Xác nhận đăng xuất
            MessageBoxResult result = MessageBox.Show("Bạn có muốn đăng xuất?",
                                                    "Xác nhận",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Đóng MainWindow
                this.Close();

                // Mở lại LoginWindow
                LoginWindowView loginWindow = new LoginWindowView();
                loginWindow.Show();
            }
        }
    }
}