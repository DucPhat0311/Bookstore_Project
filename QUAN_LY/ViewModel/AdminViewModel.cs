// QUAN_LY/ViewModel/AdminViewModel.cs
using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Services;
using QUAN_LY.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QUAN_LY.ViewModel
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly IAdminService _adminService;
        private readonly IDialogService _dialogService;

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
            set
            {
                _selectedAdmin = value;
                OnPropertyChanged();
                ClearPassword();
                OnPropertyChanged(nameof(IsEditing));
            }
        }

        private string _passwordInput;
        public string PasswordInput
        {
            get => _passwordInput;
            set { _passwordInput = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                OnSearchTextChanged();
            }
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

        public bool IsEditing => SelectedAdmin != null;

        private CancellationTokenSource _cancellationTokenSource;

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CancelCommand { get; }

        public AdminViewModel()
        {
            _adminService = new AdminServiceSQL();
            _dialogService = new DialogService();
            AdminList = new ObservableCollection<Admin>();

            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteDelete);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            CancelCommand = new RelayCommand(ExecuteCancel);

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Đang tải dữ liệu...";

                var admins = await _adminService.GetAllActiveAdminsAsync();
                AdminList = new ObservableCollection<Admin>(admins);

                StatusMessage = $"Đã tải {admins.Count} nhân viên";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tải dữ liệu: {ex.Message}";
                await _dialogService.ShowErrorAsync("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnSearchTextChanged()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadDataAsync();
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(400, _cancellationTokenSource.Token);
                var result = await _adminService.SearchAdminsAsync(SearchText);
                AdminList = new ObservableCollection<Admin>(result);
                StatusMessage = $"Tìm thấy {result.Count} kết quả cho '{SearchText}'";
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tìm kiếm: {ex.Message}";
            }
        }

        private void ExecuteAdd(object obj)
        {
            SelectedAdmin = new Admin
            {
                Role = "Sale Staff",
                IsActive = true
            };
            PasswordInput = "";
            StatusMessage = "Thêm nhân viên mới";
        }

        private bool CanExecuteEdit(object obj)
        {
            if (SelectedAdmin == null) return false;

            // Validate model
            if (SelectedAdmin.AdminId == 0 && string.IsNullOrEmpty(PasswordInput))
                return false;

            if (SelectedAdmin.HasErrors)
                return false;

            return true;
        }

        private bool CanExecuteDelete(object obj)
            => SelectedAdmin != null && SelectedAdmin.Role != "Super Admin";

        private async void ExecuteEdit(object obj)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Đang xử lý...";

                if (SelectedAdmin.AdminId == 0)
                {
                    // Set password for new admin
                    SelectedAdmin.Password = PasswordInput;

                    var result = await _adminService.AddAdminAsync(SelectedAdmin);
                    if (result.Success)
                    {
                        await LoadDataAsync();
                        SelectedAdmin = null;
                        PasswordInput = "";
                        await _dialogService.ShowSuccessAsync("Thành công", result.Message);
                    }
                    else
                    {
                        await _dialogService.ShowErrorAsync("Lỗi", result.Message);
                    }
                }
                else
                {
                    // Update existing admin
                    if (!string.IsNullOrEmpty(PasswordInput))
                    {
                        SelectedAdmin.Password = PasswordInput;
                    }

                    var result = await _adminService.UpdateAdminAsync(SelectedAdmin);
                    if (result.Success)
                    {
                        await LoadDataAsync();
                        ClearPassword();
                        await _dialogService.ShowSuccessAsync("Thành công", result.Message);
                    }
                    else
                    {
                        await _dialogService.ShowErrorAsync("Lỗi", result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Lỗi", $"Không thể lưu: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ExecuteDelete(object obj)
        {
            if (SelectedAdmin == null) return;

            var confirm = await _dialogService.ShowConfirmationAsync(
                "Xác nhận xóa",
                $"Bạn có chắc chắn muốn xóa nhân viên '{SelectedAdmin.Name}'?\nHành động này không thể hoàn tác."
            );

            if (!confirm) return;

            try
            {
                IsLoading = true;
                var result = await _adminService.DeactivateAdminAsync(SelectedAdmin.AdminId);

                if (result.Success)
                {
                    await LoadDataAsync();
                    SelectedAdmin = null;
                    await _dialogService.ShowSuccessAsync("Thành công", result.Message);
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Lỗi", result.Message);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Lỗi", $"Không thể xóa: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteRefresh(object obj) => LoadDataAsync().ConfigureAwait(false);

        private void ExecuteCancel(object obj)
        {
            SelectedAdmin = null;
            PasswordInput = "";
            StatusMessage = "Đã hủy thao tác";
        }

        private void ClearPassword()
        {
            PasswordInput = "";
            OnPropertyChanged(nameof(PasswordInput));
        }
    }
}