using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class SiswaGlobalTugasViewModel
    {
        public string FilterStatus { get; set; } = "Semua";
        public int TotalSemua { get; set; }
        public int TotalBelumSelesai { get; set; }
        public int TotalSudahDinilai { get; set; }
        public int TotalTerlewat { get; set; }

        public List<GlobalTugasItemDto> DaftarTugas { get; set; } = new List<GlobalTugasItemDto>();
    }

    public class GlobalTugasItemDto
    {
        public int TugasId { get; set; }
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string JudulTugas { get; set; }
        public DateTime Deadline { get; set; }
        public string StatusDeadline { get; set; } // "Aktif", "Mendekati", "Lewat"
        public string StatusPengumpulan { get; set; } // "Belum Dikumpulkan", "Diserahkan", "Terlambat", "Sudah Dinilai"
        public decimal? Nilai { get; set; }
        public DateTime? WaktuKumpul { get; set; }
        public bool IsOverdue { get; set; }
        public string TimeRemainingFormatted { get; set; }
    }

    public class SiswaGlobalMateriViewModel
    {
        public string FilterKategori { get; set; } = "";
        public string SearchKeyword { get; set; } = "";
        public List<string> DaftarKategoriOptions { get; set; } = new List<string>();

        public int TotalMateri { get; set; }
        public int TotalSudahDipelajari { get; set; }

        public List<GlobalMateriItemDto> DaftarMateri { get; set; } = new List<GlobalMateriItemDto>();
    }

    public class GlobalMateriItemDto
    {
        public int MateriId { get; set; }
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string JudulMateri { get; set; }
        public string TipeFile { get; set; }
        public string FileUrl { get; set; }
        public string NamaFile { get; set; }
        public DateTime TanggalUpload { get; set; }
        public bool IsSudahDipelajari { get; set; }
        public DateTime? TanggalDipelajari { get; set; }
    }

    public class SiswaGlobalQuizViewModel
    {
        public int TotalKuis { get; set; }
        public int TotalSudahDikerjakan { get; set; }
        public int TotalBelumDikerjakan { get; set; }

        public List<GlobalQuizItemDto> DaftarQuiz { get; set; } = new List<GlobalQuizItemDto>();
    }

    public class GlobalQuizItemDto
    {
        public int QuizId { get; set; }
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string JudulQuiz { get; set; }
        public int DurasiMenit { get; set; }
        public int TotalSoal { get; set; }
        public DateTime? BatasWaktuMulai { get; set; }
        public DateTime? BatasWaktuSelesai { get; set; }
        public string StatusPengerjaan { get; set; } // "Belum Dikerjakan", "Sudah Dikerjakan", "Waktu Habis"
        public decimal? Skor { get; set; }
        public string WaktuSelesaiFormat { get; set; }
    }

    public class SiswaKatalogKelasViewModel
    {
        public string SearchKeyword { get; set; } = "";
        public string FilterKategori { get; set; } = "";
        public List<string> DaftarKategoriOptions { get; set; } = new List<string>();
        public int TotalKelasTersedia { get; set; }
        public int TotalKelasDiikuti { get; set; }
        public List<KatalogKelasItemDto> DaftarKelas { get; set; } = new List<KatalogKelasItemDto>();
    }

    public class KatalogKelasItemDto
    {
        public int IdKelas { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Deskripsi { get; set; }
        public string BannerImage { get; set; }
        public string NamaKategori { get; set; }
        public string NamaGuru { get; set; }
        public string FotoGuru { get; set; }
        public int TotalSiswa { get; set; }
        public int TotalMateri { get; set; }
        public int TotalTugas { get; set; }
        public int TotalQuiz { get; set; }
        public bool IsEnrolled { get; set; }
    }
}
