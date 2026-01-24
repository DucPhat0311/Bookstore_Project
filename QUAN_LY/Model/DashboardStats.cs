using System;
using System.Collections.Generic;
using System.Text;

namespace QUAN_LY_APP.Model
{
    public class DashboardStats
    {
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
        public int BooksSold { get; set; }
        public int LowStockCount { get; set; }
    }
}
