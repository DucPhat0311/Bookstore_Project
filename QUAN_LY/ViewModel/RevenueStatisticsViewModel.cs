using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using QUAN_LY.Model;
using QUAN_LY.Utilities;

namespace QUAN_LY.ViewModel
{
    public class RevenueStatisticsViewModel : BaseViewModel
    {
        private BookStoreDbContext _dbContext;
        private DispatcherTimer _refreshTimer;

        // Số liệu tổng
        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(); }
        }

        private decimal _totalCapital;
        public decimal TotalCapital
        {
            get => _totalCapital;
            set { _totalCapital = value; OnPropertyChanged(); }
        }

        private decimal _totalProfit;
        public decimal TotalProfit
        {
            get => _totalProfit;
            set { _totalProfit = value; OnPropertyChanged(); }
        }

        // Biểu đồ đường (7 ngày gần ngất)
        private SeriesCollection _revenueProfitSeries;
        public SeriesCollection RevenueProfitSeries
        {
            get => _revenueProfitSeries;
            set { _revenueProfitSeries = value; OnPropertyChanged(); }
        }

        private string[] _labels;
        public string[] Labels
        {
            get => _labels;
            set { _labels = value; OnPropertyChanged(); }
        }

        // Formatter
        private Func<double, string> _formatter;
        public Func<double, string> Formatter
        {
            get => _formatter;
            set { _formatter = value; OnPropertyChanged(); }
        }

        // Bảng xếp hạng(dựa trên số lượng bán)
        private List<BookSales> _topSellingBooks;
        public List<BookSales> TopSellingBooks
        {
            get => _topSellingBooks;
            set { _topSellingBooks = value; OnPropertyChanged(); }
        }

        private List<BookSales> _leastSellingBooks;
        public List<BookSales> LeastSellingBooks
        {
            get => _leastSellingBooks;
            set { _leastSellingBooks = value; OnPropertyChanged(); }
        }

        public ICommand LoadDataCommand { get; }

        public RevenueStatisticsViewModel()
        {
            _dbContext = new BookStoreDbContext();
            LoadDataCommand = new RelayCommand(LoadData);
            Formatter = value => value.ToString("N0") + " VND";
            LoadData(null);

            // Timer 
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(30);
            _refreshTimer.Tick += (sender, e) => LoadData(null);
            _refreshTimer.Start();
        }

        private void LoadData(object parameter)
        {
            try
            {
                // Tính toán 
                TotalRevenue = _dbContext.Orders
                 .Where(o => o.Status == 1)
                 .Sum(o => o.TotalAmount) ?? 0; 

                TotalCapital = _dbContext.ImportReceipts
                    .Where(ir => ir.Status == 1)
                    .Sum(ir => ir.TotalCost);
                TotalProfit = TotalRevenue - TotalCapital;

                // Biểu đồ – hiển thị theo 7 ngày gần nhất
                var revenueData = new ChartValues<decimal>();
                var profitData = new ChartValues<decimal>();
                var labels = new List<string>();
                var now = DateTime.Now;

                for (int i = 6; i >= 0; i--)
                {
                    var day = now.AddDays(-i);
                    var dayStart = day.Date;
                    var dayEnd = day.Date.AddDays(1).AddTicks(-1);

                    // Tính doanh thu cho ngày
                    var rev = _dbContext.Orders
                        .Where(o => o.Status == 1 && o.OrderDate >= dayStart && o.OrderDate <= dayEnd)
                        .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                    revenueData.Add(rev);

                    // Tính lợi nhuận cho ngày
                    var totalRevBooksDay = _dbContext.OrderItems
                        .Join(_dbContext.Orders, oi => oi.OrderId, o => o.Id, (oi, o) => new { oi, o })
                        .Where(x => x.o.Status == 1 && x.o.OrderDate >= dayStart && x.o.OrderDate <= dayEnd)
                        .Join(_dbContext.Books, x => x.oi.BookId, b => b.Id, (x, b) => (decimal?)x.oi.Quantity * b.Price)
                        .Sum() ?? 0;

                    var totalImportCostDay = _dbContext.OrderItems
                        .Join(_dbContext.Orders, oi => oi.OrderId, o => o.Id, (oi, o) => new { oi, o })
                        .Where(x => x.o.Status == 1 && x.o.OrderDate >= dayStart && x.o.OrderDate <= dayEnd)
                        .Join(_dbContext.ImportDetails, x => x.oi.BookId, id => id.BookId, (x, id) => new { x.oi, x.o, id })
                        .Join(_dbContext.ImportReceipts, x => x.id.importId, ir => ir.Id, (x, ir) => new { x.oi, x.id, ir })
                        .Where(x => x.ir.Status == 1)
                        .Sum(x => (decimal?)x.oi.Quantity * x.id.importPrice) ?? 0;

                    var prof = totalRevBooksDay - totalImportCostDay;
                    profitData.Add(prof);

                    labels.Add(day.ToString("dd MMM yyyy"));
                }

                // Biểu đồ đường cong màu
                RevenueProfitSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Doanh thu",
                        Values = revenueData,
                        Stroke = System.Windows.Media.Brushes.Green,
                        Fill = System.Windows.Media.Brushes.LightGreen,
                        LineSmoothness = 0.3, 
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:N0} VND"
                    },
                    new LineSeries
                    {
                        Title = "Lợi nhuận",
                        Values = profitData,
                        Stroke = System.Windows.Media.Brushes.Orange,
                        Fill = System.Windows.Media.Brushes.LightSalmon,
                        LineSmoothness = 0.3, 
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:N0} VND"
                    }
                };
                Labels = labels.ToArray();

                // Top 5 
                TopSellingBooks = _dbContext.Books
                    .GroupJoin(_dbContext.OrderItems, b => b.Id, oi => oi.BookId, (b, ois) => new { Book = b, TotalSold = ois.Sum(oi => oi.Quantity) })
                    .Select(x => new BookSales { BookTitle = x.Book.Title, TotalSold = x.TotalSold ?? 0 })
                    .OrderByDescending(bs => bs.TotalSold)
                    .Take(5)
                    .ToList();

                LeastSellingBooks = _dbContext.Books
                    .GroupJoin(_dbContext.OrderItems, b => b.Id, oi => oi.BookId, (b, ois) => new { Book = b, TotalSold = ois.Sum(oi => oi.Quantity) })
                    .Select(x => new BookSales { BookTitle = x.Book.Title, TotalSold = x.TotalSold ?? 0 })
                    .OrderBy(bs => bs.TotalSold)
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu thống kê: {ex.Message}\n\nKiểm tra kết nối DB hoặc cấu trúc bảng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                TotalRevenue = 0;
                TotalCapital = 0;
                TotalProfit = 0;
                RevenueProfitSeries = new SeriesCollection();
                Labels = new string[0];
                TopSellingBooks = new List<BookSales>();
                LeastSellingBooks = new List<BookSales>();
            }
        }

        public class BookSales
        {
            public string BookTitle { get; set; }
            public int TotalSold { get; set; }
        }
    }
}