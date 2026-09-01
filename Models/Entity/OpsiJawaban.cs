using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("OpsiJawaban")]
    public class OpsiJawaban
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdOpsi { get; set; }

        [Required]
        public int IdSoal { get; set; }

        [Required]
        [StringLength(500)]
        public string TeksOpsi { get; set; }

        public bool IsCorrect { get; set; } = false;

        [ForeignKey("IdSoal")]
        public virtual SoalQuiz SoalQuiz { get; set; }
    }
}
