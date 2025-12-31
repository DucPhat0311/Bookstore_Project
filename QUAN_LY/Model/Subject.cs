using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUAN_LY.Model
{
    [Table("subject")]
    public class Subject
    {
        [Key]
        [Column("subject_id")]
        public int Id { get; set; } // subject_id

        [Column("name")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
