using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QUAN_LY.ViewModel;

namespace QUAN_LY.View
{
    /// <summary>
    /// Interaction logic for RevenueStatisticsView.xaml
    /// </summary>
    public partial class RevenueStatisticsView : UserControl
    {
        public RevenueStatisticsView()
        {
            InitializeComponent();
            // Loại bỏ dòng này để tránh ghi đè không cần thiết (DataContext đã được set trong XAML nếu cần)
            // this.DataContext = new QUAN_LY.ViewModel.RevenueStatisticsViewModel();
        }
    }
}