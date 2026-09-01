using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Learning_Management_System.Models.ViewModel
{
    public class GuruQuizListViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public List<GuruQuizItemDto> DaftarQuiz { get; set; } = new List<GuruQuizItemDto>();
    }

    public class GuruQuizItemDto
    {
        public int IdQuiz { get; set; }
        public int PertemuanKe { get; set; }
        public string JudulQuiz { get; set; }
        public string Deskripsi { get; set; }
        public int? DurasiMenit { get; set; }
        public decimal PassingScore { get; set; } = 75m;
        public int TotalSoal { get; set; }
        public int TotalPesertaMengerjakan { get; set; }
        public bool StatusAktif { get; set; }
        public DateTime? WaktuMulai { get; set; }
        public DateTime? WaktuSelesai { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateQuizViewModel
    {
        [Required(ErrorMessage = "ID Kelas wajib diisi.")]
        public int KelasId { get; set; }

        [Required(ErrorMessage = "Pertemuan ke berapa wajib diisi.")]
        [Range(1, 100, ErrorMessage = "Pertemuan ke- harus antara 1 dan 100.")]
        [Display(Name = "Pertemuan Ke-")]
        public int PertemuanKe { get; set; } = 1;

        [Required(ErrorMessage = "Judul kuis wajib diisi.")]
        [StringLength(200, ErrorMessage = "Judul kuis maksimal 200 karakter.")]
        [Display(Name = "Judul Kuis / Ujian Online")]
        public string JudulKuis { get; set; }

        [Display(Name = "Deskripsi / Petunjuk Ujian")]
        [DataType(DataType.MultilineText)]
        public string Deskripsi { get; set; }

        [Required(ErrorMessage = "Durasi pengerjaan wajib diisi.")]
        [Range(1, 300, ErrorMessage = "Durasi harus antara 1 hingga 300 menit.")]
        [Display(Name = "Durasi Pengerjaan (Menit)")]
        public int DurasiMenit { get; set; } = 30;

        [Display(Name = "Nilai Kelulusan Minimum (Passing Grade 0-100)")]
        [Range(0, 100)]
        public decimal PassingScore { get; set; } = 75m;

        [Display(Name = "Batas Waktu Mulai Kuis (Opsional)")]
        [DataType(DataType.DateTime)]
        public DateTime? BatasWaktuMulai { get; set; }

        [Display(Name = "Batas Waktu Selesai Kuis (Opsional)")]
        [DataType(DataType.DateTime)]
        public DateTime? BatasWaktuSelesai { get; set; }

        [Display(Name = "Acak Urutan Soal untuk Setiap Siswa")]
        public bool AcakSoal { get; set; } = false;
    }

    public class KelolaSoalViewModel
    {
        public int QuizId { get; set; }
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string JudulKuis { get; set; }
        public int? DurasiMenit { get; set; }
        public int TotalSoal { get; set; }
        public List<SoalQuizItemDto> ListSoal { get; set; } = new List<SoalQuizItemDto>();
    }

    public class SoalQuizItemDto
    {
        public int IdSoal { get; set; }
        public string Pertanyaan { get; set; }
        public string PilihanA { get; set; }
        public string PilihanB { get; set; }
        public string PilihanC { get; set; }
        public string PilihanD { get; set; }
        public string KunciJawaban { get; set; } // "A", "B", "C", "D"
        public decimal BobotNilai { get; set; } = 10m;
        public string GambarUrl { get; set; }
    }

    public class CreateSoalViewModel
    {
        [Required]
        public int QuizId { get; set; }

        [Required]
        public int KelasId { get; set; }

        [Required(ErrorMessage = "Teks pertanyaan soal wajib diisi.")]
        [Display(Name = "Pertanyaan Soal")]
        [DataType(DataType.MultilineText)]
        public string Pertanyaan { get; set; }

        [Required(ErrorMessage = "Pilihan A wajib diisi.")]
        [Display(Name = "Pilihan A")]
        public string PilihanA { get; set; }

        [Required(ErrorMessage = "Pilihan B wajib diisi.")]
        [Display(Name = "Pilihan B")]
        public string PilihanB { get; set; }

        [Required(ErrorMessage = "Pilihan C wajib diisi.")]
        [Display(Name = "Pilihan C")]
        public string PilihanC { get; set; }

        [Required(ErrorMessage = "Pilihan D wajib diisi.")]
        [Display(Name = "Pilihan D")]
        public string PilihanD { get; set; }

        [Required(ErrorMessage = "Kunci jawaban wajib dipilih.")]
        [Display(Name = "Kunci Jawaban Benar")]
        public string KunciJawaban { get; set; } // "A", "B", "C", "D"

        [Display(Name = "Bobot Nilai Soal")]
        public decimal BobotNilai { get; set; } = 10m;

        [Display(Name = "Unggah Gambar Lampiran Soal (Opsional)")]
        public HttpPostedFileBase GambarSoalOpsional { get; set; }
    }

    public class RekapHasilQuizViewModel
    {
        public int QuizId { get; set; }
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string JudulKuis { get; set; }
        public decimal PassingScore { get; set; }
        public int TotalSiswaKelas { get; set; }
        public int TotalSudahSubmit { get; set; }
        public decimal RataRataNilai { get; set; }
        public List<HasilQuizItemDto> ListHasil { get; set; } = new List<HasilQuizItemDto>();
    }

    public class HasilQuizItemDto
    {
        public int AttemptId { get; set; }
        public int StudentId { get; set; }
        public string NamaSiswa { get; set; }
        public string Email { get; set; }
        public string FotoSiswa { get; set; }
        public int JumlahBenar { get; set; }
        public int JumlahSalah { get; set; }
        public decimal? TotalNilai { get; set; }
        public bool StatusLulus { get; set; }
        public DateTime? WaktuMulai { get; set; }
        public DateTime? WaktuSelesai { get; set; }
        public string DurasiPengerjaanFormatted { get; set; }
        public string Status { get; set; } // "Completed", "InProgress", "Belum"
    }
}
