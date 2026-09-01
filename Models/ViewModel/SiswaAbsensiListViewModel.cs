using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class SiswaAbsensiListViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public string NamaGuru { get; set; }
        public string FotoGuru { get; set; }

        public int TotalHadir { get; set; }
        public int TotalSakit { get; set; }
        public int TotalIzin { get; set; }
        public int TotalAlpa { get; set; }
        public int TotalSesi { get; set; }
        public decimal PersentaseKehadiran { get; set; }

        public List<SiswaAbsensiItemDto> DaftarSesiAbsensi { get; set; } = new List<SiswaAbsensiItemDto>();
    }

    public class SiswaAbsensiItemDto
    {
        public int IdJadwal { get; set; }
        public int PertemuanKe { get; set; }
        public string JudulPertemuan { get; set; }
        public DateTime Tanggal { get; set; }
        public TimeSpan JamMulai { get; set; }
        public TimeSpan JamSelesai { get; set; }
        public string TokenPresensi { get; set; }
        public bool IsOpen { get; set; }
        public string StatusSesi { get; set; } // "Buka", "Tutup", "Mendatang"
        public string StatusKehadiranSiswa { get; set; } // "Hadir", "Sakit", "Izin", "Alpa", "Belum Presensi"
        public DateTime? WaktuPresensi { get; set; }
        public bool CanSubmitPresensi { get; set; }
        public bool IsRequiresToken { get; set; }
    }

    public class SubmitPresensiViewModel
    {
        [Required(ErrorMessage = "ID Sesi Absensi wajib diisi.")]
        public int SesiAbsensiId { get; set; }

        [Required(ErrorMessage = "ID Kelas wajib diisi.")]
        public int KelasId { get; set; }

        [Display(Name = "Kode Token Presensi")]
        public string TokenDiinput { get; set; }
    }
}
