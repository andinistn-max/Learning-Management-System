using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class AdminNotifikasiViewModel
    {
        public List<AdminNotifikasiItemDto> DaftarNotifikasi { get; set; } = new List<AdminNotifikasiItemDto>();
        public int UnreadCount { get; set; }
    }

    public class AdminNotifikasiItemDto
    {
        public int IdNotifikasi { get; set; }
        public string Judul { get; set; }
        public string Pesan { get; set; }
        public string TipeNotif { get; set; }
        public string UrlTujuan { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
