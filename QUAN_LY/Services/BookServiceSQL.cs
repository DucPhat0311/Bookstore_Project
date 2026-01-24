using Microsoft.EntityFrameworkCore;
using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using QUAN_LY_APP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading; 

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

        public bool UpdateBook(Book book)
        {
            try
            {
                var dbBook = _context.Books.Find(book.Id);

                if (dbBook == null) return false;


                dbBook.Title = book.Title;

                dbBook.AuthorId = book.AuthorId; 

                dbBook.SubjectId = book.SubjectId;

                dbBook.PublisherId = book.PublisherId;

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

        public List<Author> GetAllAuthors()
        {
            return _context.Authors.ToList();
        }

        public List<Subject> GetAllSubjects()
        {
            return _context.Subjects.ToList();
        }

        public Book GetBookById(int id)
        {
            return _context.Books.Include(b => b.Author)
                           .FirstOrDefault(b => b.Id == id);
        }

        public List<Book> GetAllBooksForManagement()
        {
            return _context.Books.Where(b => !b.IsDeleted).ToList();
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            using (var context = new BookStoreDbContext())
            {
                return await context.Books.AsNoTracking().Include(b => b.Author).ToListAsync();
            }
        }

        public async Task<List<Book>> SearchBooksAsync(string keyword, CancellationToken token)
        {
            using (var context = new BookStoreDbContext())
            {
                var query = context.Books.AsNoTracking().Include(b => b.Author).AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(b => b.Title.Contains(keyword));
                }

                return await query.Take(10).ToListAsync(token);
            }
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            using (var context = new BookStoreDbContext())
            {
                var stats = new DashboardStats();

                var startOfDay = DateTime.Today;
                var endOfDay = startOfDay.AddDays(1);

                // Tính doanh thu
                stats.Revenue = await context.Orders
                    .Where(x => x.OrderDate >= startOfDay && x.OrderDate < endOfDay)
                    .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

                // Đếm đơn hàng
                stats.OrdersCount = await context.Orders
                    .CountAsync(x => x.OrderDate >= startOfDay && x.OrderDate < endOfDay);

                // Tính sách bán
                var queryBooksSold = from o in context.Orders
                                     join d in context.OrderItems on o.Id equals d.OrderId
                                     where o.OrderDate >= startOfDay && o.OrderDate < endOfDay
                                     select (int?)d.Quantity;

                stats.BooksSold = await queryBooksSold.SumAsync() ?? 0;

                // Cảnh báo tồn kho 
                stats.LowStockCount = await context.Books
                    .CountAsync(x => x.Quantity < 10 && !x.IsDeleted);

                return stats;
            }
        }


        public List<Book> GetAllBooksForPOS()
        {
            throw new NotImplementedException();
        }

        public List<Book> SearchBooksForPOS(string keyword)
        {
            throw new NotImplementedException();
        }
    }
}
  