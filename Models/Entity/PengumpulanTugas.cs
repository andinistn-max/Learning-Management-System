using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("PengumpulanTugas")]
    public class PengumpulanTugas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdKumpul { get; set; }

        [Required]
        public int IdTugas { get; set; }

        [Required]
        public int IdSiswa { get; set; }

        [StringLength(500)]
        public string FilePath { get; set; }

        public DateTime? WaktuKumpul { get; set; }

        public decimal? Nilai { get; set; }

        public string Feedback { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Submitted";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdTugas")]
        public virtual Tugas Tugas { get; set; }

        [ForeignKey("IdSiswa")]
        public virtual Users Siswa { get; set; }
    }
}
