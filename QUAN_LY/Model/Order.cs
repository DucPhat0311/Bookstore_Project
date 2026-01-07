using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    [Table("order")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int Id { get; set; }

        [Column("admin_id")]
        public int? AdminId { get; set; }

        [Column("order_date")]
        public DateTime? OrderDate { get; set; }

        [Column("total_amount")]
        public decimal? TotalAmount { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [Column("note")]
        public string? Note { get; set; }
    }
}

