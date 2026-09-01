using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("ProgresMateri")]
    public class ProgresMateri
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProgres { get; set; }

        [Required]
        public int IdMateri { get; set; }

        [Required]
        public int IdSiswa { get; set; }

        public bool IsSelesai { get; set; } = false;

        public DateTime? DiselesaikanPada { get; set; }

        [ForeignKey("IdMateri")]
        public virtual Materi Materi { get; set; }

        [ForeignKey("IdSiswa")]
        public virtual Users Siswa { get; set; }
    }
}
