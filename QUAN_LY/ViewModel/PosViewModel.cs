using QUAN_LY.Model;
using QUAN_LY.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QUAN_LY.ViewModel
{
    // ==============================================================================
    // 1. CLASS CART ITEM (ĐƯỢC TINH CHỈNH ĐỂ BINDING HAI CHIỀU)
    // ==============================================================================
    public class CartItem : BaseViewModel
    {
        public Book Book { get; set; }
        public string Title => Book.Title;

        // Xử lý giá: Nếu null thì tính là 0
        public decimal Price => Book.Price;

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                // Tinh chỉnh: Khi số lượng đổi, thông báo cho giao diện cập nhật ngay SubTotal
                OnPropertyChanged(nameof(SubTotal));
            }
        }

        // Thành tiền = Giá * Số lượng
        public decimal SubTotal => Price * Quantity;
    }

    // ==============================================================================
    // 2. POS VIEW MODEL (LOGIC CHÍNH)
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

        public ObservableCollection<Subject> SubjectList { get; set; }
        public ObservableCollection<Author> AuthorList { get; set; }
        public ObservableCollection<Publisher> PublisherList { get; set; }

        // --- CÁC BIẾN LỰA CHỌN (FILTER) ---
        // Tinh chỉnh: Khi chọn xong sẽ gọi hàm FilterProducts() ngay lập tức
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

        // --- COMMANDS (CÁC LỆNH TỪ VIEW) ---
        public ICommand AddToCartCommand { get; set; }
        public ICommand RemoveFromCartCommand { get; set; }
        public ICommand IncreaseQtyCommand { get; set; }
        public ICommand DecreaseQtyCommand { get; set; }
        public ICommand ClearCartCommand { get; set; }
        public ICommand ClearFilterCommand { get; set; }
        public ICommand CheckoutCommand { get; set; }

        // ==============================================================================
        // CONSTRUCTOR
        // ==============================================================================
        public PosViewModel()
        {
            LoadMetaData();     // Tải danh mục (Tác giả, Thể loại...)
            FilterProducts();   // Tải danh sách sách

            // 1. Logic Thêm vào giỏ
            AddToCartCommand = new RelayCommand<Book>((book) =>
            {
                var existingItem = CartItems.FirstOrDefault(x => x.Book.Id == book.Id);

                if (existingItem != null)
                {
                    // Nếu đã có -> Tăng số lượng (Kiểm tra tồn kho)
                    if (existingItem.Quantity < book.Quantity)
                    {
                        existingItem.Quantity++;
                        UpdateCartTotal();
                    }
                    else
                    {
                        MessageBox.Show($"Kho chỉ còn {book.Quantity} cuốn.", "Hết hàng");
                    }
                }
                else
                {
                    // Nếu chưa có -> Thêm mới
                    if (book.Quantity > 0)
                    {
                        CartItems.Add(new CartItem { Book = book, Quantity = 1 });
                        UpdateCartTotal();
                    }
                    else
                    {
                        MessageBox.Show("Sách này đã hết hàng.", "Thông báo");
                    }
                }
            });

            // 2. Logic Tăng số lượng (+)
            IncreaseQtyCommand = new RelayCommand<CartItem>((item) =>
            {
                // Kiểm tra tồn kho real-time
                if (item.Quantity < item.Book.Quantity)
                {
                    item.Quantity++;
                    UpdateCartTotal();
                }
                else
                {
                    MessageBox.Show("Đã đạt giới hạn tồn kho.", "Thông báo");
                }
            });

            // 3. Logic Giảm số lượng (-)
            DecreaseQtyCommand = new RelayCommand<CartItem>((item) =>
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    UpdateCartTotal();
                }
                else
                {
                    // Nếu giảm về 0 thì hỏi xóa
                    var ans = MessageBox.Show("Bạn muốn bỏ sách này khỏi giỏ?", "Xác nhận", MessageBoxButton.YesNo);
                    if (ans == MessageBoxResult.Yes)
                    {
                        CartItems.Remove(item);
                        UpdateCartTotal();
                    }
                }
            });

            // 4. Logic Xóa 1 món
            RemoveFromCartCommand = new RelayCommand<CartItem>((item) =>
            {
                CartItems.Remove(item);
                UpdateCartTotal();
            });

            // 5. Logic Xóa hết giỏ
            ClearCartCommand = new RelayCommand<object>((p) =>
            {
                CartItems.Clear();
                UpdateCartTotal();
            });

            // 6. Logic Reset bộ lọc
            ClearFilterCommand = new RelayCommand<object>((p) =>
            {
                SelectedSubject = null;
                SelectedAuthor = null;
                SelectedPublisher = null;
                SearchKeyword = "";
                FilterProducts();
            });

            // 7. Logic Thanh toán
            CheckoutCommand = new RelayCommand<object>((p) => Checkout());
        }

        // ==============================================================================
        // CÁC HÀM XỬ LÝ (HELPER METHODS)
        // ==============================================================================

        private void LoadMetaData()
        {
            // Reset DB Context để lấy dữ liệu mới nhất
            _db = new BookStoreDbContext();
            SubjectList = new ObservableCollection<Subject>(_db.Subjects.ToList());
            AuthorList = new ObservableCollection<Author>(_db.Authors.ToList());
            PublisherList = new ObservableCollection<Publisher>(_db.Publishers.ToList());
        }

        private void FilterProducts()
        {
            // Lấy tất cả sách chưa xóa
            var query = _db.Books.Where(b => !b.IsDeleted);

            // Áp dụng các bộ lọc nếu có
            if (!string.IsNullOrEmpty(SearchKeyword))
                query = query.Where(b => b.Title.Contains(SearchKeyword));

            if (SelectedSubject != null)
                query = query.Where(b => b.SubjectId == SelectedSubject.Id);

            if (SelectedAuthor != null)
                query = query.Where(b => b.AuthorId == SelectedAuthor.Id);

            if (SelectedPublisher != null)
                query = query.Where(b => b.PublisherId == SelectedPublisher.Id);

            // Đổ dữ liệu ra View
            ProductList = new ObservableCollection<Book>(query.ToList());
        }

        private void UpdateCartTotal()
        {
            // Tính tổng tiền dựa trên SubTotal của từng CartItem
            CartTotal = CartItems.Sum(x => x.SubTotal);
        }

        private void Checkout()
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn sách trước khi thanh toán.", "Giỏ hàng trống");
                return;
            }

            try
            {
                // Tạo đơn hàng mới
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    TotalAmount = CartTotal,
                    Status = 1, // Đã thanh toán
                    AdminId = App.CurrentUser != null ? App.CurrentUser.AdminId : 1 // Đã chỉnh lại được AdminId
                };

                _db.Orders.Add(order);
                _db.SaveChanges(); // Lưu để sinh ra OrderId

                // Lưu chi tiết đơn hàng và trừ kho
                foreach (var item in CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        BookId = item.Book.Id,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    _db.OrderItems.Add(orderItem);

                    // Trừ tồn kho
                    var bookInDb = _db.Books.Find(item.Book.Id);
                    if (bookInDb != null)
                    {
                        bookInDb.Quantity -= item.Quantity;
                    }
                }

                _db.SaveChanges(); // Lưu tất cả thay đổi

                MessageBox.Show("Thanh toán thành công!", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset giao diện sau khi bán
                CartItems.Clear();
                UpdateCartTotal();

                // Tải lại dữ liệu sách (để cập nhật số lượng tồn kho mới lên giao diện)
                LoadMetaData();
                FilterProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}