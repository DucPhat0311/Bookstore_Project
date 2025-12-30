//using QUAN_LY.Interfaces;
//using QUAN_LY.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//namespace QUAN_LY.Services
//{
//    public class BookServiceAPI : IBookService
//    {
//        public bool AddBook(Book book)
//        {
//            throw new NotImplementedException();
//        }

//        public bool DeleteBook(int id)
//        {
//            throw new NotImplementedException();
//        }

//        public List<Book> GetAllBooks()
//        {
//            // Không còn SQL nữa, chỉ còn gọi điện thôi
//            var json = _httpClient.GetStringAsync("https://server/api/books").Result;
//            return JsonConvert.DeserializeObject<List<Book>>(json);
//        }

//        public Book GetBookById(int id)
//        {
//            throw new NotImplementedException();
//        }

//        public bool UpdateBook(Book book)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
