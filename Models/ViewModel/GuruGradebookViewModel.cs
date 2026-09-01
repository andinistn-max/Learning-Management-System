using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class GuruGradebookViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public string NamaGuru { get; set; }

        // Weight Composition
        public decimal BobotAbsen { get; set; } = 15m;
        public decimal BobotMateri { get; set; } = 15m;
        public decimal BobotQuiz { get; set; } = 40m;
        public decimal BobotTugas { get; set; } = 30m;

        public List<HeaderKolomItemDto> DaftarTugasHeader { get; set; } = new List<HeaderKolomItemDto>();
        public List<HeaderKolomItemDto> DaftarQuizHeader { get; set; } = new List<HeaderKolomItemDto>();
        public List<GradebookRowSiswaDto> RekapBarisSiswa { get; set; } = new List<GradebookRowSiswaDto>();
    }

    public class HeaderKolomItemDto
    {
        public int Id { get; set; }
        public string Judul { get; set; }
        public int PertemuanKe { get; set; }
        public string KodeShort { get; set; } // e.g. "T1", "Q1"
    }

    public class GradebookRowSiswaDto
    {
        public int StudentId { get; set; }
        public string NamaSiswa { get; set; }
        public string Email { get; set; }
        public string FotoSiswa { get; set; }

        public Dictionary<int, decimal?> NilaiTugasMap { get; set; } = new Dictionary<int, decimal?>();
        public Dictionary<int, decimal?> NilaiQuizMap { get; set; } = new Dictionary<int, decimal?>();

        public decimal PersentaseAbsensi { get; set; }
        public decimal PersentaseMateri { get; set; }
        public decimal RataRataTugas { get; set; }
        public decimal RataRataQuiz { get; set; }

        public decimal NilaiAkhirAkumulasi { get; set; }
        public string PredikatHuruf { get; set; } // A, B, C, D, E
        public bool StatusLulus { get; set; }
    }
}
