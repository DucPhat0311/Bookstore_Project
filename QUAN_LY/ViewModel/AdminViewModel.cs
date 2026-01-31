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

    
        private ObservableCollection<string> _availableRoles;
        public ObservableCollection<string> AvailableRoles
        {
            get => _availableRoles;
            set { _availableRoles = value; OnPropertyChanged(); }
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
            set { _passwordInput = value; OnPropertyChanged(); }
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

        public string StatusMessage { get; set; } 

        
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

                
                LoadAvailableRoles();

                IsAdding = true; 
            });

            SaveNewCommand = new RelayCommand(async (p) => await ExecuteSaveNewAsync());

            CancelAddCommand = new RelayCommand((p) =>
            {
                IsAdding = false;
                NewAdmin = null;
            });

            DeleteCommand = new RelayCommand(async (p) => await ExecuteDeleteAsync());

            
            _ = LoadDataAsync();
        }

        
        private void LoadAvailableRoles()
        {
            var roles = new ObservableCollection<string>();
            roles.Add(App.Roles.SaleStaff);

            
            if (App.CurrentUser?.Role == App.Roles.SuperAdmin)
            {
                roles.Add(App.Roles.Manager);
            }

            AvailableRoles = roles;
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    var list = await _adminService.GetAllActiveAdminsAsync();
                    AdminList = new ObservableCollection<Admin>(list);
                }
                else
                {
                    var list = await _adminService.SearchAdminsAsync(SearchText);
                    AdminList = new ObservableCollection<Admin>(list);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteSaveNewAsync()
        {
            if (NewAdmin == null) return;

           
            if (string.IsNullOrWhiteSpace(NewAdmin.Name) ||
                string.IsNullOrWhiteSpace(NewAdmin.Username) ||
                string.IsNullOrWhiteSpace(NewAdmin.Role))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordInput) || PasswordInput.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải từ 6 ký tự trở lên!", "Mật khẩu yếu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (App.CurrentUser?.Role == App.Roles.Manager && NewAdmin.Role != App.Roles.SaleStaff)
            {
                MessageBox.Show("Bạn không có quyền tạo tài khoản quản lý (Manager)!", "Lỗi phân quyền", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewAdmin.Password = PasswordInput;

            try
            {
                IsLoading = true;
                var result = await _adminService.AddAdminAsync(NewAdmin);

                if (result.Success)
                {
                    MessageBox.Show("Thêm nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAdding = false; 
                    NewAdmin = null;
                    await LoadDataAsync(); 
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
                MessageBox.Show("Bạn không thể xóa tài khoản đang đăng nhập!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

           
            var currentRole = App.CurrentUser?.Role;
            var targetRole = SelectedAdmin.Role;

            if (currentRole == App.Roles.Manager)
            {
                
                if (targetRole == App.Roles.SuperAdmin || targetRole == App.Roles.Manager)
                {
                    MessageBox.Show("Bạn chỉ có quyền quản lý nhân viên bán hàng (Sale Staff)!", "Truy cập bị từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            
            if (currentRole == App.Roles.SuperAdmin && targetRole == App.Roles.SuperAdmin)
            {
                MessageBox.Show("Không thể xóa tài khoản Super Admin khác!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
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