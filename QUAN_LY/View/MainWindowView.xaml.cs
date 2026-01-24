using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using QUAN_LY.ViewModel; 

namespace QUAN_LY
{
    
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
        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            
            Application.Current.Shutdown();
        }


        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
           
            App.CurrentUser = null;

           
            LoginWindowView loginWindow = new LoginWindowView();
            loginWindow.Show();

            
            this.Close();
        }

        // 1. Xử lý kéo thả cửa sổ khi giữ chuột vào thanh tiêu đề
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // 2. Nút Ẩn (Minimize)
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 3. Nút Phóng to / Thu nhỏ (Maximize / Restore)
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        // 4. Nút Đóng (Close)
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            // Hoặc dùng this.Close();
        }

}
}