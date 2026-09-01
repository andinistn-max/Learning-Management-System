using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Learning_Management_System.Models.ViewModel
{
    public class AdminProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter.")]
        public string NamaLengkap { get; set; }

        [Required(ErrorMessage = "Alamat email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format alamat email tidak valid.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Format nomor telepon tidak valid.")]
        [StringLength(20, ErrorMessage = "Nomor telepon maksimal 20 karakter.")]
        public string NoTelepon { get; set; }

        [StringLength(255, ErrorMessage = "Bio maksimal 255 karakter.")]
        public string Bio { get; set; }

        [StringLength(255, ErrorMessage = "Alamat maksimal 255 karakter.")]
        public string Alamat { get; set; }

        public string FotoUrl { get; set; }

        public HttpPostedFileBase FotoUpload { get; set; }

        [DataType(DataType.Password)]
        public string KataSandiLama { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password baru minimal 6 karakter.")]
        public string KataSandiBaru { get; set; }

        [DataType(DataType.Password)]
        [Compare("KataSandiBaru", ErrorMessage = "Konfirmasi password baru tidak cocok.")]
        public string KonfirmasiKataSandi { get; set; }
    }
}
