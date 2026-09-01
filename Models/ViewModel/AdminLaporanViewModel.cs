using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class AdminLaporanViewModel
    {
        public int TotalGuru { get; set; }
        public int TotalSiswa { get; set; }
        public int TotalKelasAktif { get; set; }
        public int TotalModulPembelajaran { get; set; }
        public int TotalKuisTugas { get; set; }
        public double TingkatPartisipasiSiswa { get; set; }

        public List<LaporanKategoriDto> RingkasanKategori { get; set; } = new List<LaporanKategoriDto>();
    }

    public class LaporanKategoriDto
    {
        public string NamaKategori { get; set; }
        public int TotalKelas { get; set; }
        public int TotalSiswa { get; set; }
    }
}
