using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace QUAN_LY.Model
{
    [Table("book")]
    public class Book
    {
        [Key]
        [Column("book_id")] 
        public int Id { get; set; }

        [Column("title")]
        [Required] 
        [StringLength(255)] 
        public string Title { get; set; }

        [Column("publisher_id")]
        public int PublisherId { get; set; } 

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
        public string ImageUrl { get; set; } 

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } 

        public string FullImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(ImageUrl))
                    return "https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg";

                if (ImageUrl.StartsWith("http")) return ImageUrl;

                string relativePath = ImageUrl.Replace("/", "\\"); 

                string currentFolder = AppDomain.CurrentDomain.BaseDirectory;

                for (int i = 0; i < 5; i++)
                {
                    string tryPath = Path.Combine(currentFolder, relativePath);

                    if (File.Exists(tryPath))
                    {
                        return tryPath;
                    }

                    var parent = Directory.GetParent(currentFolder);
                    if (parent == null) break; 
                    currentFolder = parent.FullName;
                }

                return "https://upload.wikimedia.org/wikipedia/commons/1/14/No_Image_Available.jpg";
            }
        }
    }
    }


