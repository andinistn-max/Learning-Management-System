using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learning_Management_System.Models.ViewModel
{
    public class SiswaQuizListViewModel
    {
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string KodeKelas { get; set; }
        public string Kategori { get; set; }
        public string NamaGuru { get; set; }
        public string FotoGuru { get; set; }

        public int TotalQuiz { get; set; }
        public int TotalSudahDikerjakan { get; set; }
        public int TotalBelumDikerjakan { get; set; }

        public List<SiswaQuizItemDto> DaftarQuiz { get; set; } = new List<SiswaQuizItemDto>();
    }

    public class SiswaQuizItemDto
    {
        public int QuizId { get; set; }
        public int PertemuanKe { get; set; }
        public string JudulQuiz { get; set; }
        public string Deskripsi { get; set; }
        public int DurasiMenit { get; set; }
        public decimal PassingScore { get; set; }
        public int TotalSoal { get; set; }
        public string StatusPengerjaan { get; set; } // "Belum Dikerjakan", "Sudah Dikerjakan", "Waktu Habis"
        public decimal? Nilai { get; set; }
        public DateTime? WaktuSubmit { get; set; }
        public int? AttemptId { get; set; }
        public bool CanAttempt { get; set; }
    }

    public class KerjakanQuizViewModel
    {
        public int QuizId { get; set; }
        public int KelasId { get; set; }
        public string JudulQuiz { get; set; }
        public string Deskripsi { get; set; }
        public int DurasiMenit { get; set; }
        public int SisaDetik { get; set; }
        public int AttemptId { get; set; }

        public List<SiswaSoalQuizItemDto> DaftarSoal { get; set; } = new List<SiswaSoalQuizItemDto>();
    }

    public class SiswaSoalQuizItemDto
    {
        public int SoalId { get; set; }
        public int NomorUrut { get; set; }
        public string Pertanyaan { get; set; }
        public string GambarUrl { get; set; }

        public int OpsiA_Id { get; set; }
        public string PilihanA { get; set; }

        public int OpsiB_Id { get; set; }
        public string PilihanB { get; set; }

        public int OpsiC_Id { get; set; }
        public string PilihanC { get; set; }

        public int OpsiD_Id { get; set; }
        public string PilihanD { get; set; }
    }

    public class SubmitJawabanQuizViewModel
    {
        [Required]
        public int QuizId { get; set; }

        [Required]
        public int KelasId { get; set; }

        [Required]
        public int AttemptId { get; set; }

        public List<JawabanSiswaItemDto> DaftarJawaban { get; set; } = new List<JawabanSiswaItemDto>();
    }

    public class JawabanSiswaItemDto
    {
        public int SoalId { get; set; }
        public int OpsiDipilihId { get; set; }
    }

    public class HasilPengerjaanQuizViewModel
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int KelasId { get; set; }
        public string NamaKelas { get; set; }
        public string JudulQuiz { get; set; }
        public int TotalSoal { get; set; }
        public int JumlahBenar { get; set; }
        public int JumlahSalah { get; set; }
        public decimal TotalSkor { get; set; }
        public decimal PassingScore { get; set; }
        public string StatusLulus { get; set; }
        public DateTime? WaktuSubmit { get; set; }
    }
}
