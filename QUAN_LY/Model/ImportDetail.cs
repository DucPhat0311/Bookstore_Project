using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    public class ImportDetail
    {
        public int Id { get; set; } // import_detail_id
        public int ImportId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; } // Giá vốn
    }
}
