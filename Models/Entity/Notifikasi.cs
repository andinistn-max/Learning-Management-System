using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("Notifikasi")]
    public class Notifikasi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdNotif { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required]
        [StringLength(200)]
        public string Judul { get; set; }

        [StringLength(500)]
        public string Pesan { get; set; }

        [StringLength(50)]
        public string TipeNotif { get; set; }

        [StringLength(500)]
        public string UrlTujuan { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("IdUser")]
        public virtual Users User { get; set; }
    }
}
