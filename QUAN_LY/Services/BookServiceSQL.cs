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

        public List<Book> GetAllBooksForManagement()
        {
            return _context.Books.Where(b => !b.IsDeleted).ToList();
        }

        public Book GetBookById(int id)
        {
            return _context.Books.Find(id);
        }

        public bool UpdateBook(Book book)
        {
            try
            {
                var dbBook = _context.Books.Find(book.Id);

                if (dbBook == null) return false; 


                dbBook.Title = book.Title;

                dbBook.PublisherId = book.PublisherId;

                dbBook.YearPublished = book.YearPublished;

                dbBook.Edition = book.Edition;

                dbBook.Price = book.Price;

                dbBook.Quantity = book.Quantity;

                dbBook.ImageUrl = book.ImageUrl;

                // lưu xuống database
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

        public List<Book> GetAllBooksForPOS()
        {
            throw new NotImplementedException();
        }

        public List<Book> SearchBooksForManagement(string keyword)
        {
            throw new NotImplementedException();
        }

        public List<Book> SearchBooksForPOS(string keyword)
        {
            throw new NotImplementedException();
        }
    }
}