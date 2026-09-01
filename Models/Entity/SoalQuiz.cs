using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("SoalQuiz")]
    public class SoalQuiz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdSoal { get; set; }

        [Required]
        public int IdQuiz { get; set; }

        [Required]
        public string Pertanyaan { get; set; }

        [Required]
        [StringLength(30)]
        public string TipeSoal { get; set; } = "PilihanGanda";

        [ForeignKey("IdQuiz")]
        public virtual Quiz Quiz { get; set; }
    }
}
