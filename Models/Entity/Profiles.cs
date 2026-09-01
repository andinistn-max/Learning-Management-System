using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("Profiles")]
    public class Profiles
    {
        [Key]
        [ForeignKey("User")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdUser { get; set; }

        [StringLength(30)]
        public string NoHp { get; set; }

        [StringLength(500)]
        public string Bio { get; set; }

        [StringLength(500)]
        public string Alamat { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TglLahir { get; set; }

        [StringLength(20)]
        public string JenisKelamin { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual Users User { get; set; }
    }
}
