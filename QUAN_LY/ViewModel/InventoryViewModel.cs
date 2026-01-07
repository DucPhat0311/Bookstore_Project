using QUAN_LY.Model;
using QUAN_LY.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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

        // ===== CHỌN LỰA CÁI NHÀ XUẤT BẢN VÀ SÁCH ===========
        private Publisher _selectedPublisher;
        public Publisher SelectedPublisher { get => _selectedPublisher; set { _selectedPublisher = value; OnPropertyChanged(); FilterBooks(); } }

        private Book _selectedBook;
        public Book SelectedBook
        {
            get => _selectedBook;
            set { _selectedBook = value; OnPropertyChanged(); } // Cập nhật để hiện ảnh
        }

        private string _inputQty = "0";
        public string InputQty { get => _inputQty; set { _inputQty = value; OnPropertyChanged(); } }

        private string _inputPrice = "0";
        public string InputPrice { get => _inputPrice; set { _inputPrice = value; OnPropertyChanged(); } }

        // Tính tổng tiền của cả phiếu nhập
        private decimal _totalCost;
        public decimal TotalCost { get => _totalCost; set { _totalCost = value; OnPropertyChanged(); } }

        //===== BẮT LỖI NHẬP LIỆU ===========
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

        public InventoryViewModel()
        {
            LoadInitialData();

            // NÚT TẠO PHIẾU MỚI: Reset toàn bộ form và bảng bên phải
            NewReceiptCommand = new RelayCommand<object>((p) => {
                ImportItems.Clear();
                SelectedBook = null;
                InputQty = "0";
                InputPrice = "0";
                TotalCost = 0;
            });
            // Xem lịch sử nhập kho 
            ToggleHistoryCommand = new RelayCommand<object>((p) => {
                IsHistoryView = !IsHistoryView;
                if (IsHistoryView) LoadHistory();
            });
            // Thêm vào dnah sáhc
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
             // Xác nhận nhập kho
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
                    MessageBox.Show("Nhập kho thành công!");
                    NewReceiptCommand.Execute(null);
                }
                catch (Exception ex) { MessageBox.Show("Lỗi lưu DB: " + ex.Message); }
            }, (p) => ImportItems.Count > 0);
        }
        // Cái này dùng để lọc theo nhà xuất bản đã chọn ở trên
        private void FilterBooks()
        {
            FilteredBookList.Clear();
            if (SelectedPublisher == null) return;
            var list = _db.Books.Where(x => x.PublisherId == SelectedPublisher.Id).ToList();
            foreach (var b in list) FilteredBookList.Add(b);
        }

        // như cái tên
        private void LoadHistory()
        {
            ImportHistory.Clear();
            var data = (from d in _db.ImportDetails
                        join r in _db.ImportReceipts on d.importId equals r.Id
                        join b in _db.Books on d.BookId equals b.Id
                        select new { r.ImportDate, b.Title, d.quantity, d.importPrice }).OrderByDescending(x => x.ImportDate).ToList();
            foreach (var i in data) ImportHistory.Add(i);
        }

        // Load danh sách nhà xuất bản ban đầu
        private void LoadInitialData()
        {
            var pubs = _db.Publishers.ToList();
            foreach (var p in pubs) PublisherList.Add(p);
        }
    }
}