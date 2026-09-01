using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class GuruKalkulasiNilaiViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public int TotalPertemuan { get; set; }
        public int TotalMateri { get; set; }
        public int TotalQuiz { get; set; }
        public int TotalTugas { get; set; }

        public List<SiswaNilaiKomprehensifDto> RekapSiswa { get; set; } = new List<SiswaNilaiKomprehensifDto>();
    }

    public class SiswaNilaiKomprehensifDto
    {
        public int StudentId { get; set; }
        public string NamaSiswa { get; set; }
        public string Email { get; set; }
        public string FotoSiswa { get; set; }

        // Komponen Absensi (Bobot 15%)
        public int TotalKehadiran { get; set; }
        public int TotalSesiPresensi { get; set; }
        public decimal SkorAbsensi { get; set; } // 0 - 100
        public decimal NilaiBobotAbsensi { get; set; } // SkorAbsensi * 0.15

        // Komponen Progres Materi (Bobot 15%)
        public int TotalMateriDibaca { get; set; }
        public int TotalMateriKelas { get; set; }
        public decimal SkorMateri { get; set; } // 0 - 100
        public decimal NilaiBobotMateri { get; set; } // SkorMateri * 0.15

        // Komponen Rata-rata Kuis (Bobot 40%)
        public decimal RataRataSkorQuiz { get; set; } // 0 - 100
        public decimal NilaiBobotQuiz { get; set; } // RataRataSkorQuiz * 0.40

        // Komponen Rata-rata Tugas (Bobot 30%)
        public decimal RataRataSkorTugas { get; set; } // 0 - 100
        public decimal NilaiBobotTugas { get; set; } // RataRataSkorTugas * 0.30

        // Hasil Akhir Akumulasi
        public decimal NilaiAkhirKumulatif { get; set; } // Max 100
        public string PredikatHuruf { get; set; } // A, B, C, D, E
        public string StatusKelulusan { get; set; } // "Lulus" / "Tidak Lulus"
    }
}
