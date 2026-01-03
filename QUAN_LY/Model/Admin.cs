// QUAN_LY/Model/Admin.cs
using QUAN_LY.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUAN_LY.Model
{
    [Table("admin")]
    public class Admin : BaseViewModel
    {
        [Key]
        [Column("admin_id")]
        public int AdminId { get; set; }

        private string _name;
        [Column("name")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _username;
        [Column("username")]
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(100)]
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        [Column("password")]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _role;
        [Column("role")]
        [Required]
        public string Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); }
        }

        private bool _isActive = true;
        [Column("is_active")]
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public string RoleDisplay => Role switch
        {
            "Super Admin" => "Quản trị hệ thống",
            "Manager" => "Quản lý",
            "Sale Staff" => "Nhân viên bán hàng",
            _ => Role
        };

        public string StatusDisplay => IsActive ? "Đang làm việc" : "Đã nghỉ";
    }
}