using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations; 
using System.Linq;
using System.Runtime.CompilerServices;

namespace QUAN_LY.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged, IDataErrorInfo
    {

        public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

        // Thuộc tính này trả về lỗi của toàn bộ object
        public string Error => null;

        // Thuộc tính này trả về lỗi của từng trường (Cực quan trọng)
        public string this[string columnName]
        {
            get
            {
                var validationResults = new List<ValidationResult>();
                var property = GetType().GetProperty(columnName);

                if (property != null)
                {
                    var validationContext = new ValidationContext(this) { MemberName = columnName };
                    var value = property.GetValue(this);

                    // Kiểm tra dựa trên các Attribute ([Required], [Range]...)
                    if (!Validator.TryValidateProperty(value, validationContext, validationResults))
                    {
                        // Trả về lỗi đầu tiên tìm thấy
                        return validationResults.First().ErrorMessage;
                    }
                }
                return null; // Không có lỗi
            }
        }

        // Hàm kiểm tra xem Object có đang hợp lệ không (Dùng để bật/tắt nút Lưu)
        public bool IsValid()
        {
            var validationContext = new ValidationContext(this);
            var validationResults = new List<ValidationResult>();
            return Validator.TryValidateObject(this, validationContext, validationResults, true);
        }
    }
}