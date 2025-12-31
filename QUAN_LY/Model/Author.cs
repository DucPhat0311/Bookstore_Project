using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    [Table("author")]
    public class Author
    {
        [Key]
        [Column("author_id")]
        public int Id { get; set; } // author_id

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
    }
}
