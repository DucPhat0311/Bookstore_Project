using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows; 
using System.Windows.Input;

namespace QUAN_LY.ViewModel
{
    public class BookViewModel : BaseViewModel
    {
        private readonly IBookService _bookService;

        private List<Book> _bookList;
        public List<Book> BookList
        {
            get => _bookList;
            set { _bookList = value; OnPropertyChanged(); }
        }

        private Book _selectedBook;
        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged();
            }
        }

        private List<Publisher> _publisherList;
        public List<Publisher> PublisherList
        {
            get => _publisherList;
            set { _publisherList = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public BookViewModel()
        {
            _bookService = new Services.BookServiceSQL();
            SelectedBook = new Book(); 
            LoadData();

            AddCommand = new RelayCommand<object>(
                (p) =>
                {
                    var isSuccess = _bookService.AddBook(SelectedBook);
                    if (isSuccess)
                    {
                        LoadData();
                        MessageBox.Show("Thêm thành công!");
                        SelectedBook = new Book();
                    }
                },
                (p) =>
                {
                    if (SelectedBook != null && SelectedBook.Id > 0) return false;

                    if (SelectedBook == null || string.IsNullOrEmpty(SelectedBook.Title)) return false;

                    return true;
                }
            );

            EditCommand = new RelayCommand<object>(
                (p) =>
                {
                    var isSuccess = _bookService.UpdateBook(SelectedBook);
                    if (isSuccess)
                    {
                        LoadData();
                        MessageBox.Show("Cập nhật thành công!");
                    }
                },
                (p) =>
                {
                    if (SelectedBook == null || SelectedBook.Id == 0) return false;
                    return true;
                }
            );

            DeleteCommand = new RelayCommand<object>(
                (p) =>
                {
                    var result = MessageBox.Show("Bạn có chắc muốn xóa?", "Xác nhận", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        var isSuccess = _bookService.DeleteBook(SelectedBook.Id);
                        if (isSuccess)
                        {
                            LoadData();
                            SelectedBook = new Book(); 
                            MessageBox.Show("Đã xóa!");
                        }
                    }
                },
                (p) =>
                {
                    if (SelectedBook == null || SelectedBook.Id == 0) return false;
                    return true;
                }
            );

            ClearCommand = new RelayCommand<object>(
                (p) =>
                {
                    SelectedBook = new Book(); 
                },
                (p) => true 
            );
        }

        void LoadData()
        {
            BookList = _bookService.GetAllBooks();
            PublisherList = _bookService.GetAllPublishers();
        }
    }
}