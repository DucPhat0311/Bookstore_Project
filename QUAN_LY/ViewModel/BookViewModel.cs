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

        private List<Book> _masterList;    // biến này chứa tất cả các sách
        private List<Book> _filteredList;  // biến này chứa các sách sau khi đã lọc và tìm kiếm 

        private int _currentPage = 1; // trang hiện tại
        private int _itemsPerPage = 8; // số sách hiện lên trên mỗi trang

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
                (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (PreviousPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

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

        // 3 cái COMBOBOX cho Publisher, Author, Subject
        private List<Publisher> _publisherList;
        public List<Publisher> PublisherList { get => _publisherList; set { _publisherList = value; OnPropertyChanged(); } }

        private List<Author> _authorList;
        public List<Author> AuthorList { get => _authorList; set { _authorList = value; OnPropertyChanged(); } }

        private List<Subject> _subjectList;
        public List<Subject> SubjectList { get => _subjectList; set { _subjectList = value; OnPropertyChanged(); } }

        // Các nút Command
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
            _bookService = new BookServiceSQL();
            DisplayItems = new ObservableCollection<Book>();

            LoadData(); 

            NextPageCommand = new RelayCommand(
                _ => { CurrentPage++; UpdatePagination(); },
                _ => CanNext()
            );

            PreviousPageCommand = new RelayCommand(
                _ => { CurrentPage--; UpdatePagination(); },
                _ => CurrentPage > 1
            );

            SearchCommand = new RelayCommand(_ => ApplyFilters());

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
            // Reset tất cả các bộ lọc về mặc định
            SearchKeyword = string.Empty;
            SelectedFilterSubject = null;
            SelectedFilterAuthor = null;
            SelectedFilterPublisher = null;
            SelectedSortOption = "Mặc định";
            ApplyFilters();
        }
    );
        }

        // Load dữ liệu ban đầu
        void LoadData()
        {
            _masterList = _bookService.GetAllBooksForManagement();

            PublisherList = _bookService.GetAllPublishers();
            AuthorList = _bookService.GetAllAuthors();
            SubjectList = _bookService.GetAllSubjects();

            SelectedBook = new Book();

            _filteredList = new List<Book>(_masterList);

            SelectedSortOption = "Mặc định";

            ApplyFilters();
        }


        // Tìm kiếm
        private void ApplyFilters()
        {
            var query = _masterList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                string key = SearchKeyword.ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(key));
            }

            if (SelectedFilterSubject != null)
            {
                query = query.Where(x => x.SubjectId == SelectedFilterSubject.Id);
            }

            if (SelectedFilterAuthor != null)
            {
                query = query.Where(x => x.AuthorId == SelectedFilterAuthor.Id);
            }

            if (SelectedFilterPublisher != null)
            {
                query = query.Where(x => x.PublisherId == SelectedFilterPublisher.Id);
            }

            _filteredList = query.ToList();

            ApplySort();
        }

        // Sắp xếp
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

            // Sắp xếp xong ==> Reset về trang 1
            CurrentPage = 1;
            UpdatePagination();
        }


        // Phân trang
        void UpdatePagination()
        {
            if (_filteredList == null) return;

            DisplayItems.Clear();

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