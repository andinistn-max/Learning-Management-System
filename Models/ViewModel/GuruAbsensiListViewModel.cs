using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Learning_Management_System.Models.ViewModel
{
    public class GuruAbsensiListViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public List<SesiAbsensiItemDto> DaftarSesi { get; set; } = new List<SesiAbsensiItemDto>();
    }

    public class SesiAbsensiItemDto
    {
        public int SesiAbsensiId { get; set; }
        public int PertemuanKe { get; set; }
        public DateTime Tanggal { get; set; }
        public TimeSpan JamMulai { get; set; }
        public TimeSpan JamSelesai { get; set; }
        public string TokenPresensi { get; set; }
        public bool IsOpen { get; set; }
        public string StatusSesi { get; set; } // "Open", "Closed"
        public int TotalHadir { get; set; }
        public int TotalSiswa { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSesiAbsensiViewModel
    {
        [Required(ErrorMessage = "ID Kelas wajib diisi.")]
        public int KelasId { get; set; }

        [Required(ErrorMessage = "Pertemuan ke berapa wajib diisi.")]
        [Range(1, 100, ErrorMessage = "Pertemuan ke- harus antara 1 dan 100.")]
        [Display(Name = "Pertemuan Ke-")]
        public int PertemuanKe { get; set; } = 1;

        [Required(ErrorMessage = "Tanggal pelaksanaan sesi presensi wajib diisi.")]
        [Display(Name = "Tanggal Pertemuan")]
        [DataType(DataType.Date)]
        public DateTime Tanggal { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Jam mulai sesi presensi wajib diisi.")]
        [Display(Name = "Jam Mulai Sesi")]
        public string JamMulaiStr { get; set; } = "08:00";

        [Required(ErrorMessage = "Jam selesai sesi presensi wajib diisi.")]
        [Display(Name = "Jam Selesai Sesi")]
        public string JamSelesaiStr { get; set; } = "10:00";

        [Display(Name = "Generate Token Presensi 6 Karakter Otomatis")]
        public bool GenerateTokenOtomatis { get; set; } = true;

        [Display(Name = "Token Custom (Opsional - 6 Karakter Alfanumerik)")]
        [StringLength(20)]
        public string TokenCustom { get; set; }
    }

    public class RekapPresensiKelasViewModel
    {
        public int SesiAbsensiId { get; set; }
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public int PertemuanKe { get; set; }
        public DateTime Tanggal { get; set; }
        public TimeSpan JamMulai { get; set; }
        public TimeSpan JamSelesai { get; set; }
        public string TokenPresensi { get; set; }
        public bool IsOpen { get; set; }
        public int TotalSiswa { get; set; }
        public int TotalHadir { get; set; }
        public int TotalIzin { get; set; }
        public int TotalSakit { get; set; }
        public int TotalAlpa { get; set; }
        public List<RekapPresensiSiswaItemDto> ListRekapSiswa { get; set; } = new List<RekapPresensiSiswaItemDto>();
    }

    public class RekapPresensiSiswaItemDto
    {
        public int PresensiId { get; set; }
        public int StudentId { get; set; }
        public string NamaSiswa { get; set; }
        public string Email { get; set; }
        public string FotoSiswa { get; set; }
        public string StatusKehadiran { get; set; } // "Hadir", "Izin", "Sakit", "Alpa", "Belum Presensi"
        public DateTime? WaktuPresensi { get; set; }
        public string Keterangan { get; set; }
    }

    public class UpdateStatusPresensiViewModel
    {
        public int PresensiId { get; set; }

        [Required]
        public int SesiAbsensiId { get; set; }

        [Required]
        public int KelasId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Status kehadiran wajib dipilih.")]
        public string StatusKehadiran { get; set; } // "Hadir", "Izin", "Sakit", "Alpa"

        public string Keterangan { get; set; }
    }
}
