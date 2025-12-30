using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QUAN_LY.Model;

namespace QUAN_LY.Interfaces
{
    public interface IBookService
    {
        List<Book> GetAllBooksForManagement(); // tiêu chí lấy tất cả sách của method ở trang quản lý sách khác với method ở POS (trang bán hàng)
        List<Book> GetAllBooksForPOS(); // method này chỉ lấy sách chưa xóa và số lượng lớn hơn 0 khác với cái trên

        Book GetBookById(int id); 
        bool AddBook(Book book);
        bool UpdateBook(Book book);
        bool DeleteBook(int id);

        List<Publisher> GetAllPublishers();

        List<Book> SearchBooksForManagement(string keyword); // hàm tìm kiếm sách cho trang quản lý sách

        List<Book> SearchBooksForPOS(string keyword); // hàm tìm kiếm sách cho trang POS


    }
}
