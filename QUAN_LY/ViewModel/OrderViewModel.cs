using QUAN_LY.Utilities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QUAN_LY.ViewModel
{
    public class OrderViewModel : BaseViewModel
    {
        private Model.BookStoreDbContext _db = new Model.BookStoreDbContext();

        public ObservableCollection<Model.Order> OrderList { get; set; } = new();

        // Danh sách hiển thị trong Popup
        private ObservableCollection<dynamic> _orderDetailList = new();
        public ObservableCollection<dynamic> OrderDetailList { get => _orderDetailList; set { _orderDetailList = value; OnPropertyChanged(); } }

        // Biến điều khiển ẩn/hiện Popup
        private bool _isPopupOpen;
        public bool IsPopupOpen { get => _isPopupOpen; set { _isPopupOpen = value; OnPropertyChanged(); } }

        private Model.Order _selectedOrder;
        public Model.Order SelectedOrder
        {
            get => _selectedOrder;
            set { _selectedOrder = value; OnPropertyChanged(); if (value != null) OpenOrderDetail(); }
        }

        public System.Windows.Input.ICommand ClosePopupCommand { get; set; }

        public OrderViewModel()
        {
            LoadData();
            ClosePopupCommand = new RelayCommand<object>((p) => IsPopupOpen = false);
        }

        public void LoadData()
        {
            OrderList.Clear();
            var list = _db.Orders.OrderByDescending(x => x.OrderDate).ToList();
            foreach (var item in list) OrderList.Add(item);
        }

        private void OpenOrderDetail()
        {
            var details = (from d in _db.OrderItems
                           join b in _db.Books on d.BookId equals b.Id
                           where d.OrderId == SelectedOrder.Id
                           select new
                           {
                               BookTitle = b.Title,
                               Quantity = d.Quantity ?? 0,
                               Price = d.Price ?? 0, //cái này là unit price tại bên database bên kia ghi price làm biếng sửa
                               SubTotal = (d.Quantity ?? 0) * (d.Price ?? 0)
                           }).ToList<dynamic>();

            OrderDetailList = new ObservableCollection<dynamic>(details);
            IsPopupOpen = true; // Mở giao diện đẹp thay vì MessageBox
        }
    }
}