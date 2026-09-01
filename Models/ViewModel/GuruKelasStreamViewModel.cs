using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Learning_Management_System.Models.ViewModel
{
    public class GuruKelasStreamViewModel
    {
        public int IdKelas { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public string Deskripsi { get; set; }
        public string Level { get; set; }
        public int? Durasi { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ActiveTab { get; set; } = "stream";

        public int JumlahSiswa { get; set; }
        public int JumlahMateri { get; set; }
        public int JumlahTugas { get; set; }
        public int JumlahQuiz { get; set; }

        public List<GuruStreamPostDto> DaftarPostingan { get; set; } = new List<GuruStreamPostDto>();
    }

    public class GuruStreamPostDto
    {
        public int IdPost { get; set; }
        public int IdUser { get; set; }
        public string NamaPenulis { get; set; }
        public string FotoPenulis { get; set; }
        public string RolePenulis { get; set; }
        public string Konten { get; set; }
        public string FileAttachment { get; set; }
        public string NamaFile { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAuthor { get; set; }

        public List<GuruStreamCommentDto> ListKomentar { get; set; } = new List<GuruStreamCommentDto>();
    }

    public class GuruStreamCommentDto
    {
        public int IdComment { get; set; }
        public int IdPost { get; set; }
        public int IdUser { get; set; }
        public string NamaPenulis { get; set; }
        public string FotoPenulis { get; set; }
        public string RolePenulis { get; set; }
        public string Konten { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAuthor { get; set; }
    }

    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "ID Kelas wajib diisi.")]
        public int KelasId { get; set; }

        [Required(ErrorMessage = "Pesan pengumuman atau postingan wajib diisi.")]
        public string KontenTeks { get; set; }

        public HttpPostedFileBase LampiranFile { get; set; }
    }

    public class CreateCommentViewModel
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        public int KelasId { get; set; }

        [Required(ErrorMessage = "Komentar tidak boleh kosong.")]
        public string KontenKomentar { get; set; }
    }
}
