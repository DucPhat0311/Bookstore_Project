using Microsoft.EntityFrameworkCore;
using QUAN_LY.Model;
using QUAN_LY.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace QUAN_LY.ViewModel
{
    public class OrderViewModel : BaseViewModel
    {
        private readonly BookStoreDbContext _db = new();

        // Sử dụng object thay vì dynamic để tránh lỗi Expression Tree khi lọc
        private ObservableCollection<object> _orderList = new();
        public ObservableCollection<object> OrderList { get => _orderList; set { _orderList = value; OnPropertyChanged(); } }

        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set { _filterText = value; OnPropertyChanged(); LoadData(); }
        }

        private ObservableCollection<object> _orderDetailList = new();
        public ObservableCollection<object> OrderDetailList { get => _orderDetailList; set { _orderDetailList = value; OnPropertyChanged(); } }

        private bool _isPopupOpen;
        public bool IsPopupOpen { get => _isPopupOpen; set { _isPopupOpen = value; OnPropertyChanged(); } }

        private dynamic _selectedOrder; // Giữ dynamic ở đây để dễ truy xuất thuộc tính khi hiển thị
        public dynamic SelectedOrder
        {
            get => _selectedOrder;
            set { _selectedOrder = value; OnPropertyChanged(); if (value != null) OpenOrderDetail(); }
        }

        public ICommand ClosePopupCommand { get; set; }

        public OrderViewModel()
        {
            LoadData();
            ClosePopupCommand = new RelayCommand<object>((p) => IsPopupOpen = false);
        }

        public void LoadData()
        {
            // Bước 1: Truy vấn kiểu ẩn danh (Anonymous Type) giúp EF dịch sang SQL dễ dàng
            var query = from o in _db.Orders
                        join a in _db.Admins on o.AdminId equals a.AdminId into adminJoin
                        from a in adminJoin.DefaultIfEmpty()
                        select new
                        {
                            Id = o.Id, // Ánh xạ từ [Column("order_id")]
                            OrderDate = o.OrderDate,
                            TotalAmount = o.TotalAmount,
                            Note = o.Note,
                            StaffName = a != null ? a.Name : "Hệ thống"
                        };

            // Bước 2: Lọc dữ liệu trên Queryable (EF sẽ dịch cái này thành WHERE trong SQL)
            if (!string.IsNullOrEmpty(FilterText))
            {
                string search = FilterText.ToLower();
                query = query.Where(x => x.Id.ToString().Contains(search) ||
                                         x.StaffName.ToLower().Contains(search));
            }

            // Bước 3: Đưa về List và hiển thị
            var result = query.OrderByDescending(x => x.OrderDate).ToList<object>();
            OrderList = new ObservableCollection<object>(result);
        }

        private void OpenOrderDetail()
        {
            if (SelectedOrder == null) return;

            // Lấy ID từ SelectedOrder (phải ép kiểu vì SelectedOrder là dynamic)
            int currentOrderId = (int)SelectedOrder.Id;

            var details = from d in _db.OrderItems
                          join b in _db.Books on d.BookId equals b.Id
                          where d.OrderId == currentOrderId
                          select new
                          {
                              BookTitle = b.Title,
                              Quantity = d.Quantity ?? 0,
                              Price = d.Price ?? 0, // Ánh xạ từ [Column("unit_price")]
                              SubTotal = (d.Quantity ?? 0) * (d.Price ?? 0)
                          };

            var resultList = details.ToList<object>();
            OrderDetailList.Clear();
            foreach (var item in resultList)
            {
                OrderDetailList.Add(item);
            }
            IsPopupOpen = true;
        }
    }
}