using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("JadwalAbsen")]
    public class JadwalAbsen
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdJadwal { get; set; }

        [Required]
        public int IdKelas { get; set; }

        [Required]
        public int PertemuanKe { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Tanggal { get; set; }

        [Required]
        public TimeSpan JamMulai { get; set; }

        [Required]
        public TimeSpan JamSelesai { get; set; }

        [StringLength(20)]
        public string TokenPresensi { get; set; }

        public bool IsOpen { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdKelas")]
        public virtual Kelas Kelas { get; set; }
    }
}
