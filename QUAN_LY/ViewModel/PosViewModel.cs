using QUAN_LY.Model;
using QUAN_LY.Utilities;
using QUAN_LY.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace QUAN_LY.ViewModel
{
    // ==============================================================================
    // 1. CLASS CART ITEM
    // ==============================================================================
    public class CartItem : BaseViewModel
    {
        public Book Book { get; set; }

        public string Title => Book.Title;
        public string FullImageSource => Book.FullImageSource;
        public decimal Price => Book.Price;

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SubTotal));
            }
        }
        public decimal SubTotal => Price * Quantity;
    }

    // ==============================================================================
    // 2. POS VIEW MODEL
    // ==============================================================================
    public class PosViewModel : BaseViewModel
    {
        private BookStoreDbContext _db = new BookStoreDbContext();

        // --- DANH SÁCH DỮ LIỆU ---

        private ObservableCollection<Book> _productList;
        public ObservableCollection<Book> ProductList
        {
            get => _productList;
            set { _productList = value; OnPropertyChanged(); }
        }

        // List gốc để backup dữ liệu cho tìm kiếm
        private List<Subject> _originSubjectList;
        private List<Author> _originAuthorList;
        private List<Publisher> _originPublisherList;

        private ObservableCollection<Subject> _subjectList;
        public ObservableCollection<Subject> SubjectList { get => _subjectList; set { _subjectList = value; OnPropertyChanged(); } }

        private ObservableCollection<Author> _authorList;
        public ObservableCollection<Author> AuthorList { get => _authorList; set { _authorList = value; OnPropertyChanged(); } }

        private ObservableCollection<Publisher> _publisherList;
        public ObservableCollection<Publisher> PublisherList { get => _publisherList; set { _publisherList = value; OnPropertyChanged(); } }

        // --- LOGIC TÌM KIẾM TRONG COMBOBOX (SEARCH LIKE) ---

        private string _subjectSearchText;
        public string SubjectSearchText
        {
            get => _subjectSearchText;
            set
            {
                _subjectSearchText = value;
                OnPropertyChanged();
                if (string.IsNullOrEmpty(value)) SubjectList = new ObservableCollection<Subject>(_originSubjectList);
                else SubjectList = new ObservableCollection<Subject>(_originSubjectList.Where(x => x.Name.ToLower().Contains(value.ToLower())));
            }
        }

        private string _authorSearchText;
        public string AuthorSearchText
        {
            get => _authorSearchText;
            set
            {
                _authorSearchText = value;
                OnPropertyChanged();
                if (string.IsNullOrEmpty(value)) AuthorList = new ObservableCollection<Author>(_originAuthorList);
                else AuthorList = new ObservableCollection<Author>(_originAuthorList.Where(x => x.Name.ToLower().Contains(value.ToLower())));
            }
        }

        private string _publisherSearchText;
        public string PublisherSearchText
        {
            get => _publisherSearchText;
            set
            {
                _publisherSearchText = value;
                OnPropertyChanged();
                if (string.IsNullOrEmpty(value)) PublisherList = new ObservableCollection<Publisher>(_originPublisherList);
                else PublisherList = new ObservableCollection<Publisher>(_originPublisherList.Where(x => x.Name.ToLower().Contains(value.ToLower())));
            }
        }

        // --- FILTER SẢN PHẨM ---

        private Subject _selectedSubject;
        public Subject SelectedSubject { get => _selectedSubject; set { _selectedSubject = value; OnPropertyChanged(); FilterProducts(); } }

        private Author _selectedAuthor;
        public Author SelectedAuthor { get => _selectedAuthor; set { _selectedAuthor = value; OnPropertyChanged(); FilterProducts(); } }

        private Publisher _selectedPublisher;
        public Publisher SelectedPublisher { get => _selectedPublisher; set { _selectedPublisher = value; OnPropertyChanged(); FilterProducts(); } }

        private string _searchKeyword;
        public string SearchKeyword { get => _searchKeyword; set { _searchKeyword = value; OnPropertyChanged(); FilterProducts(); } }

        // --- GIỎ HÀNG ---

        public ObservableCollection<CartItem> CartItems { get; set; } = new ObservableCollection<CartItem>();

        private decimal _cartTotal;
        public decimal CartTotal { get => _cartTotal; set { _cartTotal = value; OnPropertyChanged(); } }

        // --- COMMANDS ---

        public ICommand AddToCartCommand { get; set; }
        public ICommand RemoveFromCartCommand { get; set; }
        public ICommand IncreaseQtyCommand { get; set; }
        public ICommand DecreaseQtyCommand { get; set; }
        public ICommand ClearCartCommand { get; set; }
        public ICommand ClearFilterCommand { get; set; }
        public ICommand CheckoutCommand { get; set; }
        public ICommand AddProductCommand { get; set; }

        // ==============================================================================
        // CONSTRUCTOR
        // ==============================================================================
        public PosViewModel()
        {
            LoadMetaData();
            FilterProducts();

            AddToCartCommand = new RelayCommand<Book>((book) =>
            {
                var existingItem = CartItems.FirstOrDefault(x => x.Book.Id == book.Id);
                if (existingItem != null)
                {
                    if (existingItem.Quantity < book.Quantity)
                    {
                        existingItem.Quantity++;
                        UpdateCartTotal();
                    }
                    else
                    {
                        CustomMessageBox.Show($"Kho chỉ còn {book.Quantity} cuốn. Không thể bán thêm.", "Hết hàng", MessageBoxType.Warning);
                    }
                }
                else
                {
                    if (book.Quantity > 0)
                    {
                        CartItems.Add(new CartItem { Book = book, Quantity = 1 });
                        UpdateCartTotal();
                    }
                    else
                    {
                        CustomMessageBox.Show("Sách này đã hết hàng.", "Thông báo", MessageBoxType.Info);
                    }
                }
            });

            IncreaseQtyCommand = new RelayCommand<CartItem>((item) =>
            {
                if (item.Quantity < item.Book.Quantity)
                {
                    item.Quantity++;
                    UpdateCartTotal();
                }
                else
                {
                    CustomMessageBox.Show("Đã đạt giới hạn tồn kho.", "Thông báo", MessageBoxType.Warning);
                }
            });

            DecreaseQtyCommand = new RelayCommand<CartItem>((item) =>
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    UpdateCartTotal();
                }
                else
                {
                    var ans = CustomMessageBox.Show($"Bạn có chắc muốn bỏ sách '{item.Title}' khỏi giỏ?", "Xác nhận", MessageBoxType.Confirmation);
                    if (ans == true)
                    {
                        CartItems.Remove(item);
                        UpdateCartTotal();
                    }
                }
            });

            RemoveFromCartCommand = new RelayCommand<CartItem>((item) =>
            {
                var ans = CustomMessageBox.Show($"Xóa sách '{item.Title}' khỏi giỏ hàng?", "Xác nhận", MessageBoxType.Confirmation);
                if (ans == true)
                {
                    CartItems.Remove(item);
                    UpdateCartTotal();
                }
            });

            ClearCartCommand = new RelayCommand<object>((p) =>
            {
                if (CartItems.Count > 0)
                {
                    var ans = CustomMessageBox.Show("Bạn muốn xóa toàn bộ giỏ hàng?", "Xác nhận", MessageBoxType.Confirmation);
                    if (ans == true)
                    {
                        CartItems.Clear();
                        UpdateCartTotal();
                    }
                }
            });

            ClearFilterCommand = new RelayCommand<object>((p) =>
            {
                SelectedSubject = null;
                SelectedAuthor = null;
                SelectedPublisher = null;

                SubjectSearchText = "";
                AuthorSearchText = "";
                PublisherSearchText = "";
                SearchKeyword = "";

                FilterProducts();
            });

            CheckoutCommand = new RelayCommand<object>((p) => Checkout());

            AddProductCommand = new RelayCommand<object>((p) =>
            {
                CustomMessageBox.Show("Tính năng đang phát triển!", "Thông báo", MessageBoxType.Info);
            });
        }

        // ==============================================================================
        // CÁC HÀM XỬ LÝ PHỤ TRỢ
        // ==============================================================================

        private void LoadMetaData()
        {
            _db = new BookStoreDbContext();

            _originSubjectList = _db.Subjects.ToList();
            SubjectList = new ObservableCollection<Subject>(_originSubjectList);

            _originAuthorList = _db.Authors.ToList();
            AuthorList = new ObservableCollection<Author>(_originAuthorList);

            _originPublisherList = _db.Publishers.ToList();
            PublisherList = new ObservableCollection<Publisher>(_originPublisherList);
        }

        private void FilterProducts()
        {
            var query = _db.Books.Where(b => !b.IsDeleted);

            if (!string.IsNullOrEmpty(SearchKeyword))
                query = query.Where(b => b.Title.Contains(SearchKeyword));
            if (SelectedSubject != null)
                query = query.Where(b => b.SubjectId == SelectedSubject.Id);
            if (SelectedAuthor != null)
                query = query.Where(b => b.AuthorId == SelectedAuthor.Id);
            if (SelectedPublisher != null)
                query = query.Where(b => b.PublisherId == SelectedPublisher.Id);

            ProductList = new ObservableCollection<Book>(query.ToList());
        }

        private void UpdateCartTotal()
        {
            CartTotal = CartItems.Sum(x => x.SubTotal);
        }

        private void Checkout()
        {
            if (CartItems.Count == 0)
            {
                CustomMessageBox.Show("Giỏ hàng đang trống. Vui lòng chọn sách!", "Cảnh báo", MessageBoxType.Warning);
                return;
            }

            var confirm = CustomMessageBox.Show($"Xác nhận thanh toán tổng cộng {CartTotal:N0} VNĐ?", "Thanh toán", MessageBoxType.Confirmation);
            if (confirm != true) return;

            try
            {
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    TotalAmount = CartTotal,
                    Status = 1,
                    AdminId = App.CurrentUser != null ? App.CurrentUser.AdminId : 1
                };

                _db.Orders.Add(order);
                _db.SaveChanges();

                foreach (var item in CartItems)
                {
                    _db.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        BookId = item.Book.Id,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });

                    var bookInDb = _db.Books.Find(item.Book.Id);
                    if (bookInDb != null)
                    {
                        bookInDb.Quantity -= item.Quantity;
                    }
                }

                _db.SaveChanges();

                CustomMessageBox.Show("Thanh toán thành công! Hóa đơn đã được lưu.", "Hoàn tất", MessageBoxType.Success);

                CartItems.Clear();
                UpdateCartTotal();
                LoadMetaData();
                FilterProducts();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi hệ thống", MessageBoxType.Error);
            }
        }
    }
}