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
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _username;
        [Column("username")]
        [Required(ErrorMessage = "Tên đăng nhập bắt buộc")]
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        [NotMapped]
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        [Column("password")]
        public string PasswordHash { get; set; }

        private string _role;
        [Column("role")]
        [Required(ErrorMessage = "Chưa chọn chức vụ")]
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

        
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                var context = new ValidationContext(this) { MemberName = columnName };
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateProperty(GetType().GetProperty(columnName).GetValue(this), context, results))
                {
                    return results[0].ErrorMessage;
                }
                return null;
            }
        }

      
        public bool HasErrors
        {
            get
            {
                
                var context = new ValidationContext(this);
                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(this, context, results, true);
                return !isValid;
            }
        }
    }
}