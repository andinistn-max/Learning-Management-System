using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Learning_Management_System.Models.ViewModel
{
    public class SiswaTugasListViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public string NamaGuru { get; set; }
        public string FotoGuru { get; set; }

        public int TotalTugas { get; set; }
        public int TotalSudahDikumpulkan { get; set; }
        public int TotalBelumDikumpulkan { get; set; }

        public List<SiswaTugasItemDto> DaftarTugas { get; set; } = new List<SiswaTugasItemDto>();
    }

    public class SiswaTugasItemDto
    {
        public int IdTugas { get; set; }
        public string JudulTugas { get; set; }
        public string Instruksi { get; set; }
        public DateTime Deadline { get; set; }
        public string FileLampiranSoalUrl { get; set; }
        public string NamaFileLampiran { get; set; }

        // Status Pengumpulan: "Belum Dikumpulkan", "Diserahkan", "Terlambat", "Sudah Dinilai"
        public string StatusPengumpulan { get; set; }
        public bool IsLate { get; set; }
        public bool IsOverdue { get; set; }

        public int? SubmissionId { get; set; }
        public string FileSubmisiUrl { get; set; }
        public string NamaFileSubmisi { get; set; }
        public DateTime? WaktuKumpul { get; set; }
        public decimal? Nilai { get; set; }
        public string FeedbackGuru { get; set; }
        public bool IsSudahDinilai { get; set; }
        public bool CanCancelSubmission { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SubmitTugasViewModel
    {
        [Required(ErrorMessage = "ID Tugas wajib diisi.")]
        public int TugasId { get; set; }

        [Required(ErrorMessage = "ID Kelas wajib diisi.")]
        public int KelasId { get; set; }

        [Required(ErrorMessage = "Mohon pilih file berkas jawaban Anda.")]
        [Display(Name = "File Jawaban")]
        public HttpPostedFileBase FileJawaban { get; set; }

        [Display(Name = "Catatan untuk Guru (Opsional)")]
        public string CatatanSiswa { get; set; }
    }
}
