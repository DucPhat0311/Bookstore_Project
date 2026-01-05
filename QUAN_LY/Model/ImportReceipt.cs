using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUAN_LY.Model
{
    [Table("importreceipt")]
    public class ImportReceipt
    {
        [Key]
        [Column("import_id")] // Kiểm tra trong DB xem là 'id' hay 'import_id' nhé
        public int Id { get; set; }

        [Column("publisher_id")]
        public int PublisherId { get; set; }

        [Column("admin_id")]
        public int? AdminId { get; set; }

        [Column("import_date")]
        public DateTime ImportDate { get; set; }

        [Column("total_cost")]
        public decimal TotalCost { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
}
