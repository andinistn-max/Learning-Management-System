using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class AdminTambahGuruViewModel
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter.")]
        [Display(Name = "Nama Lengkap")]
        public string NamaLengkap { get; set; }

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [Display(Name = "Alamat Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password wajib diisi.")]
        [MinLength(6, ErrorMessage = "Password minimal 6 karakter.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Phone(ErrorMessage = "Format nomor telepon tidak valid.")]
        [StringLength(30, ErrorMessage = "Nomor telepon maksimal 30 karakter.")]
        [Display(Name = "Nomor Telepon / WA")]
        public string NoTelepon { get; set; }

        [StringLength(100, ErrorMessage = "Gelar / spesialisasi maksimal 100 karakter.")]
        [Display(Name = "Gelar / Spesialisasi")]
        public string Spesialisasi { get; set; }

        [StringLength(255, ErrorMessage = "Alamat maksimal 255 karakter.")]
        [Display(Name = "Alamat Lengkap")]
        public string Alamat { get; set; }
    }
}
