// QUAN_LY/Model/Admin.cs
using QUAN_LY.ViewModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUAN_LY.Model
{
    [Table("admin")]
    public class Admin : BaseViewModel, IDataErrorInfo
    {
        [Key]
        [Column("admin_id")]
        public int AdminId { get; set; }

        private string _name;
        [Column("name")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
        public string Name
        {
            get => _name;
            set { _name = value.Trim(); OnPropertyChanged(); ValidateProperty(); }
        }

        private string _username;
        [Column("username")]
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-100 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ chứa chữ cái, số và dấu gạch dưới")]
        public string Username
        {
            get => _username;
            set { _username = value.Trim().ToLower(); OnPropertyChanged(); ValidateProperty(); }
        }

        private string _password;
        [NotMapped] // Không lưu vào DB
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); ValidateProperty(); }
        }

        [Column("password")]
        public string PasswordHash { get; set; } // Hash lưu vào DB

        private string _role;
        [Column("role")]
        [Required]
        public string Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); ValidateProperty(); }
        }

        private bool _isActive = true;
        [Column("is_active")]
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        // Computed properties
        public string RoleDisplay => Role switch
        {
            "Super Admin" => "Quản trị hệ thống",
            "Manager" => "Quản lý",
            "Sale Staff" => "Nhân viên bán hàng",
            _ => Role
        };

        public string StatusDisplay => IsActive ? "Đang làm việc" : "Đã nghỉ";

        // Validation
        // Trong class Admin
        public void ValidateProperty()
        {
            Validate("Password");
            OnPropertyChanged(nameof(Error));
            OnPropertyChanged(nameof(HasErrors));
        }

        private void Validate(string propertyName)
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var results = new List<ValidationResult>();

            Validator.TryValidateProperty(
                GetType().GetProperty(propertyName)?.GetValue(this),
                context,
                results
            );

            _errors[propertyName] = results.FirstOrDefault()?.ErrorMessage;
        }

        private Dictionary<string, string> _errors = new Dictionary<string, string>();

        // IDataErrorInfo implementation
        public string Error => string.Join("\n", _errors.Values.Where(v => !string.IsNullOrEmpty(v)));
        public string this[string columnName]
        {
            get
            {
                if (_errors.ContainsKey(columnName))
                    return _errors[columnName];
                return null;
            }
        }

        public bool HasErrors => !string.IsNullOrEmpty(Error);
    }
}