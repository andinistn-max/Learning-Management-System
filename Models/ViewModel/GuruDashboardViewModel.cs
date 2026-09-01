using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class GuruDashboardViewModel
    {
        public int TotalKelasDiampu { get; set; }
        public int TotalSiswaTerdaftar { get; set; }
        public int TotalTugasPerluDikoreksi { get; set; }
        public int TotalKuisAktif { get; set; }

        public List<GuruDashboardKelasDto> DaftarKelas { get; set; } = new List<GuruDashboardKelasDto>();
        public List<GuruDashboardAktivitasDto> AktivitasTerbaruSiswa { get; set; } = new List<GuruDashboardAktivitasDto>();
    }

    public class GuruDashboardKelasDto
    {
        public int IdKelas { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public string ThumbnailUrl { get; set; }
        public int JumlahSiswa { get; set; }
        public int JumlahMateri { get; set; }
        public int JumlahTugas { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GuruDashboardAktivitasDto
    {
        public int IdSubmisi { get; set; }
        public string NamaSiswa { get; set; }
        public string FotoSiswa { get; set; }
        public string NamaKelas { get; set; }
        public string JudulTugas { get; set; }
        public DateTime TanggalKumpul { get; set; }
        public string StatusNilai { get; set; }
        public decimal? Nilai { get; set; }
    }
}
