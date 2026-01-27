using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QUAN_LY.ViewModel
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly IAdminService _adminService;

        private ObservableCollection<Admin> _adminList;
        public ObservableCollection<Admin> AdminList
        {
            get => _adminList;
            set { _adminList = value; OnPropertyChanged(); }
        }

        private Admin _selectedAdmin;
        public Admin SelectedAdmin
        {
            get => _selectedAdmin;
            set { _selectedAdmin = value; OnPropertyChanged(); }
        }

        private Admin _newAdmin;
        public Admin NewAdmin
        {
            get => _newAdmin;
            set { _newAdmin = value; OnPropertyChanged(); }
        }

        
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
               
                _ = LoadDataAsync();
            }
        }
        

        private string _passwordInput;
        public string PasswordInput
        {
            get => _passwordInput;
            set
            {
                _passwordInput = value;
                OnPropertyChanged();
                if (NewAdmin != null) NewAdmin.Password = value;
            }
        }

        private bool _isAdding;
        public bool IsAdding
        {
            get => _isAdding;
            set { _isAdding = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

       
        public ICommand LoadDataCommand { get; set; }
        public ICommand OpenAddCommand { get; set; }
        public ICommand SaveNewCommand { get; set; }
        public ICommand CancelAddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }


        public AdminViewModel(IAdminService adminService)
        {
            _adminService = adminService;

            LoadDataCommand = new RelayCommand(async (p) => await LoadDataAsync());

            OpenAddCommand = new RelayCommand((p) =>
            {
                NewAdmin = new Admin { IsActive = true, Role = null };
                PasswordInput = "";
                IsAdding = true;
                StatusMessage = "Nhập thông tin nhân viên mới...";
            });

            SaveNewCommand = new RelayCommand(async (p) => await ExecuteSaveNewAsync(), (p) => CanSave());

            CancelAddCommand = new RelayCommand((p) => { IsAdding = false; StatusMessage = "Đã hủy thao tác."; });

            DeleteCommand = new RelayCommand(async (p) => await ExecuteDeleteAsync(), (p) => SelectedAdmin != null);

            _ = LoadDataAsync();
        }

       
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                System.Collections.Generic.List<Admin> list;

               
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    list = await _adminService.GetAllActiveAdminsAsync();
                    StatusMessage = $"Đã tải {list.Count} nhân viên.";
                }
               
                else
                {
                    list = await _adminService.SearchAdminsAsync(SearchText);
                    StatusMessage = $"Tìm thấy {list.Count} kết quả cho '{SearchText}'.";
                }

                AdminList = new ObservableCollection<Admin>(list);
            }
            catch (Exception ex)
            {
                StatusMessage = "Lỗi tải dữ liệu: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
        

        private bool CanSave()
        {
            if (IsLoading || NewAdmin == null) return false;
            bool isModelValid = !NewAdmin.HasErrors;
            bool isPasswordValid = !string.IsNullOrEmpty(PasswordInput) && PasswordInput.Length >= 6;
            return isModelValid && isPasswordValid;
        }

        private async Task ExecuteSaveNewAsync()
        {
            if (NewAdmin.Role == null)
            {
                MessageBox.Show("Vui lòng chọn chức vụ (Sale Staff hoặc Manager)!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(PasswordInput) || PasswordInput.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Mật khẩu yếu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsLoading) return;

            try
            {
                IsLoading = true;
                NewAdmin.Password = PasswordInput;

                var result = await _adminService.AddAdminAsync(NewAdmin);

                if (result.Success)
                {
                    MessageBox.Show("Thêm nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAdding = false;

                    
                    SearchText = "";
                   
                }
                else
                {
                    MessageBox.Show(result.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteDeleteAsync()
        {
            if (SelectedAdmin == null) return;

            if (SelectedAdmin.Username == App.CurrentUser?.Username)
            {
                MessageBox.Show("Bạn không thể tự xóa chính mình!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedAdmin.Role == App.Roles.SuperAdmin && App.CurrentUser?.Role != App.Roles.SuperAdmin)
            {
                MessageBox.Show("Bạn không đủ quyền xóa Super Admin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên '{SelectedAdmin.Name}'?",
                                          "Xác nhận xóa",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var result = await _adminService.DeactivateAdminAsync(SelectedAdmin.AdminId);

                    if (result.Success)
                    {
                        MessageBox.Show("Đã xóa nhân viên.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}