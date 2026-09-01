using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter.")]
        [Display(Name = "Nama Lengkap")]
        public string NamaLengkap { get; set; }

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [StringLength(100, ErrorMessage = "Email maksimal 100 karakter.")]
        [Display(Name = "Alamat Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password wajib diisi.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password minimal 6 karakter.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Konfirmasi password wajib diisi.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Konfirmasi password tidak cocok dengan password.")]
        [Display(Name = "Konfirmasi Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Pilih peran pengguna (Guru atau Siswa).")]
        [Display(Name = "Peran Pengguna")]
        public string Role { get; set; }
    }
}
