using Microsoft.EntityFrameworkCore;
using QUAN_LY.Model;

namespace QUAN_LY.Model
{
	public class BookStoreDbContext : DbContext
	{
		public DbSet<Book> Books { get; set; }
        public DbSet<Publisher> Publishers { get; set; }

        public DbSet<Author> Authors { get; set; }

        public DbSet<Subject> Subjects { get; set; }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<ImportDetail> ImportDetails { get; set; }
        public DbSet<ImportReceipt> ImportReceipts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			

			string connectionString = "Server=localhost;" + // Host
									  "Port=3306;" +         // Port
									  "Database=bookstore;" +  // Tên Database
									  "Uid=root;" +    // Username
									  "Pwd=123456;";   // Password

			optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
            // Đảm bảo tên bảng khớp với database (tránh lỗi in hoa)
            modelBuilder.Entity<Admin>().ToTable("admin");
            modelBuilder.Entity<Author>().ToTable("author");
            modelBuilder.Entity<Book>().ToTable("book");
            modelBuilder.Entity<ImportDetail>().ToTable("importdetail");
            modelBuilder.Entity<ImportReceipt>().ToTable("importreceipt");
            modelBuilder.Entity<Order>().ToTable("order");
            modelBuilder.Entity<OrderItem>().ToTable("orderitem");
            modelBuilder.Entity<Publisher>().ToTable("publisher");
            modelBuilder.Entity<Subject>().ToTable("subject");
        }
	}
}