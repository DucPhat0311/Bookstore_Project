using QUAN_LY.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace QUAN_LY.Model
{

    [Table("book")]
    public class Book : BaseViewModel
    {
        [Key]
        [Column("book_id")]
        public int Id { get; set; } 

        private string _title;
        [Column("title")]
        [Required(ErrorMessage = "Tên sách không được để trống")] 
        [StringLength(255)]
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); } 
        }

        private int _authorId;
        [Column("author_id")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Tác giả")] 
        public int AuthorId
        {
            get => _authorId;
            set { _authorId = value; OnPropertyChanged(); }
        }

        private int _subjectId;
        [Column("subject_id")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Thể loại")]
        public int SubjectId
        {
            get => _subjectId;
            set { _subjectId = value; OnPropertyChanged(); }
        }

        private int _publisherId;
        [Column("publisher_id")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn NXB")]
        public int PublisherId
        {
            get => _publisherId;
            set { _publisherId = value; OnPropertyChanged(); }
        }

        private decimal _price;
        [Column("price")]
        [Range(typeof(decimal), "10000", "1000000", ErrorMessage = "Giá bán phải từ 1,000đ tới 1,000,000đ")]
        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }


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

        public string StatusText
        {
            get
            {
                if (Quantity <= 0) return "Hết hàng";
                if (Quantity < 10) return "Sắp hết";
                return "Còn hàng";
            }
        }

        [ForeignKey("AuthorId")]
        public virtual Author Author { get; set; } 

        public string AuthorName => Author?.Name ?? "Không rõ";

    }
    }


