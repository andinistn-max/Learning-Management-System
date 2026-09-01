using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("Quiz")]
    public class Quiz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdQuiz { get; set; }

        [Required]
        public int IdKelas { get; set; }

        [Required]
        public int PertemuanKe { get; set; }

        [Required]
        [StringLength(200)]
        public string JudulQuiz { get; set; }

        public string Deskripsi { get; set; }

        public int? Durasi { get; set; }

        public decimal PassingScore { get; set; } = 75m;

        public DateTime? WaktuMulai { get; set; }

        public DateTime? WaktuSelesai { get; set; }

        public bool IsActive { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdKelas")]
        public virtual Kelas Kelas { get; set; }
    }
}
