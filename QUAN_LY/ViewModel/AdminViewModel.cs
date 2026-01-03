// QUAN_LY/ViewModel/AdminViewModel.cs
using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Services;
using QUAN_LY.Utilities;
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

        private CancellationTokenSource _cancellationTokenSource;

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public AdminViewModel()
        {
            _adminService = new AdminServiceSQL();
            AdminList = new ObservableCollection<Admin>();

            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, CanEditOrDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanEditOrDelete);
            RefreshCommand = new RelayCommand(ExecuteRefresh);

            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            var admins = await _adminService.GetAllActiveAdminsAsync();
            AdminList = new ObservableCollection<Admin>(admins);
        }

        private async void OnSearchTextChanged()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadDataAsync();
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(400, _cancellationTokenSource.Token);
                var result = await _adminService.SearchAdminsAsync(SearchText);
                AdminList = new ObservableCollection<Admin>(result);
            }
            catch (TaskCanceledException) { }
        }

        private void ExecuteAdd(object obj)
        {
            SelectedAdmin = new Admin { Role = "Sale Staff" }; // Mặc định
        }

        private bool CanEditOrDelete(object obj) => SelectedAdmin != null && SelectedAdmin.Role != "Super Admin";

        private async void ExecuteEdit(object obj)
        {
            // Có thể mở dialog chi tiết để sửa, ở đây chỉ dùng SelectedAdmin để bind
            if (SelectedAdmin.AdminId == 0) // Thêm mới
            {
                var success = await _adminService.AddAdminAsync(SelectedAdmin);
                if (success)
                {
                    LoadDataAsync();
                    SelectedAdmin = null;
                }
                else
                {
                    // Thông báo username trùng
                }
            }
            else // Cập nhật
            {
                var success = await _adminService.UpdateAdminAsync(SelectedAdmin);
                if (success) LoadDataAsync();
            }
        }

        private async void ExecuteDelete(object obj)
        {
            if (SelectedAdmin == null) return;

            var success = await _adminService.DeactivateAdminAsync(SelectedAdmin.AdminId);
            if (success) LoadDataAsync();
        }

        private void ExecuteRefresh(object obj) => LoadDataAsync();
    }
}