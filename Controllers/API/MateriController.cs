using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Cors;
using Learning_Management_System.Helpers;
using Learning_Management_System.Models.Entity;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Controllers.API
{
    /// <summary>
    /// Layanan Modul Materi Pembelajaran REST API
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/materi")]
    public class MateriController : ApiController
    {
        private readonly LmsDbContext db = new LmsDbContext();

        /// <summary>
        /// Mengambil daftar seluruh materi pembelajaran pada suatu kelas
        /// </summary>
        [HttpGet]
        [Route("kelas/{idKelas:int}")]
        [AllowAnonymous]
        public IHttpActionResult GetMateriByKelas(int idKelas)
        {
            var list = db.Materi.Where(m => m.IdKelas == idKelas && !m.IsDeleted)
                .OrderBy(m => m.PertemuanKe)
                .Select(m => new
                {
                    m.IdMateri,
                    m.IdKelas,
                    m.PertemuanKe,
                    m.JudulMateri,
                    m.Deskripsi,
                    m.NamaFile,
                    m.FilePath,
                    m.TipeMateri,
                    m.CreatedAt
                }).ToList();

            return Ok(ApiResponse<object>.Ok(list, "Daftar materi berhasil diambil."));
        }

        /// <summary>
        /// Mengunggah / membuat modul materi pembelajaran baru (Role: Guru / Admin)
        /// </summary>
        [HttpPost]
        [Route("")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult TambahMateri([FromBody] Materi materi)
        {
            if (materi == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            materi.IsDeleted = false;
            materi.CreatedAt = DateTime.Now;

            db.Materi.Add(materi);
            db.SaveChanges();

            return Ok(ApiResponse<object>.Ok(new { idMateri = materi.IdMateri }, "Materi pembelajaran berhasil ditambahkan."));
        }

        /// <summary>
        /// Soft delete materi pembelajaran (IsDeleted = true)
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult DeleteMateri(int id)
        {
            var m = db.Materi.FirstOrDefault(x => x.IdMateri == id && !x.IsDeleted);
            if (m == null) return NotFound();

            m.IsDeleted = true;
            m.DeletedAt = DateTime.Now;
            db.SaveChanges();

            return Ok(ApiResponse.Ok("Materi berhasil dihapus."));
        }

        /// <summary>
        /// Siswa menandai materi telah selesai dipelajari
        /// </summary>
        [HttpPost]
        [Route("{id:int}/selesai")]
        [JwtAuthorize("Siswa")]
        public IHttpActionResult TandaiSelesai(int id)
        {
            var claims = User as ClaimsPrincipal;
            string userIdStr = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claims?.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out int idSiswa)) return Unauthorized();

            var materi = db.Materi.FirstOrDefault(m => m.IdMateri == id && !m.IsDeleted);
            if (materi == null) return NotFound();

            var progres = db.ProgresMateri.FirstOrDefault(p => p.IdMateri == id && p.IdSiswa == idSiswa);
            if (progres == null)
            {
                progres = new ProgresMateri
                {
                    IdMateri = id,
                    IdSiswa = idSiswa,
                    IsSelesai = true,
                    DiselesaikanPada = DateTime.Now
                };
                db.ProgresMateri.Add(progres);
            }
            else
            {
                progres.IsSelesai = true;
                progres.DiselesaikanPada = DateTime.Now;
            }

            db.SaveChanges();
            return Ok(ApiResponse.Ok("Progres materi berhasil disimpan."));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
