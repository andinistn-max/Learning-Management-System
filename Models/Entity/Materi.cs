using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("Materi")]
    public class Materi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdMateri { get; set; }

        [Required]
        public int IdKelas { get; set; }

        [Required]
        public int PertemuanKe { get; set; }

        [Required]
        [StringLength(200)]
        public string JudulMateri { get; set; }

        public string Deskripsi { get; set; }

        [StringLength(255)]
        public string NamaFile { get; set; }

        [StringLength(500)]
        public string FilePath { get; set; }

        [StringLength(50)]
        public string TipeMateri { get; set; }

        public long? FileSize { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdKelas")]
        public virtual Kelas Kelas { get; set; }
    }
}
