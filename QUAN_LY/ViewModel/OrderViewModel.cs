using QUAN_LY.Model;
using QUAN_LY.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QUAN_LY.ViewModel
{
    // 1. Class DTO quan trọng để hiển thị chi tiết (Fix lỗi bảng trắng)
    public class OrderDetailDTO
    {
        public string BookTitle { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
    }

    // 2. Class hiển thị Giỏ hàng
    public class CartItem : BaseViewModel
    {
        public Book Book { get; set; }
        public string Title => Book.Title;
        public decimal Price => Book.Price ;
        private int _quantity;
        public int Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(SubTotal)); } }
        public decimal SubTotal => Price * Quantity;
    }

    public class OrderViewModel : BaseViewModel
    {
        private BookStoreDbContext _db = new BookStoreDbContext();

        // ============================
        // PHẦN 1: BÁN HÀNG (POS)
        // ============================
        private ObservableCollection<Book> _productList;
        public ObservableCollection<Book> ProductList { get => _productList; set { _productList = value; OnPropertyChanged(); } }

        public ObservableCollection<Subject> SubjectList { get; set; } = new ObservableCollection<Subject>();
        public ObservableCollection<Author> AuthorList { get; set; } = new ObservableCollection<Author>();
        public ObservableCollection<Publisher> PublisherList { get; set; } = new ObservableCollection<Publisher>();

        private Subject _selectedSubject;
        public Subject SelectedSubject { get => _selectedSubject; set { _selectedSubject = value; OnPropertyChanged(); FilterProducts(); } }

        private Author _selectedAuthor;
        public Author SelectedAuthor { get => _selectedAuthor; set { _selectedAuthor = value; OnPropertyChanged(); FilterProducts(); } }

        private Publisher _selectedPublisher;
        public Publisher SelectedPublisher { get => _selectedPublisher; set { _selectedPublisher = value; OnPropertyChanged(); FilterProducts(); } }

        private string _searchKeyword;
        public string SearchKeyword { get => _searchKeyword; set { _searchKeyword = value; OnPropertyChanged(); FilterProducts(); } }

        public ObservableCollection<CartItem> CartItems { get; set; } = new ObservableCollection<CartItem>();
        private decimal _cartTotal;
        public decimal CartTotal { get => _cartTotal; set { _cartTotal = value; OnPropertyChanged(); } }


        // ============================
        // PHẦN 2: LỊCH SỬ & CHI TIẾT
        // ============================
        public ObservableCollection<Order> OrderList { get; set; } = new ObservableCollection<Order>();

        // SỬA LẠI: Dùng OrderDetailDTO thay vì dynamic
        private ObservableCollection<OrderDetailDTO> _orderDetailList = new ObservableCollection<OrderDetailDTO>();
        public ObservableCollection<OrderDetailDTO> OrderDetailList { get => _orderDetailList; set { _orderDetailList = value; OnPropertyChanged(); } }

        private bool _isPopupOpen;
        public bool IsPopupOpen { get => _isPopupOpen; set { _isPopupOpen = value; OnPropertyChanged(); } }

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                if (value != null) OpenOrderDetail();
            }
        }

        // ============================
        // COMMANDS
        // ============================
        public ICommand AddToCartCommand { get; set; }
        public ICommand ClearCartCommand { get; set; }
        public ICommand CheckoutCommand { get; set; }
        public ICommand ClearFilterCommand { get; set; }
        public ICommand ClosePopupCommand { get; set; }
        public ICommand RemoveFromCartCommand { get; set; }
        public ICommand IncreaseQtyCommand { get; set; }
        public ICommand DecreaseQtyCommand { get; set; }


        public OrderViewModel()
        {
            LoadFilterData();
            FilterProducts();
            LoadHistory();

            // --- Logic Bán hàng ---
            AddToCartCommand = new RelayCommand<Book>((book) => {
                var item = CartItems.FirstOrDefault(x => x.Book.Id == book.Id);
                if (item != null) item.Quantity++;
                else CartItems.Add(new CartItem { Book = book, Quantity = 1 });
                UpdateTotal();
            });

            RemoveFromCartCommand = new RelayCommand<CartItem>((item) => { CartItems.Remove(item); UpdateTotal(); });

            IncreaseQtyCommand = new RelayCommand<CartItem>((item) => {
                if (item.Quantity < item.Book.Quantity) { item.Quantity++; UpdateTotal(); }
            });

            DecreaseQtyCommand = new RelayCommand<CartItem>((item) => {
                if (item.Quantity > 1) { item.Quantity--; UpdateTotal(); }
                else { CartItems.Remove(item); UpdateTotal(); }
            });

            ClearCartCommand = new RelayCommand<object>((p) => { CartItems.Clear(); UpdateTotal(); });

            CheckoutCommand = new RelayCommand<object>((p) => {
                if (CartItems.Count == 0) return;

                var order = new Order { OrderDate = DateTime.Now, TotalAmount = CartTotal, Status = 1, AdminId = 1 };
                _db.Orders.Add(order);
                _db.SaveChanges();

                foreach (var item in CartItems)
                {
                    _db.OrderItems.Add(new OrderItem { OrderId = order.Id, BookId = item.Book.Id, Quantity = item.Quantity, Price = item.Price });
                    var book = _db.Books.Find(item.Book.Id);
                    if (book != null) book.Quantity -= item.Quantity;
                }
                _db.SaveChanges();

                MessageBox.Show("Thanh toán thành công!");
                CartItems.Clear(); UpdateTotal(); FilterProducts(); LoadHistory();
            });

            ClearFilterCommand = new RelayCommand<object>((p) => {
                SelectedSubject = null; SelectedAuthor = null; SelectedPublisher = null; SearchKeyword = "";
            });

            ClosePopupCommand = new RelayCommand<object>((p) => IsPopupOpen = false);
        }

        private void LoadFilterData()
        {
            _db = new BookStoreDbContext();
            SubjectList = new ObservableCollection<Subject>(_db.Subjects.ToList());
            AuthorList = new ObservableCollection<Author>(_db.Authors.ToList());
            PublisherList = new ObservableCollection<Publisher>(_db.Publishers.ToList());
        }

        private void FilterProducts()
        {
            var query = _db.Books.Where(x => !x.IsDeleted && x.Quantity > 0).AsQueryable();

            if (!string.IsNullOrEmpty(SearchKeyword)) query = query.Where(x => x.Title.Contains(SearchKeyword));
            if (SelectedSubject != null) query = query.Where(x => x.SubjectId == SelectedSubject.Id);
            if (SelectedAuthor != null) query = query.Where(x => x.AuthorId == SelectedAuthor.Id);
            if (SelectedPublisher != null) query = query.Where(x => x.PublisherId == SelectedPublisher.Id);

            ProductList = new ObservableCollection<Book>(query.ToList());
        }

        private void LoadHistory()
        {
            OrderList.Clear();
            _db = new BookStoreDbContext();
            var list = _db.Orders.OrderByDescending(x => x.OrderDate).ToList();
            foreach (var item in list) OrderList.Add(item);
        }

        // HÀM QUAN TRỌNG ĐÃ SỬA:
        private void OpenOrderDetail()
        {
            if (SelectedOrder == null) return;

            using (var db = new BookStoreDbContext())
            {
                var details = (from d in db.OrderItems
                               join b in db.Books on d.BookId equals b.Id
                               where d.OrderId == SelectedOrder.Id
                               // Ép kiểu về OrderDetailDTO
                               select new OrderDetailDTO
                               {
                                   BookTitle = b.Title,
                                   Quantity = d.Quantity ?? 0,
                                   Price = d.Price ?? 0,
                                   SubTotal = (d.Quantity ?? 0) * (d.Price ?? 0)
                               }).ToList();

                OrderDetailList = new ObservableCollection<OrderDetailDTO>(details);
                IsPopupOpen = true;
            }
        }

        private void UpdateTotal() => CartTotal = CartItems.Sum(x => x.SubTotal);
    }
}