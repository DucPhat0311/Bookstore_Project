using System.Windows;
using System.Windows.Input;

namespace QUAN_LY
{
    public partial class LoginWindowView : Window
    {
        public LoginWindowView()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}