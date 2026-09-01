using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class SiswaDashboardViewModel
    {
        public int TotalKelasDiikuti { get; set; }
        public int TotalTugasPending { get; set; }
        public int TotalKuisTersedia { get; set; }
        public decimal RataRataNilaiKeseluruhan { get; set; }

        public List<SiswaKelasItemDto> DaftarKelasSiswa { get; set; } = new List<SiswaKelasItemDto>();
        public List<DeadlineItemDto> DaftarDeadlineMendekat { get; set; } = new List<DeadlineItemDto>();
    }

    public class SiswaKelasItemDto
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public string NamaGuru { get; set; }
        public string FotoGuru { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal PersentaseProgresBelajar { get; set; }
        public int TotalMateri { get; set; }
        public int TotalTugas { get; set; }
        public int TotalQuiz { get; set; }
        public DateTime TglJoin { get; set; }
    }

    public class DeadlineItemDto
    {
        public string ItemType { get; set; } // "Tugas", "Quiz"
        public int ItemId { get; set; }
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string Judul { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsOverdue { get; set; }
        public string TimeRemainingFormatted { get; set; }
    }

    public class GabungKelasViewModel
    {
        [Required(ErrorMessage = "Kode Kelas wajib diisi.")]
        [StringLength(20, ErrorMessage = "Kode Kelas maksimal 20 karakter.")]
        [Display(Name = "Kode Akses Kelas")]
        public string KodeKelas { get; set; }
    }
}
