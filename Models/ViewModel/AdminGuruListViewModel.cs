using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class AdminGuruListViewModel
    {
        public List<GuruItemDto> DaftarGuru { get; set; } = new List<GuruItemDto>();

        // Filter & Search Properties
        public string SearchQuery { get; set; }
        public string StatusFilter { get; set; } = "Semua";
        public string SortBy { get; set; } = "Nama";

        // Pagination Properties
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; } = 0;
    }

    public class GuruItemDto
    {
        public int UserId { get; set; }
        public string NamaLengkap { get; set; }
        public string Email { get; set; }
        public string NoTelepon { get; set; }
        public string FotoProfile { get; set; }
        public int JumlahKelasDiampu { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
