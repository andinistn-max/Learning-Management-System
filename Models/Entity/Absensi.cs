using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("Absensi")]
    public class Absensi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAbsen { get; set; }

        [Required]
        public int IdJadwal { get; set; }

        [Required]
        public int IdSiswa { get; set; }

        public DateTime? WaktuAbsen { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        [StringLength(500)]
        public string Catatan { get; set; }

        [ForeignKey("IdJadwal")]
        public virtual JadwalAbsen JadwalAbsen { get; set; }

        [ForeignKey("IdSiswa")]
        public virtual Users Siswa { get; set; }
    }
}
