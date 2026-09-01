using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Alamat email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [Display(Name = "Alamat Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password wajib diisi.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Ingat Saya")]
        public bool RememberMe { get; set; }
    }
}
