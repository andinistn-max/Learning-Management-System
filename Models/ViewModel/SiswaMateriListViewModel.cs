using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class SiswaMateriListViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public string NamaGuru { get; set; }
        public string FotoGuru { get; set; }

        public int TotalMateri { get; set; }
        public int TotalMateriSelesai { get; set; }
        public decimal PersentaseProgres { get; set; }

        public List<SiswaMateriItemDto> DaftarMateri { get; set; } = new List<SiswaMateriItemDto>();
    }

    public class SiswaMateriItemDto
    {
        public int IdMateri { get; set; }
        public int PertemuanKe { get; set; }
        public string Judul { get; set; }
        public string Deskripsi { get; set; }
        public string TipeFile { get; set; }
        public string FileUrl { get; set; }
        public string NamaFile { get; set; }
        public string UkuranFileFormatted { get; set; }
        public string VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSudahDibaca { get; set; }
        public DateTime? TanggalDibaca { get; set; }
    }

    public class MarkMateriReadViewModel
    {
        [Required]
        public int MateriId { get; set; }

        [Required]
        public int KelasId { get; set; }
    }
}
