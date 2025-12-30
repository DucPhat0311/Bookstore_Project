using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace QUAN_LY.Model
{
    // 1. Map tên bảng (SQL là "Book", không có 's')
    [Table("book")]
    public class Book
    {
        [Key]
        [Column("book_id")] // Tên cột trong SQL
        public int Id { get; set; }

        [Column("title")]
        [Required] // Bắt buộc nhập (NOT NULL)
        [StringLength(255)] // Khớp với VARCHAR(255)
        public string Title { get; set; }

        [Column("publisher_id")]
        public int PublisherId { get; set; } // Dùng int? (nullable) để tránh lỗi nếu sách chưa có NXB

        [Column("year_published")]
        public int YearPublished { get; set; }

        [Column("edition")]
        [StringLength(50)]
        public string Edition { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("image_url")]
        public string ImageUrl { get; set; } // SQL là TEXT hoặc VARCHAR

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } // SQL là BIT hoặc BOOLEAN

        // --- PHẦN MỞ RỘNG (NAVIGATION PROPERTY) ---
        // Để sau này có thể chấm: book.Publisher.Name
        // Bạn cần tạo class Publisher trước rồi mới uncomment dòng dưới

        // [ForeignKey("PublisherId")]
        // public virtual Publisher Publisher { get; set; }

        public string FullImageSource
        {
            get
            {
                // 1. Kiểm tra cơ bản
                if (string.IsNullOrEmpty(ImageUrl))
                    return "https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg";

                if (ImageUrl.StartsWith("http")) return ImageUrl;

                // 2. Chuan hoa duong dan (Doi / thanh \ cho dung Windows)
                string relativePath = ImageUrl.Replace("/", "\\"); // VD: "Images\Book1.jpg"

                // 3. Bat dau tim tu thu muc dang chay (.exe)
                string currentFolder = AppDomain.CurrentDomain.BaseDirectory;

                // 4. Vong lap tim kiem: Quet tu thu muc bin ra ngoai toi da 5 cap thu muc
                // Tai sao 5? Vi thuong project chi sau: bin\Debug\net6.0... (khoang 3-4 cap)
                for (int i = 0; i < 5; i++)
                {
                    // Tao duong dan thu: currentFolder + Images\Book1.jpg
                    string tryPath = Path.Combine(currentFolder, relativePath);

                    // Neu tim thay -> Tra ve ngay!
                    if (File.Exists(tryPath))
                    {
                        return tryPath;
                    }

                    // Neu khong thay -> Lui ra thu muc cha
                    var parent = Directory.GetParent(currentFolder);
                    if (parent == null) break; // Da lui ra toi o C:\ -> Dung
                    currentFolder = parent.FullName;
                }

                // 5. Neu quet het moi noi van khong thay -> Tra ve anh loi
                return "https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg";
            }
        }
    }
    }


