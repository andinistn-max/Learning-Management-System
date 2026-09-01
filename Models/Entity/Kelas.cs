using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("Kelas")]
    public class Kelas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdKelas { get; set; }

        [Required]
        public int IdKategori { get; set; }

        [Required]
        public int IdGuru { get; set; }

        [Required]
        [StringLength(200)]
        public string NamaKelas { get; set; }

        public string Deskripsi { get; set; }

        [StringLength(500)]
        public string Thumbnail { get; set; }

        [StringLength(30)]
        public string Level { get; set; }

        public int? Durasi { get; set; }

        public bool IsPublish { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdKategori")]
        public virtual Kategori Kategori { get; set; }

        [ForeignKey("IdGuru")]
        public virtual Users Guru { get; set; }
    }
}
