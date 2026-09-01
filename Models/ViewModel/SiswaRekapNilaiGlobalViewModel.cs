using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class SiswaRekapNilaiGlobalViewModel
    {
        public decimal IPKKumulatif { get; set; }
        public int TotalKelasDiikuti { get; set; }
        public int TotalTugasDinilai { get; set; }
        public int TotalQuizDiselesaikan { get; set; }

        public List<SiswaNilaiKelasCardDto> DaftarNilaiKelas { get; set; } = new List<SiswaNilaiKelasCardDto>();
    }

    public class SiswaNilaiKelasCardDto
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string NamaGuru { get; set; }
        public string FotoGuru { get; set; }
        public string Kategori { get; set; }
        public string ThumbnailUrl { get; set; }

        // 1. Absensi (15%)
        public int TotalKehadiran { get; set; }
        public int TotalSesiAbsen { get; set; }
        public decimal SkorKehadiran { get; set; }
        public decimal NilaiBobotAbsen { get; set; }

        // 2. Progres Materi (15%)
        public int TotalMateriDibaca { get; set; }
        public int TotalMateri { get; set; }
        public decimal SkorMateri { get; set; }
        public decimal NilaiBobotMateri { get; set; }

        // 3. Quiz (40%)
        public decimal RataRataQuiz { get; set; }
        public decimal NilaiBobotQuiz { get; set; }
        public int QuizDiselesaikan { get; set; }

        // 4. Tugas (30%)
        public decimal RataRataTugas { get; set; }
        public decimal NilaiBobotTugas { get; set; }
        public int TugasDinilai { get; set; }

        // Hasil Akhir
        public decimal NilaiAkhirKelas { get; set; }
        public string PredikatHuruf { get; set; }
        public string StatusKelulusan { get; set; }
    }
}
