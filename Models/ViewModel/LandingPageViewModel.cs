using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class LandingPageViewModel
    {
        public List<KategoriDto> KategoriList { get; set; } = new List<KategoriDto>();
        public List<KelasPreviewDto> KelasPopuler { get; set; } = new List<KelasPreviewDto>();
        public int TotalKelas { get; set; }
        public int TotalGuru { get; set; }
        public int TotalSiswa { get; set; }
        public int TotalKategori { get; set; }
    }

    public class KategoriDto
    {
        public int IdKategori { get; set; }
        public string NamaKategori { get; set; }
        public string Deskripsi { get; set; }
        public int TotalKelas { get; set; }
    }

    public class KelasPreviewDto
    {
        public int IdKelas { get; set; }
        public string NamaKelas { get; set; }
        public string Deskripsi { get; set; }
        public string Thumbnail { get; set; }
        public string Level { get; set; }
        public int? Durasi { get; set; }
        public string NamaKategori { get; set; }
        public string NamaGuru { get; set; }
        public string FotoGuru { get; set; }
        public int TotalSiswa { get; set; }
    }
}
