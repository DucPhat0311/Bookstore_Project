using System.Windows;
using QUAN_LY.Model;

namespace QUAN_LY
{
    public partial class App : Application
    {
        // Biến static để truy cập từ toàn bộ dự án: App.CurrentUser
        public static Admin CurrentUser { get; set; }

        // Định nghĩa các Role cứng để tránh gõ sai chính tả sau này
        public static class Roles
        {
            public const string SuperAdmin = "Super Admin";
            public const string Manager = "Manager";
            public const string SaleStaff = "Sale Staff";
        }
    }
}