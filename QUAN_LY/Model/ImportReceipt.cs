using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    public class ImportReceipt
    {
        public int Id { get; set; } // import_id
        public int PublisherId { get; set; }
        public int AdminId { get; set; }
        public DateTime ImportDate { get; set; }
        public decimal TotalCost { get; set; }
        public int Status { get; set; } // 0: Preparing, 1: Imported
    }
}
