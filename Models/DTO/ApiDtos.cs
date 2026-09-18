using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.DTO
{
    public class RegisterRequestDto
    {
        [Required]
        public string NamaLengkap { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, MinLength(6)]
        public string Password { get; set; }
        public int IdRole { get; set; } = 3; // 3 = Siswa, 2 = Guru
    }

    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class GoogleLoginRequestDto
    {
        [Required]
        public string GoogleIdToken { get; set; }
    }

    public class ForgotPasswordRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        public string Token { get; set; }
        [Required, MinLength(6)]
        public string NewPassword { get; set; }
    }

    public class BuatKelasRequestDto
    {
        [Required]
        public string NamaKelas { get; set; }
        public int IdKategori { get; set; } = 1;
        public string Deskripsi { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class UpdateKelasRequestDto
    {
        public string NamaKelas { get; set; }
        public int? IdKategori { get; set; }
        public string Deskripsi { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class BuatTugasRequestDto
    {
        [Required]
        public int IdKelas { get; set; }
        [Required]
        public string JudulTugas { get; set; }
        public string Deskripsi { get; set; }
        public DateTime? Deadline { get; set; }
        public string FileLampiranUrl { get; set; }
    }

    public class SubmitTugasRequestDto
    {
        [Required]
        public int IdTugas { get; set; }
        [Required]
        public string FileUrl { get; set; }
        public string Catatan { get; set; }
    }

    public class NilaiTugasRequestDto
    {
        [Required]
        public int IdPengumpulan { get; set; }
        [Required, Range(0, 100)]
        public decimal Nilai { get; set; }
        public string Feedback { get; set; }
    }

    public class BuatQuizRequestDto
    {
        [Required]
        public int IdKelas { get; set; }
        [Required]
        public string JudulQuiz { get; set; }
        public string Deskripsi { get; set; }
        public int DurasiMenit { get; set; } = 30;
        public DateTime? BatasWaktu { get; set; }
    }

    public class TambahSoalQuizRequestDto
    {
        [Required]
        public string Pertanyaan { get; set; }
        public int BobotNilai { get; set; } = 10;
        public List<OpsiJawabanDto> Opsi { get; set; } = new List<OpsiJawabanDto>();
    }

    public class OpsiJawabanDto
    {
        public string TeksOpsi { get; set; }
        public bool IsBenar { get; set; }
    }

    public class SubmitQuizRequestDto
    {
        public int IdQuiz { get; set; }
        public List<JawabanItemDto> Jawaban { get; set; } = new List<JawabanItemDto>();
    }

    public class JawabanItemDto
    {
        public int IdSoal { get; set; }
        public int IdOpsiDipilih { get; set; }
    }

    public class BukaJadwalAbsenRequestDto
    {
        [Required]
        public int IdKelas { get; set; }
        [Required]
        public string JudulPertemuan { get; set; }
        public DateTime WaktuMulai { get; set; } = DateTime.Now;
        public DateTime WaktuSelesai { get; set; } = DateTime.Now.AddHours(2);
    }

    public class PresensiRequestDto
    {
        [Required]
        public int IdJadwal { get; set; }
        public string Keterangan { get; set; } = "Hadir";
    }
}
