using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("StreamPostingan")]
    public class StreamPostingan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdStream { get; set; }

        [Required]
        public int IdKelas { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required]
        public string Pesan { get; set; }

        [StringLength(500)]
        public string AttachmentUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("IdKelas")]
        public virtual Kelas Kelas { get; set; }

        [ForeignKey("IdUser")]
        public virtual Users User { get; set; }
    }
}
