using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Cors;
using Learning_Management_System.Helpers;
using Learning_Management_System.Models.DTO;
using Learning_Management_System.Models.Entity;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Controllers.API
{
    /// <summary>
    /// Layanan Ujian Online &amp; Evaluasi Kuis REST API
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/quiz")]
    public class QuizController : ApiController
    {
        private readonly LmsDbContext db = new LmsDbContext();

        /// <summary>
        /// Membuat paket kuis baru di dalam kelas (Role: Guru / Admin)
        /// </summary>
        [HttpPost]
        [Route("")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult BuatQuiz([FromBody] BuatQuizRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            int lastPertemuan = db.Quiz.Where(q => q.IdKelas == dto.IdKelas).Select(q => (int?)q.PertemuanKe).Max() ?? 0;

            var quiz = new Quiz
            {
                IdKelas = dto.IdKelas,
                PertemuanKe = lastPertemuan + 1,
                JudulQuiz = dto.JudulQuiz,
                Deskripsi = dto.Deskripsi,
                Durasi = dto.DurasiMenit > 0 ? dto.DurasiMenit : 30,
                PassingScore = 75m,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.Quiz.Add(quiz);
            db.SaveChanges();

            return Ok(ApiResponse<object>.Ok(new { idQuiz = quiz.IdQuiz }, "Kuis online berhasil dibuat."));
        }

        /// <summary>
        /// Menambahkan butir soal pilihan ganda beserta opsi jawaban ke kuis (Role: Guru / Admin)
        /// </summary>
        [HttpPost]
        [Route("{id:int}/soal")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult TambahSoal(int id, [FromBody] TambahSoalQuizRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var quiz = db.Quiz.FirstOrDefault(q => q.IdQuiz == id);
            if (quiz == null) return NotFound();

            var soal = new SoalQuiz
            {
                IdQuiz = id,
                Pertanyaan = dto.Pertanyaan,
                TipeSoal = "PilihanGanda"
            };
            db.SoalQuiz.Add(soal);
            db.SaveChanges();

            if (dto.Opsi != null && dto.Opsi.Any())
            {
                foreach (var op in dto.Opsi)
                {
                    db.OpsiJawaban.Add(new OpsiJawaban
                    {
                        IdSoal = soal.IdSoal,
                        TeksOpsi = op.TeksOpsi,
                        IsCorrect = op.IsBenar
                    });
                }
                db.SaveChanges();
            }

            return Ok(ApiResponse<object>.Ok(new { idSoal = soal.IdSoal }, "Soal kuis berhasil ditambahkan."));
        }

        /// <summary>
        /// Siswa memulai pengerjaan kuis (Mencatat Waktu Mulai)
        /// </summary>
        [HttpPost]
        [Route("{id:int}/start")]
        [JwtAuthorize("Siswa")]
        public IHttpActionResult StartQuiz(int id)
        {
            var claims = User as ClaimsPrincipal;
            string userIdStr = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claims?.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out int idSiswa)) return Unauthorized();

            var quiz = db.Quiz.FirstOrDefault(q => q.IdQuiz == id);
            if (quiz == null) return NotFound();

            var riwayat = db.NilaiQuiz.FirstOrDefault(n => n.IdQuiz == id && n.IdSiswa == idSiswa);
            if (riwayat == null)
            {
                riwayat = new NilaiQuiz
                {
                    IdQuiz = id,
                    IdSiswa = idSiswa,
                    WaktuMulai = DateTime.Now,
                    Score = 0,
                    Status = "InProgress"
                };
                db.NilaiQuiz.Add(riwayat);
                db.SaveChanges();
            }

            var soals = db.SoalQuiz.Where(s => s.IdQuiz == id).Select(s => new
            {
                s.IdSoal,
                s.Pertanyaan,
                Opsi = db.OpsiJawaban.Where(o => o.IdSoal == s.IdSoal).Select(o => new
                {
                    o.IdOpsi,
                    o.TeksOpsi
                }).ToList()
            }).ToList();

            return Ok(ApiResponse<object>.Ok(new
            {
                idQuiz = quiz.IdQuiz,
                judulQuiz = quiz.JudulQuiz,
                durasiMenit = quiz.Durasi,
                waktuMulai = riwayat.WaktuMulai,
                daftarSoal = soals
            }, "Kuis dimulai."));
        }

        /// <summary>
        /// Siswa submit jawaban kuis (Auto-scoring kalkulasi nilai otomatis)
        /// </summary>
        [HttpPost]
        [Route("{id:int}/submit")]
        [JwtAuthorize("Siswa")]
        public IHttpActionResult SubmitQuiz(int id, [FromBody] SubmitQuizRequestDto dto)
        {
            var claims = User as ClaimsPrincipal;
            string userIdStr = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claims?.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out int idSiswa)) return Unauthorized();

            var quiz = db.Quiz.FirstOrDefault(q => q.IdQuiz == id);
            if (quiz == null) return NotFound();

            var soals = db.SoalQuiz.Where(s => s.IdQuiz == id).ToList();
            int totalSoal = soals.Count;
            if (totalSoal == 0) totalSoal = 1;

            int jumlahBenar = 0;

            if (dto != null && dto.Jawaban != null)
            {
                foreach (var j in dto.Jawaban)
                {
                    var opsi = db.OpsiJawaban.FirstOrDefault(o => o.IdOpsi == j.IdOpsiDipilih && o.IdSoal == j.IdSoal);
                    if (opsi != null && opsi.IsCorrect)
                    {
                        jumlahBenar++;
                    }
                }
            }

            decimal nilaiAkhir = Math.Round((decimal)jumlahBenar / totalSoal * 100, 2);

            var record = db.NilaiQuiz.FirstOrDefault(n => n.IdQuiz == id && n.IdSiswa == idSiswa);
            if (record == null)
            {
                record = new NilaiQuiz
                {
                    IdQuiz = id,
                    IdSiswa = idSiswa,
                    WaktuMulai = DateTime.Now,
                    WaktuSubmit = DateTime.Now,
                    Score = nilaiAkhir,
                    Status = nilaiAkhir >= quiz.PassingScore ? "Passed" : "Failed"
                };
                db.NilaiQuiz.Add(record);
            }
            else
            {
                record.WaktuSubmit = DateTime.Now;
                record.Score = nilaiAkhir;
                record.Status = nilaiAkhir >= quiz.PassingScore ? "Passed" : "Failed";
            }

            db.SaveChanges();

            return Ok(ApiResponse<object>.Ok(new
            {
                idQuiz = id,
                nilaiAkhir = nilaiAkhir,
                totalBenar = jumlahBenar,
                totalSoal = totalSoal,
                status = record.Status
            }, "Kuis berhasil diselesaikan dan dinilai otomatis."));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
