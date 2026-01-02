using Microsoft.Win32;
using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Services;
using QUAN_LY.Utilities; 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; 
using System.Windows;
using System.Windows.Input;

namespace QUAN_LY.ViewModel
{
    public class BookViewModel : BaseViewModel
    {
        private readonly IBookService _bookService;

        // --- BIẾN DỮ LIỆU ---
        private List<Book> _masterList;    // Kho chứa tất cả sách
        private List<Book> _filteredList;  // Danh sách sau khi tìm kiếm & sắp xếp

        // --- PHÂN TRANG ---
        private int _currentPage = 1;
        private int _itemsPerPage = 8;

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
                // Cập nhật trạng thái nút bấm
                (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (PreviousPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // List hiển thị lên View 
        private ObservableCollection<Book> _displayItems;
        public ObservableCollection<Book> DisplayItems
        {
            get => _displayItems;
            set { _displayItems = value; OnPropertyChanged(); }
        }

        private Book _selectedBook;
        public Book SelectedBook
        {
            get => _selectedBook;
            set { _selectedBook = value; OnPropertyChanged(); }
        }

        // --- TÌM KIẾM ---
        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
                ApplyFilters(); 
            }
        }

        // --- SẮP XẾP ---
        public List<string> SortOptions { get; } = new List<string>
        {
            "Mặc định",
            "Giá: Thấp -> Cao",
            "Giá: Cao -> Thấp",
            "Tên: A -> Z"
        };

        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                _selectedSortOption = value;
                OnPropertyChanged();
                ApplySort(); 
            }
        }

        // --- BIẾN LỌC ---
        private Subject _selectedFilterSubject;
        public Subject SelectedFilterSubject
        {
            get => _selectedFilterSubject;
            set
            {
                _selectedFilterSubject = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        private Author _selectedFilterAuthor;
        public Author SelectedFilterAuthor
        {
            get => _selectedFilterAuthor;
            set
            {
                _selectedFilterAuthor = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        private Publisher _selectedFilterPublisher;
        public Publisher SelectedFilterPublisher
        {
            get => _selectedFilterPublisher;
            set
            {
                _selectedFilterPublisher = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public ICommand ClearFilterCommand { get; set; }

        // --- COMBOBOX DATA ---
        private List<Publisher> _publisherList;
        public List<Publisher> PublisherList { get => _publisherList; set { _publisherList = value; OnPropertyChanged(); } }

        private List<Author> _authorList;
        public List<Author> AuthorList { get => _authorList; set { _authorList = value; OnPropertyChanged(); } }

        private List<Subject> _subjectList;
        public List<Subject> SubjectList { get => _subjectList; set { _subjectList = value; OnPropertyChanged(); } }

        // --- COMMANDS ---
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand UploadImageCommand { get; }

        public BookViewModel()
        {
            // Khởi tạo Service 
            _bookService = new BookServiceSQL();
            DisplayItems = new ObservableCollection<Book>();

            LoadData(); // Load lần đầu

            // --- KHỞI TẠO COMMAND ---
            NextPageCommand = new RelayCommand(
                _ => { CurrentPage++; UpdatePagination(); },
                _ => CanNext()
            );

            PreviousPageCommand = new RelayCommand(
                _ => { CurrentPage--; UpdatePagination(); },
                _ => CurrentPage > 1
            );

            SearchCommand = new RelayCommand(_ => ApplyFilters());

            // Command Thêm
            AddCommand = new RelayCommand<object>(
                (p) =>
                {
                    if (SelectedBook == null || !SelectedBook.IsValid())
                    {
                        MessageBox.Show("Thông tin chưa hợp lệ. Vui lòng kiểm tra các ô báo đỏ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (_bookService.AddBook(SelectedBook))
                    {
                        MessageBox.Show("Thêm thành công!");
                        LoadData();
                    }
                },
                (p) =>
                {
                    if (SelectedBook == null || !string.IsNullOrEmpty(SelectedBook.Error)) return false; // Check sơ bộ
                    if (SelectedBook != null && SelectedBook.Id > 0) return false;
                    return true;
                }
            );

            // Command Sửa
            EditCommand = new RelayCommand<object>(
                (p) =>
                {
                    if (SelectedBook == null || !SelectedBook.IsValid())
                    {
                        MessageBox.Show("Thông tin chưa chính xác!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (_bookService.UpdateBook(SelectedBook))
                    {
                        MessageBox.Show("Cập nhật thành công!");
                        LoadData();
                    }
                    else MessageBox.Show("Cập nhật thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                },
                (p) => SelectedBook != null && SelectedBook.Id > 0
            );

            // Command Xóa
            DeleteCommand = new RelayCommand<object>(
                (p) =>
                {
                    if (MessageBox.Show("Bạn có chắc muốn xóa?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (_bookService.DeleteBook(SelectedBook.Id))
                        {
                            MessageBox.Show("Đã xóa!");
                            LoadData();
                        }
                    }
                },
                (p) => SelectedBook != null && SelectedBook.Id > 0
            );

            ClearCommand = new RelayCommand<object>(
                (p) => { SelectedBook = new Book(); SelectedBook.ImageUrl = null; },
                (p) => true
            );

            UploadImageCommand = new RelayCommand<object>(
                (p) =>
                {
                    OpenFileDialog dlg = new OpenFileDialog { Filter = "Image files (*.jpg, *.png)|*.jpg;*.png" };
                    if (dlg.ShowDialog() == true)
                    {
                        SelectedBook.ImageUrl = dlg.FileName;
                        OnPropertyChanged(nameof(SelectedBook));
                    }
                }
            );
            ClearFilterCommand = new RelayCommand(
        _ =>
        {
            // Reset tất cả về null/rỗng
            SearchKeyword = string.Empty;
            SelectedFilterSubject = null;
            SelectedFilterAuthor = null;
            SelectedFilterPublisher = null;
            SelectedSortOption = "Mặc định";
            ApplyFilters();
        }
    );
        }

        // --- CÁC HÀM XỬ LÝ LOGIC ---

        void LoadData()
        {
            _masterList = _bookService.GetAllBooksForManagement();

            // Load ComboBox data
            PublisherList = _bookService.GetAllPublishers();
            AuthorList = _bookService.GetAllAuthors();
            SubjectList = _bookService.GetAllSubjects();

            SelectedBook = new Book();

            // Reset danh sách lọc = danh sách gốc
            _filteredList = new List<Book>(_masterList);

            // Đặt mặc định sắp xếp
            SelectedSortOption = "Mặc định";

            // Tìm kiếm (nếu có từ khóa cũ) -> Sắp xếp -> Phân trang
            ApplyFilters();
        }

      

        // Hàm xử lý trung tâm: Tìm kiếm + Lọc + Sắp xếp
        private void ApplyFilters()
        {
            // Bắt đầu từ danh sách gốc
            var query = _masterList.AsEnumerable();

            // Lọc theo TỪ KHÓA TÌM KIẾM
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                string key = SearchKeyword.ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(key));
            }

            // Lọc theo THỂ LOẠI
            if (SelectedFilterSubject != null)
            {
                query = query.Where(x => x.SubjectId == SelectedFilterSubject.Id);
            }

            // Lọc theo TÁC GIẢ
            if (SelectedFilterAuthor != null)
            {
                query = query.Where(x => x.AuthorId == SelectedFilterAuthor.Id);
            }

            // Lọc theo NHÀ XUẤT BẢN
            if (SelectedFilterPublisher != null)
            {
                query = query.Where(x => x.PublisherId == SelectedFilterPublisher.Id);
            }

            // Lưu kết quả lọc tạm thời
            _filteredList = query.ToList();

            // Gọi hàm Sắp xếp (Hàm này sẽ gọi tiếp Phân trang)
            ApplySort();
        }

        // SẮP XẾP
        private void ApplySort()
        {
            if (_filteredList == null) return;

            switch (SelectedSortOption)
            {
                case "Giá: Thấp -> Cao":
                    _filteredList = _filteredList.OrderBy(x => x.Price).ToList();
                    break;
                case "Giá: Cao -> Thấp":
                    _filteredList = _filteredList.OrderByDescending(x => x.Price).ToList();
                    break;
                case "Tên: A -> Z":
                    _filteredList = _filteredList.OrderBy(x => x.Title).ToList();
                    break;
                default: 
                    _filteredList = _filteredList.OrderBy(x => x.Id).ToList();
                    break;
            }

            // Sau khi sắp xếp xong thì Reset về trang 1 và hiển thị
            CurrentPage = 1;
            UpdatePagination();
        }

        // PHÂN TRANG 
        void UpdatePagination()
        {
            if (_filteredList == null) return;

            DisplayItems.Clear();

            // Skip và Take dựa trên _filteredList đã được Tìm kiếm và Sắp xếp
            var items = _filteredList.Skip((CurrentPage - 1) * _itemsPerPage).Take(_itemsPerPage).ToList();

            foreach (var item in items)
            {
                DisplayItems.Add(item);
            }
        }

        bool CanNext()
        {
            if (_filteredList == null) return false;
            return CurrentPage * _itemsPerPage < _filteredList.Count;
        }

    } 
} 