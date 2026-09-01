using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("NilaiQuiz")]
    public class NilaiQuiz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAttempt { get; set; }

        [Required]
        public int IdQuiz { get; set; }

        [Required]
        public int IdSiswa { get; set; }

        public decimal? Score { get; set; }

        public DateTime? WaktuMulai { get; set; }

        public DateTime? WaktuSubmit { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "InProgress";

        [ForeignKey("IdQuiz")]
        public virtual Quiz Quiz { get; set; }

        [ForeignKey("IdSiswa")]
        public virtual Users Siswa { get; set; }
    }
}
