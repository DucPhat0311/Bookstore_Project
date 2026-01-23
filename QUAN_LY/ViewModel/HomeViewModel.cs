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

        #region --- SEARCH PROPERTIES ---
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
        #endregion

        #region --- DASHBOARD PROPERTIES (MỚI THÊM) ---

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

        #endregion

        public HomeViewModel()
        {
            _bookService = new BookServiceSQL();
            BookList = new ObservableCollection<Book>();

            LoadDashboardStats();
        }

        public async void LoadDashboardStats()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var db = new BookStoreDbContext())
                    {
                        var today = DateTime.Today; 

                        decimal revenue = db.Orders
                                            .Where(x => x.OrderDate == today)
                                            .Sum(x => (decimal?)x.TotalAmount) ?? 0;

                        int ordersCount = db.Orders.Count(x => x.OrderDate == today);

                        int booksSold = (from o in db.Orders
                                         join d in db.OrderItems on o.Id equals d.OrderId
                                         where o.OrderDate == today
                                         select (int?)d.Quantity).Sum() ?? 0;

                        int lowStock = db.Books.Count(x => x.Quantity < 10);

                        TodayRevenue = string.Format("{0:N0} đ", revenue);
                        TodayOrders = ordersCount;
                        TodayBooksSold = booksSold;
                        LowStockCount = lowStock;
                    }
                }
                catch (Exception ex)
                {
                    TodayRevenue = "Lỗi";
                }
            });
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