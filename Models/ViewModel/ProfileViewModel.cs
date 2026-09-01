using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Learning_Management_System.Models.ViewModel
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter.")]
        [Display(Name = "Nama Lengkap")]
        public string NamaLengkap { get; set; }

        [Display(Name = "Alamat Email")]
        public string Email { get; set; }

        [StringLength(20, ErrorMessage = "Nomor telepon maksimal 20 karakter.")]
        [Display(Name = "Nomor Telepon / WhatsApp")]
        public string NoTelepon { get; set; }

        [StringLength(500, ErrorMessage = "Bio maksimal 500 karakter.")]
        [Display(Name = "Biografi Singkat")]
        public string Bio { get; set; }

        [StringLength(255, ErrorMessage = "Alamat maksimal 255 karakter.")]
        [Display(Name = "Alamat Lengkap")]
        public string Alamat { get; set; }

        [StringLength(20)]
        [Display(Name = "Jenis Kelamin")]
        public string JenisKelamin { get; set; }

        public string FotoUrl { get; set; }

        [Display(Name = "Upload Foto Profil")]
        public HttpPostedFileBase FotoUpload { get; set; }
    }
}
