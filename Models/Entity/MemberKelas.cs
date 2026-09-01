using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("MemberKelas")]
    public class MemberKelas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdMember { get; set; }

        [Required]
        public int IdSiswa { get; set; }

        [Required]
        public int IdKelas { get; set; }

        public DateTime TglJoin { get; set; } = DateTime.Now;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Active";

        [ForeignKey("IdSiswa")]
        public virtual Users Siswa { get; set; }

        [ForeignKey("IdKelas")]
        public virtual Kelas Kelas { get; set; }
    }
}
