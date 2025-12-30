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
		}
	}
}