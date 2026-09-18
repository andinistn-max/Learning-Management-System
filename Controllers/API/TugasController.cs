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
    /// Layanan Penugasan dan Pengumpulan Tugas REST API
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/tugas")]
    public class TugasController : ApiController
    {
        private readonly LmsDbContext db = new LmsDbContext();

        /// <summary>
        /// Membuat tugas baru di dalam ruang kelas (Role: Guru / Admin)
        /// </summary>
        [HttpPost]
        [Route("")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult BuatTugas([FromBody] BuatTugasRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            int lastPertemuan = db.Tugas.Where(t => t.IdKelas == dto.IdKelas && !t.IsDeleted).Select(t => (int?)t.PertemuanKe).Max() ?? 0;

            var tugas = new Tugas
            {
                IdKelas = dto.IdKelas,
                PertemuanKe = lastPertemuan + 1,
                JudulTugas = dto.JudulTugas,
                Deskripsi = dto.Deskripsi,
                Deadline = dto.Deadline ?? DateTime.Now.AddDays(7),
                FileAttachment = dto.FileLampiranUrl,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            db.Tugas.Add(tugas);
            db.SaveChanges();

            return Ok(ApiResponse<object>.Ok(new { idTugas = tugas.IdTugas }, "Penugasan berhasil dibuat."));
        }

        /// <summary>
        /// Siswa mengumpulkan dokumen jawaban tugas
        /// </summary>
        [HttpPost]
        [Route("submit")]
        [JwtAuthorize("Siswa")]
        public IHttpActionResult SubmitTugas([FromBody] SubmitTugasRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var claims = User as ClaimsPrincipal;
            string userIdStr = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claims?.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out int idSiswa)) return Unauthorized();

            var tugas = db.Tugas.FirstOrDefault(t => t.IdTugas == dto.IdTugas && !t.IsDeleted);
            if (tugas == null) return NotFound();

            var pengumpulan = db.PengumpulanTugas.FirstOrDefault(p => p.IdTugas == dto.IdTugas && p.IdSiswa == idSiswa);
            if (pengumpulan == null)
            {
                pengumpulan = new PengumpulanTugas
                {
                    IdTugas = dto.IdTugas,
                    IdSiswa = idSiswa,
                    FilePath = dto.FileUrl,
                    WaktuKumpul = DateTime.Now,
                    Status = "Submitted",
                    CreatedAt = DateTime.Now
                };
                db.PengumpulanTugas.Add(pengumpulan);
            }
            else
            {
                pengumpulan.FilePath = dto.FileUrl;
                pengumpulan.WaktuKumpul = DateTime.Now;
                pengumpulan.Status = "Submitted";
                pengumpulan.UpdatedAt = DateTime.Now;
            }

            db.SaveChanges();
            return Ok(ApiResponse.Ok("Tugas berhasil dikumpulkan."));
        }

        /// <summary>
        /// Guru memberikan nilai dan catatan feedback pada submisi tugas siswa (Role: Guru / Admin)
        /// </summary>
        [HttpPost]
        [Route("nilai")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult NilaiTugas([FromBody] NilaiTugasRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var pengumpulan = db.PengumpulanTugas.FirstOrDefault(p => p.IdKumpul == dto.IdPengumpulan);
            if (pengumpulan == null) return NotFound();

            pengumpulan.Nilai = dto.Nilai;
            pengumpulan.Feedback = dto.Feedback;
            pengumpulan.Status = "Graded";
            pengumpulan.UpdatedAt = DateTime.Now;

            db.SaveChanges();
            return Ok(ApiResponse.Ok("Nilai dan evaluasi tugas berhasil disimpan."));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
