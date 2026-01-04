
using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Services;
using QUAN_LY.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        // Form hiển thị khi thêm mới
        private Admin _newAdmin;
        public Admin NewAdmin
        {
            get => _newAdmin;
            set { _newAdmin = value; OnPropertyChanged(); }
        }

        private bool _isAdding;
        public bool IsAdding
        {
            get => _isAdding;
            set { _isAdding = value; OnPropertyChanged(); }
        }

        private CancellationTokenSource _cancellationTokenSource;

        public ICommand AddCommand { get; }
        public ICommand SaveNewCommand { get; }
        public ICommand CancelAddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public AdminViewModel()
        {
            _adminService = new AdminServiceSQL();
            _dialogService = new DialogService();
            AdminList = new ObservableCollection<Admin>();
            NewAdmin = new Admin
            {
                Role = "Sale Staff",
                IsActive = true
            };

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveNewCommand = new RelayCommand(ExecuteSaveNew, CanExecuteSaveNew);
            CancelAddCommand = new RelayCommand(ExecuteCancelAdd);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteDelete);
            RefreshCommand = new RelayCommand(ExecuteRefresh);

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
            IsAdding = true;
            NewAdmin = new Admin
            {
                Role = "Sale Staff",
                IsActive = true
            };
            PasswordInput = "";
            StatusMessage = "Thêm nhân viên mới";
        }

        private bool CanExecuteSaveNew(object obj)
        {
            if (NewAdmin == null) return false;

            // Chỉ validate khi thêm mới
            if (string.IsNullOrEmpty(NewAdmin.Name) ||
                string.IsNullOrEmpty(NewAdmin.Username) ||
                string.IsNullOrEmpty(PasswordInput))
                return false;

            return !NewAdmin.HasErrors;
        }

        private bool CanExecuteDelete(object obj)
            => SelectedAdmin != null && SelectedAdmin.Role != "Super Admin";

        private async void ExecuteSaveNew(object obj)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Đang thêm nhân viên...";

                // Set password
                NewAdmin.Password = PasswordInput;

                var result = await _adminService.AddAdminAsync(NewAdmin);
                if (result.Success)
                {
                    await LoadDataAsync();
                    IsAdding = false;
                    PasswordInput = "";
                    await _dialogService.ShowSuccessAsync("Thành công", result.Message);
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Lỗi", result.Message);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Lỗi", $"Không thể thêm: {ex.Message}");
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

        private void ExecuteCancelAdd(object obj)
        {
            IsAdding = false;
            PasswordInput = "";
            StatusMessage = "Đã hủy thêm nhân viên";
        }

        private void ExecuteRefresh(object obj) => LoadDataAsync().ConfigureAwait(false);
    }
}