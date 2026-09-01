using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("Tugas")]
    public class Tugas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdTugas { get; set; }

        [Required]
        public int IdKelas { get; set; }

        [Required]
        public int PertemuanKe { get; set; }

        [Required]
        [StringLength(200)]
        public string JudulTugas { get; set; }

        public string Deskripsi { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [StringLength(500)]
        public string FileAttachment { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdKelas")]
        public virtual Kelas Kelas { get; set; }
    }
}
