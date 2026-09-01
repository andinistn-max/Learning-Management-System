using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Learning_Management_System.Models.ViewModel
{
    public class GuruTugasListViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public List<GuruTugasItemDto> DaftarTugas { get; set; } = new List<GuruTugasItemDto>();
    }

    public class GuruTugasItemDto
    {
        public int IdTugas { get; set; }
        public int PertemuanKe { get; set; }
        public string JudulTugas { get; set; }
        public string Instruksi { get; set; }
        public DateTime Deadline { get; set; }
        public string FileLampiranUrl { get; set; }
        public string NamaFileLampiran { get; set; }
        public int TotalSiswa { get; set; }
        public int JumlahMengumpulkan { get; set; }
        public int JumlahDinilai { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTugasViewModel
    {
        [Required(ErrorMessage = "ID Kelas wajib diisi.")]
        public int KelasId { get; set; }

        [Required(ErrorMessage = "Pertemuan ke berapa wajib diisi.")]
        [Range(1, 100, ErrorMessage = "Pertemuan ke- harus antara 1 dan 100.")]
        [Display(Name = "Pertemuan Ke-")]
        public int PertemuanKe { get; set; } = 1;

        [Required(ErrorMessage = "Judul tugas wajib diisi.")]
        [StringLength(200, ErrorMessage = "Judul tugas maksimal 200 karakter.")]
        [Display(Name = "Judul Penugasan")]
        public string Judul { get; set; }

        [Display(Name = "Instruksi & Petunjuk Pengerjaan")]
        [DataType(DataType.MultilineText)]
        public string Instruksi { get; set; }

        [Required(ErrorMessage = "Batas waktu pengumpulan (deadline) wajib diisi.")]
        [Display(Name = "Batas Waktu Pengumpulan (Deadline)")]
        [DataType(DataType.DateTime)]
        public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "File Lampiran Soal / Template (Opsional)")]
        public HttpPostedFileBase FileLampiran { get; set; }
    }

    public class DetailSubmisiTugasViewModel
    {
        public int TugasId { get; set; }
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string JudulTugas { get; set; }
        public int PertemuanKe { get; set; }
        public DateTime Deadline { get; set; }
        public int TotalSiswa { get; set; }
        public int JumlahMengumpulkan { get; set; }
        public int JumlahDinilai { get; set; }
        public List<SubmisiTugasItemDto> ListSubmisi { get; set; } = new List<SubmisiTugasItemDto>();
    }

    public class SubmisiTugasItemDto
    {
        public int SubmissionId { get; set; }
        public int StudentId { get; set; }
        public string NamaSiswa { get; set; }
        public string Email { get; set; }
        public string FotoSiswa { get; set; }
        public string FileSubmisiUrl { get; set; }
        public string NamaFileSubmisi { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsLate { get; set; }
        public decimal? Nilai { get; set; }
        public string Feedback { get; set; }
        public string Status { get; set; } // "Submitted", "Graded", "Pending"
    }

    public class BeriNilaiViewModel
    {
        [Required]
        public int SubmissionId { get; set; }

        [Required]
        public int TugasId { get; set; }

        [Required]
        public int KelasId { get; set; }

        [Required(ErrorMessage = "Nilai wajib diisi (0 - 100).")]
        [Range(0, 100, ErrorMessage = "Nilai harus berada pada rentang 0 hingga 100.")]
        public decimal Nilai { get; set; }

        [Display(Name = "Catatan / Evaluasi Feedback Guru")]
        public string Feedback { get; set; }
    }
}
