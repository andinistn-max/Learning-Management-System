using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Alamat email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [Display(Name = "Alamat Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password baru wajib diisi.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password baru minimal 6 karakter.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password Baru")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Konfirmasi password baru wajib diisi.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Konfirmasi password tidak cocok dengan password baru.")]
        [Display(Name = "Konfirmasi Password Baru")]
        public string ConfirmPassword { get; set; }
    }
}
