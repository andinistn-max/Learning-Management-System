using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("KomentarStream")]
    public class KomentarStream
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdKomentar { get; set; }

        [Required]
        public int IdStream { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required]
        public string Komentar { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("IdStream")]
        public virtual StreamPostingan Stream { get; set; }

        [ForeignKey("IdUser")]
        public virtual Users User { get; set; }
    }
}
