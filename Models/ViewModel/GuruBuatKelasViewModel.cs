using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Learning_Management_System.Models.ViewModel
{
    public class GuruBuatKelasViewModel
    {
        [Required(ErrorMessage = "Nama kelas wajib diisi.")]
        [StringLength(150, ErrorMessage = "Nama kelas maksimal 150 karakter.")]
        [Display(Name = "Nama Kelas Pembelajaran")]
        public string NamaKelas { get; set; }

        [Required(ErrorMessage = "Kategori kelas wajib dipilih.")]
        [Display(Name = "Kategori Kelas")]
        public int IdKategori { get; set; }

        [Display(Name = "Deskripsi Kelas")]
        [DataType(DataType.MultilineText)]
        public string Deskripsi { get; set; }

        [Display(Name = "Tingkat Kesulitan / Level")]
        public string Level { get; set; } = "Pemula";

        [Display(Name = "Perkiraan Durasi (Jam)")]
        [Range(1, 500, ErrorMessage = "Durasi harus antara 1 sampai 500 jam.")]
        public int? Durasi { get; set; }

        [Display(Name = "Thumbnail / Cover Gambar Kelas")]
        public HttpPostedFileBase ThumbnailUpload { get; set; }

        public IEnumerable<SelectListItem> KategoriOptions { get; set; }
        public IEnumerable<SelectListItem> LevelOptions { get; set; }
    }
}
