using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    public class OrderItem
    {
        public int Id { get; set; } // order_item_id
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Giá tại thời điểm bán
    }
}
