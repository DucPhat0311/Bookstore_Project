using QUAN_LY.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Windows.Input;

namespace QUAN_LY.Model
{

    [Table("book")]
    public class Book : BaseViewModel
    {
        // isbn
        private string _isbn;
        [Column("isbn")] 
        public string Isbn
        {
            get => _isbn;
            set { _isbn = value; OnPropertyChanged(); }
        }

        // book_id
        [Key]
        [Column("book_id")]
        public int Id { get; set; } 

        private string _title;
        [Column("title")]
        [Required(ErrorMessage = "Tên sách không được để trống")] 
        [StringLength(255)]

        // title
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); } 
        }

        // author_id
        private int _authorId;
        [Column("author_id")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Tác giả")] 
        public int AuthorId
        {
            get => _authorId;
            set { _authorId = value; OnPropertyChanged(); }
        }

        // subject_id
        private int _subjectId;
        [Column("subject_id")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Thể loại")]
        public int SubjectId
        {
            get => _subjectId;
            set { _subjectId = value; OnPropertyChanged(); }
        }

        // publisher_id
        private int _publisherId;
        [Column("publisher_id")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn NXB")]
        public int PublisherId
        {
            get => _publisherId;
            set { _publisherId = value; OnPropertyChanged(); }
        }

        // price
        private decimal _price;
        [Column("price")]
        [Range(typeof(decimal), "10000", "1000000", ErrorMessage = "Giá bán phải từ 1,000đ tới 1,000,000đ")]
        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        // quantity
        [Column("quantity")]
        public int Quantity { get; set; }

        // image_url
        [Column("image_url")]
        public string ImageUrl { get; set; }

        //  is_deleted
        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        // các thuộc tính không ánh xạ
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

        [NotMapped]
        public ICommand AutoFillCommand { get; set; }

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


