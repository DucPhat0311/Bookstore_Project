using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Services;
using QUAN_LY.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows; 

namespace QUAN_LY.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly BookServiceSQL _bookService;

        private CancellationTokenSource _cancellationTokenSource;

        private ObservableCollection<Book> _bookList;
        public ObservableCollection<Book> BookList
        {
            get => _bookList;
            set { _bookList = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                OnSearchTextChanged();
            }
        }

        // Doanh thu
        private string _todayRevenue = "0 đ";
        public string TodayRevenue
        {
            get => _todayRevenue;
            set { _todayRevenue = value; OnPropertyChanged(); }
        }

        // Đơn hàng
        private int _todayOrders = 0;
        public int TodayOrders
        {
            get => _todayOrders;
            set { _todayOrders = value; OnPropertyChanged(); }
        }

        // Sách đã bán
        private int _todayBooksSold = 0;
        public int TodayBooksSold
        {
            get => _todayBooksSold;
            set { _todayBooksSold = value; OnPropertyChanged(); }
        }

        // Cảnh báo tồn kho
        private int _lowStockCount = 0;
        public int LowStockCount
        {
            get => _lowStockCount;
            set { _lowStockCount = value; OnPropertyChanged(); }
        }


        public HomeViewModel()
        {
            _bookService = new BookServiceSQL();
            BookList = new ObservableCollection<Book>();

            LoadDashboardStats();
        }

        public async void LoadDashboardStats()
        {
            try
            {
                var stats = await _bookService.GetDashboardStatsAsync();

                TodayRevenue = string.Format("{0:N0} đ", stats.Revenue);
                TodayOrders = stats.OrdersCount;
                TodayBooksSold = stats.BooksSold;
                LowStockCount = stats.LowStockCount;
            }
            catch (Exception ex)
            {
                TodayRevenue = "Lỗi";
            }
        }

        private async void OnSearchTextChanged()
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                BookList.Clear();
                return;
            }

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                await Task.Delay(500, token);
                var result = await _bookService.SearchBooksAsync(_searchText, token);
                BookList = new ObservableCollection<Book>(result);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
            }
        }
    }
}