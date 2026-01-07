using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUAN_LY.Model
{
    [Table("orderitem")]
    public class OrderItem
    {
        [Key]
        [Column("order_item_id")] // Hoặc 'id', tùy DB của bạn
        public int Id { get; set; }

        [Column("order_id")]
        public int? OrderId { get; set; }

        [Column("book_id")]
        public int? BookId { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("unit_price")] // mới đầu chỗ này ánh xạ sai ghi price
        public decimal? Price { get; set; }
    }

}
