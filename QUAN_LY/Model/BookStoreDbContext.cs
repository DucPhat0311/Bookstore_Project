using Microsoft.EntityFrameworkCore;
using QUAN_LY.Model;

namespace QUAN_LY.Model
{
	public class BookStoreDbContext : DbContext
	{
		public DbSet<Book> Books { get; set; }
        public DbSet<Publisher> Publishers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// 1. Chuỗi kết nối đã được convert từ thông tin Java của bạn
			// JDBC URL: jdbc:mysql://mysql-c80eef2-ducphat0311-1c2c.i.aivencloud.com:24156/movie

			string connectionString = "Server=localhost;" + // Host
									  "Port=3306;" +                                           // Port
									  "Database=bookstore;" +                                       // Tên Database
									  "Uid=root;" +                                         // Username
									  "Pwd=123456;";                        // Password

			// 2. Sử dụng UseMySql (Thay vì UseSqlServer)
			// Cần hàm AutoDetect để nó tự nhận diện phiên bản MySQL
			optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}