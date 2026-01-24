using QUAN_LY.Model;
using QUAN_LY_APP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Interfaces
{
    public interface IBookService
    {
        bool AddBook(Book book);
        bool UpdateBook(Book book);
        bool DeleteBook(int id);
        List<Publisher> GetAllPublishers();
        List<Author> GetAllAuthors();
        List<Subject> GetAllSubjects();
        Book GetBookById(int id);
        List<Book> GetAllBooksForManagement(); // tiêu chí lấy tất cả sách của method ở trang quản lý sách khác với method ở POS (trang bán hàng)
        Task<List<Book>> GetAllBooksAsync();

        Task<DashboardStats> GetDashboardStatsAsync();

        public int GetOrCreateAuthor(string name);

        public int GetOrCreatePublisher(string name);

        public int GetOrCreateSubject(string name);


        List<Book> GetAllBooksForPOS(); // method này chỉ lấy sách chưa xóa và số lượng lớn hơn 0 khác với cái trên
        List<Book> SearchBooksForPOS(string keyword); // hàm tìm kiếm sách cho trang POS



    }
}
