using QUAN_LY.Model;
using QUAN_LY.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace QUAN_LY.ViewModel
{
    public class InventoryViewModel : BaseViewModel, IDataErrorInfo
    {
        private BookStoreDbContext _db = new BookStoreDbContext();

        public ObservableCollection<Publisher> PublisherList { get; set; } = new();
        public ObservableCollection<Book> FilteredBookList { get; set; } = new();
        public ObservableCollection<ImportDetail> ImportItems { get; set; } = new();
        public ObservableCollection<object> ImportHistory { get; set; } = new();

        private bool _isHistoryView = false;
        public bool IsHistoryView { get => _isHistoryView; set { _isHistoryView = value; OnPropertyChanged(); } }

        private Publisher _selectedPublisher;
        public Publisher SelectedPublisher { get => _selectedPublisher; set { _selectedPublisher = value; OnPropertyChanged(); FilterBooks(); } }

        private Book _selectedBook;
        public Book SelectedBook { get => _selectedBook; set { _selectedBook = value; OnPropertyChanged(); } }

        private string _inputQty = "0";
        public string InputQty { get => _inputQty; set { _inputQty = value; OnPropertyChanged(); } }

        private string _inputPrice = "0";
        public string InputPrice { get => _inputPrice; set { _inputPrice = value; OnPropertyChanged(); } }

        private decimal _totalCost;
        public decimal TotalCost { get => _totalCost; set { _totalCost = value; OnPropertyChanged(); } }

        // Bắt lỗi nhập liệu
        public string Error => null;
        public string this[string name]
        {
            get
            {
                if (name == nameof(InputQty) && (!int.TryParse(InputQty, out int q) || q <= 0)) return "SL > 0";
                if (name == nameof(InputPrice) && (!decimal.TryParse(InputPrice, out decimal p) || p <= 0)) return "Giá > 0";
                return null;
            }
        }

        public ICommand AddItemCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand NewReceiptCommand { get; set; }
        public ICommand ToggleHistoryCommand { get; set; }
        public ICommand ExportHistoryExcelCommand { get; set; }

        public InventoryViewModel()
        {
            LoadInitialData();
            // khởi tạo phiếu nhập mới
            NewReceiptCommand = new RelayCommand<object>((p) => {
                ImportItems.Clear();
                SelectedBook = null;
                InputQty = "0";
                InputPrice = "0";
                TotalCost = 0;
            });
            // khởi tạo phiếu nhập mới
            ToggleHistoryCommand = new RelayCommand<object>((p) => {
                IsHistoryView = !IsHistoryView;
                if (IsHistoryView) LoadHistory();
            });
            // thêm sách vào danh sách nhập
            AddItemCommand = new RelayCommand<object>((p) => {
                ImportItems.Add(new ImportDetail
                {
                    BookId = SelectedBook.Id,
                    BookTitle = SelectedBook.Title,
                    quantity = int.Parse(InputQty),
                    importPrice = decimal.Parse(InputPrice)
                });
                TotalCost = ImportItems.Sum(x => x.quantity * x.importPrice);
            }, (p) => SelectedBook != null && string.IsNullOrEmpty(this[nameof(InputQty)]) && string.IsNullOrEmpty(this[nameof(InputPrice)]));
            // lưu phiếu nhập vào DB
            SaveCommand = new RelayCommand<object>((p) => {
                try
                {
                    var receipt = new ImportReceipt
                    {
                        PublisherId = SelectedPublisher.Id,
                        ImportDate = DateTime.Now,
                        TotalCost = TotalCost
                    };
                    _db.ImportReceipts.Add(receipt);
                    _db.SaveChanges();

                    foreach (var item in ImportItems)
                    {
                        item.importId = receipt.Id;
                        _db.ImportDetails.Add(item);
                        var b = _db.Books.Find(item.BookId);
                        if (b != null) b.Quantity += item.quantity;
                    }
                    _db.SaveChanges();
                    MessageBox.Show("Nhập kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    NewReceiptCommand.Execute(null);
                }
                catch (Exception ex) { MessageBox.Show("Lỗi lưu DB: " + ex.Message); }
            }, (p) => ImportItems.Count > 0);

            ExportHistoryExcelCommand = new RelayCommand<object>((p) => ExportHistoryToExcel(), (p) => IsHistoryView && ImportHistory.Count > 0);
        }

        private void ExportHistoryToExcel()
        {
            // sử dụng api EPPlus xuất file Excel
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("QUAN_LY");

            SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = "LichSuNhapKho_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var pck = new OfficeOpenXml.ExcelPackage())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Lịch Sử Nhập Kho");

                        // Header
                        // Thiết lập tiêu đề cột
                        string[] headers = { "Ngày Nhập", "Tên Sách", "Số Lượng", "Đơn Giá", "Thành Tiền" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            // Tạo ô và định dạng
                            var cell = ws.Cells[1, i + 1];
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(26, 35, 126));
                            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }

                        // Dữ liệu
                        int row = 2;
                        // Duyệt từng item trong ImportHistory
                        foreach (dynamic item in ImportHistory)
                        {
                            // Gán giá trị vào từng cột
                            ws.Cells[row, 1].Value = item.ImportDate?.ToString("dd/MM/yyyy HH:mm");
                            ws.Cells[row, 2].Value = item.Title;
                            ws.Cells[row, 3].Value = item.quantity;
                            ws.Cells[row, 4].Value = item.importPrice;
                            ws.Cells[row, 5].Value = (decimal)(item.quantity ?? 0) * (decimal)(item.importPrice ?? 0);

                            ws.Cells[row, 4, row, 5].Style.Numberformat.Format = "#,##0";
                            row++;
                        }
                        // định dạng cột
                        ws.Cells.AutoFitColumns();
                        File.WriteAllBytes(sfd.FileName, pck.GetAsByteArray());
                        MessageBox.Show("Xuất file Excel thành công!");
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }
        // lọc sách theo nhà xuất bản được chọn
        private void FilterBooks()
        {
            FilteredBookList.Clear();
            if (SelectedPublisher == null) return;
            var list = _db.Books.Where(x => x.PublisherId == SelectedPublisher.Id).ToList();
            foreach (var b in list) FilteredBookList.Add(b);
        }
        // tải lịch sử nhập kho từ DB
        private void LoadHistory()
        {
            ImportHistory.Clear();
            var data = (from d in _db.ImportDetails
                        join r in _db.ImportReceipts on d.importId equals r.Id
                        join b in _db.Books on d.BookId equals b.Id
                        select new { r.ImportDate, b.Title, d.quantity, d.importPrice }).OrderByDescending(x => x.ImportDate).ToList();
            foreach (var i in data) ImportHistory.Add(i);
        }
        // tải dữ liệu ban đầu
        private void LoadInitialData()
        {
            try
            {
                var pubs = _db.Publishers.ToList();
                foreach (var p in pubs) PublisherList.Add(p);
            }
            catch { }
        }
    }
}