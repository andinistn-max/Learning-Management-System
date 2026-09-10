using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Learning_Management_System.Models.ViewModel
{
    public class AdminKelasListViewModel
    {
        public List<AdminKelasItemDto> DaftarKelas { get; set; } = new List<AdminKelasItemDto>();

        // Filter Properties
        public string SearchQuery { get; set; }
        public string KategoriFilter { get; set; } = "Semua";
        public string StatusFilter { get; set; } = "Aktif";
        public string SortBy { get; set; } = "Tanggal";

        // Kategori Dropdown List
        public List<SelectListItem> KategoriOptions { get; set; } = new List<SelectListItem>();

        // Pagination Properties
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; } = 0;
    }

    public class AdminKelasItemDto
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string NamaKategori { get; set; }
        public string NamaGuruPengampu { get; set; }
        public string ThumbnailUrl { get; set; }
        public int JumlahSiswa { get; set; }
        public int JumlahMateri { get; set; }
        public int JumlahTugas { get; set; }
        public bool IsPublish { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime TanggalDibuat => CreatedAt;
    }
}
