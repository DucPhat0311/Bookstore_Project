using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QUAN_LY.View
{
    public enum MessageBoxType
    {
        Info,          
        Success,        
        Warning,        
        Error,         
        Confirmation    
    }

    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox()
        {
            InitializeComponent();
        }

  
        public static bool? Show(string message, string title = "Thông báo", MessageBoxType type = MessageBoxType.Info)
        {
            CustomMessageBox msgBox = new CustomMessageBox();
            msgBox.Setup(message, title, type);
            return msgBox.ShowDialog();
        }

     
        private void Setup(string message, string title, MessageBoxType type)
        {
            txtMessage.Text = message;
            txtTitle.Text = title;

          
            var primaryColor = (SolidColorBrush)FindResource("PrimaryColor");
            var accentColor = (SolidColorBrush)FindResource("AccentColor");   
            var successColor = (SolidColorBrush)FindResource("SuccessColor"); 
            var dangerColor = (SolidColorBrush)FindResource("DangerColor");   
            var warningColor = new SolidColorBrush(Color.FromRgb(243, 156, 18));

       
            btnCancel.Visibility = Visibility.Collapsed;

            switch (type)
            {
                case MessageBoxType.Success:
                    iconPath.Data = Geometry.Parse("M9,20.42L2.79,14.21L5.62,11.38L9,14.77L18.88,4.88L21.71,7.71L9,20.42Z");
                    iconPath.Fill = successColor;
                    btnOK.Background = successColor;
                    btnOK.Content = "Tuyệt vời";
                    break;

                case MessageBoxType.Error:
                    iconPath.Data = Geometry.Parse("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z");
                    iconPath.Fill = dangerColor;
                    btnOK.Background = dangerColor;
                    btnOK.Content = "Đóng";
                    break;

                case MessageBoxType.Warning:
                    iconPath.Data = Geometry.Parse("M12,2L1,21H23M12,6L19.53,19H4.47M11,10V14H13V10M11,16V18H13V16");
                    iconPath.Fill = warningColor;
                    btnOK.Background = warningColor;
                    btnOK.Content = "Đã hiểu";
                    break;

                case MessageBoxType.Confirmation:
                    iconPath.Data = Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L18,9.5L16.59,8.09L11,13.67L7.91,10.59L6.5,12L11,16.5Z");
                    iconPath.Fill = primaryColor;
                    btnOK.Background = primaryColor;
                    btnOK.Content = "Đồng ý";
                    btnCancel.Visibility = Visibility.Visible;
                    break;

                case MessageBoxType.Info:
                default:
                    iconPath.Data = Geometry.Parse("M12,2A10,10 0 1,0 22,12A10,10 0 0,0 12,2M12,17A1,1 0 1,1 13,16A1,1 0 0,1 12,17M12,13A1,1 0 0,1 11,12V7A1,1 0 0,1 13,7V12A1,1 0 0,1 12,13Z");
                    iconPath.Fill = accentColor;
                    btnOK.Background = accentColor;
                    btnOK.Content = "OK";
                    break;
            }
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}