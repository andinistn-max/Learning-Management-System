using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class SiswaRincianNilaiKelasViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string NamaGuru { get; set; }
        public string FotoGuru { get; set; }
        public string Kategori { get; set; }
        public string ThumbnailUrl { get; set; }

        // 1. Absensi (15%)
        public int TotalSesi { get; set; }
        public int TotalHadir { get; set; }
        public decimal SkorAbsensi { get; set; }
        public decimal PoinBobotAbsensi { get; set; }

        // 2. Materi (15%)
        public int TotalMateri { get; set; }
        public int TotalMateriSelesai { get; set; }
        public decimal SkorMateri { get; set; }
        public decimal PoinBobotMateri { get; set; }

        // 3. Quiz (40%)
        public List<ItemNilaiQuizDto> DaftarNilaiQuiz { get; set; } = new List<ItemNilaiQuizDto>();
        public decimal RataRataQuiz { get; set; }
        public decimal PoinBobotQuiz { get; set; }

        // 4. Tugas (30%)
        public List<ItemNilaiTugasDto> DaftarNilaiTugas { get; set; } = new List<ItemNilaiTugasDto>();
        public decimal RataRataTugas { get; set; }
        public decimal PoinBobotTugas { get; set; }

        // Hasil Akhir
        public decimal NilaiAkhirKumulatif { get; set; }
        public string PredikatHuruf { get; set; }
        public string StatusKelulusan { get; set; }
    }

    public class ItemNilaiQuizDto
    {
        public string JudulQuiz { get; set; }
        public DateTime? Tanggal { get; set; }
        public decimal? SkorPerolehan { get; set; }
    }

    public class ItemNilaiTugasDto
    {
        public string JudulTugas { get; set; }
        public DateTime? TanggalSubmisi { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; }
        public decimal? NilaiTugas { get; set; }
        public string FeedbackGuru { get; set; }
    }
}
