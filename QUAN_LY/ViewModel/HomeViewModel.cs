using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Services;
using QUAN_LY.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading; 

namespace QUAN_LY.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly BookServiceSQL _bookService;

        // Biến để quản lý việc hủy request cũ
        private CancellationTokenSource _cancellationTokenSource;

        private ObservableCollection<Book> _bookList;
        public ObservableCollection<Book> BookList
        {
            get => _bookList;
            set { _bookList = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // Mỗi lần set text, gọi hàm xử lý Debounce
                OnSearchTextChanged();
            }
        }

        public HomeViewModel()
        {
            _bookService = new BookServiceSQL();
            BookList = new ObservableCollection<Book>();
        }

        private async void OnSearchTextChanged()
        {
            // Nếu người dùng xóa hết chữ, xóa list hiển thị
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                BookList.Clear();
                return;
            }

            // HỦY request cũ nếu nó đang chạy dở
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            // Tạo token mới cho lượt gõ hiện tại
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                // DEBOUNCE: Chờ 500ms xem người dùng có gõ tiếp không
                // Nếu trong 500ms này người dùng gõ phím khác, hàm này sẽ bị Cancel ở bước 3 của lần gọi sau
                await Task.Delay(500, token);


                // Nếu còn sống sót sau 500ms, gọi xuống DB
                var result = await _bookService.SearchBooksAsync(_searchText, token);

                // Cập nhật giao diện
                BookList = new ObservableCollection<Book>(result);
            }
            catch (TaskCanceledException)
            {
                // Bỏ qua lỗi này (do người dùng gõ tiếp nên hủy cái cũ)
            }
            catch (Exception ex)
            {
            }
           
        }
    }
}