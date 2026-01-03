using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;       // Thêm dòng này (cho [Key])
using System.ComponentModel.DataAnnotations.Schema; // Thêm dòng này (cho [Column])
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    // Nếu tên bảng trong DB khác tên class, bạn có thể thêm [Table("ten_bang")]
    [Table("ImportDetail")]
    public class ImportDetail
    {
        [Key]
        [Column("import_detail_id")] // Ánh xạ với cột khóa chính trong DB
        public int id { get; set; }

        [Column("import_id")] // Ánh xạ với cột import_id
        public int importId { get; set; }

        [Column("book_id")] // Ánh xạ với cột book_id
        public int BookId { get; set; }

        [Column("quantity")] // Ánh xạ với cột quantity
        public int quantity { get; set; }

        [Column("import_price")] // Ánh xạ với cột import_price (Giá vốn)
        public decimal importPrice { get; set; }
    }
}