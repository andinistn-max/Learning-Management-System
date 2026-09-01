using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Learning_Management_System.Models.ViewModel
{
    public class GuruMateriListViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public List<GuruMateriItemDto> DaftarMateri { get; set; } = new List<GuruMateriItemDto>();
    }

    public class GuruMateriItemDto
    {
        public int IdMateri { get; set; }
        public int PertemuanKe { get; set; }
        public string JudulMateri { get; set; }
        public string Deskripsi { get; set; }
        public string NamaFile { get; set; }
        public string FilePath { get; set; }
        public string TipeMateri { get; set; }
        public long? FileSize { get; set; }
        public string FileSizeFormatted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMateriViewModel
    {
        [Required(ErrorMessage = "ID Kelas wajib diisi.")]
        public int KelasId { get; set; }

        [Required(ErrorMessage = "Pertemuan ke berapa wajib diisi.")]
        [Range(1, 100, ErrorMessage = "Pertemuan ke- harus antara 1 dan 100.")]
        [Display(Name = "Pertemuan Ke-")]
        public int PertemuanKe { get; set; } = 1;

        [Required(ErrorMessage = "Judul materi wajib diisi.")]
        [StringLength(200, ErrorMessage = "Judul materi maksimal 200 karakter.")]
        [Display(Name = "Judul Modul Materi")]
        public string JudulMateri { get; set; }

        [Display(Name = "Deskripsi / Catatan Materi")]
        [DataType(DataType.MultilineText)]
        public string Deskripsi { get; set; }

        [Display(Name = "Unggah Berkas File (PDF, DOCX, PPTX, MP4, ZIP)")]
        public HttpPostedFileBase FileUpload { get; set; }

        [Display(Name = "Tautan Video Pembelajaran (Opsional YouTube/Drive)")]
        public string VideoUrlOpsional { get; set; }
    }
}
