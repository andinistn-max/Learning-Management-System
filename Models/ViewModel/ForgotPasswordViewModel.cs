using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Alamat email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [Display(Name = "Alamat Email")]
        public string Email { get; set; }
    }
}
