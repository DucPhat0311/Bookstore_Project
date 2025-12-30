using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    [Table("publisher")]
    public class Publisher
    {
        [Key]
        [Column("publisher_id")] 
        public int Id { get; set; } // publisher_id

        [Column("name")]
        [Required] 
        [StringLength(255)] 
        public string Name { get; set; }

        [Column("phone")]
        [StringLength(20)]
        public string Phone { get; set; }

        [Column("address")]
        [StringLength(255)]
        public string Address { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
