using System;
using System.Collections.Generic;
using System.Linq;
using QUAN_LY.Model;
using QUAN_LY.Interfaces;

namespace QUAN_LY.Services
{
    public class BookServiceSQL : IBookService
    {
        private readonly BookStoreDbContext _context;

        public BookServiceSQL()
        {
            _context = new BookStoreDbContext();
        }

        public bool AddBook(Book book)
        {
            try
            {
                book.IsDeleted = false;
                _context.Books.Add(book);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeleteBook(int id)
        {
            try
            {
                var book = _context.Books.Find(id);
                if (book == null) return false;

                book.IsDeleted = true;

                return _context.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public List<Book> GetAllBooks()
        {
            return _context.Books.Where(b => !b.IsDeleted).ToList();
        }

        // 4. LẤY 1 CUỐN THEO ID
        public Book GetBookById(int id)
        {
            return _context.Books.Find(id);
        }

        public bool UpdateBook(Book book)
        {
            try
            {
                // Bước 1: Tìm cuốn sách cũ trong Database bằng ID
                var dbBook = _context.Books.Find(book.Id);

                if (dbBook == null) return false; // Không tìm thấy để sửa

                // Bước 2: Gán giá trị mới vào (Mapping theo đúng cột SQL bạn đưa)

                // cột: title
                dbBook.Title = book.Title;

                // cột: publisher_id
                dbBook.PublisherId = book.PublisherId;

                // cột: year_published
                dbBook.YearPublished = book.YearPublished;

                // cột: edition
                dbBook.Edition = book.Edition;

                // cột: price
                dbBook.Price = book.Price;

                // cột: quantity
                dbBook.Quantity = book.Quantity;

                // cột: image_url
                dbBook.ImageUrl = book.ImageUrl;

                // Lưu ý: Không update IsDeleted ở đây vì hàm này dùng để sửa thông tin

                // Bước 3: Lưu xuống DB
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<Publisher> GetAllPublishers()
        {
            return _context.Publishers.Where(p => p.IsActive).ToList();
        }
    }
}