using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    public class Admin
    {
        public int Id { get; set; } // admin_id
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // password phải hash trước khi lưu
        public string Role { get; set; } // "Manager" hoặc "Sale Staff"
        public bool IsActive { get; set; }
    }
}
