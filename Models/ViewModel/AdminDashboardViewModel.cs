using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class AdminDashboardViewModel
    {
        public int TotalGuru { get; set; }
        public int TotalSiswa { get; set; }
        public int TotalKelas { get; set; }
        public int TotalMateri { get; set; }
        public int TotalKuisTugas { get; set; }

        public List<KelasPopulerItemDto> KelasTerpopuler { get; set; } = new List<KelasPopulerItemDto>();
        public List<LogAktivitasItemDto> LogAktivitasTerbaru { get; set; } = new List<LogAktivitasItemDto>();
    }

    public class KelasPopulerItemDto
    {
        public int IdKelas { get; set; }
        public string NamaKelas { get; set; }
        public string NamaGuru { get; set; }
        public string NamaKategori { get; set; }
        public int JumlahSiswa { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class LogAktivitasItemDto
    {
        public int Id { get; set; }
        public string JudulAktivitas { get; set; }
        public string Deskripsi { get; set; }
        public DateTime Tanggal { get; set; }
        public string TipeAktivitas { get; set; }
        public string NamaUser { get; set; }
    }
}
