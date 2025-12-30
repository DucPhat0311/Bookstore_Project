using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    public class Order
    {
        public int Id { get; set; } // order_id
        public int AdminId { get; set; } // Ai bán
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; } // 0: Pending, 1: Paid, 2: Cancelled
        public string Note { get; set; }
    }
}

