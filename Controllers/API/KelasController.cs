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
    /// Layanan Manajemen Ruang Kelas REST API
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/kelas")]
    public class KelasController : ApiController
    {
        private readonly LmsDbContext db = new LmsDbContext();

        /// <summary>
        /// Mengambil seluruh data katalog kelas pembelajaran aktif
        /// </summary>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public IHttpActionResult GetDaftarKelas(string search = null, string kategori = null, string sort = null, int page = 1, int limit = 10)
        {
            var query = db.Kelas.Include("Kategori").Include("Guru").Where(k => !k.IsDeleted && k.IsPublish);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(k => k.NamaKelas.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(kategori))
            {
                query = query.Where(k => k.Kategori.NamaKategori.ToLower() == kategori.Trim().ToLower());
            }

            switch (sort?.ToLower())
            {
                case "terlama":
                    query = query.OrderBy(k => k.CreatedAt);
                    break;
                case "nama_asc":
                    query = query.OrderBy(k => k.NamaKelas);
                    break;
                case "nama_desc":
                    query = query.OrderByDescending(k => k.NamaKelas);
                    break;
                default:
                    query = query.OrderByDescending(k => k.CreatedAt);
                    break;
            }

            int totalItems = query.Count();
            var items = query.Skip((page - 1) * limit).Take(limit).Select(k => new
            {
                k.IdKelas,
                k.NamaKelas,
                k.KodeKelas,
                Kategori = k.Kategori != null ? k.Kategori.NamaKategori : "-",
                GuruPengampu = k.Guru != null ? k.Guru.NamaLengkap : "-",
                Thumbnail = k.Thumbnail,
                k.Deskripsi,
                k.CreatedAt
            }).ToList();

            return Ok(ApiResponse<object>.Ok(new
            {
                totalItems = totalItems,
                page = page,
                limit = limit,
                items = items
            }, "Data kelas berhasil dimuat."));
        }

        /// <summary>
        /// Mengambil rincian detail kelas berdasarkan ID
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public IHttpActionResult GetDetailKelas(int id)
        {
            var k = db.Kelas.Include("Kategori").Include("Guru").FirstOrDefault(x => x.IdKelas == id && !x.IsDeleted);
            if (k == null) return NotFound();

            int totalSiswa = db.MemberKelas.Count(m => m.IdKelas == id);
            int totalMateri = db.Materi.Count(m => m.IdKelas == id && !m.IsDeleted);
            int totalTugas = db.Tugas.Count(t => t.IdKelas == id && !t.IsDeleted);
            int totalQuiz = db.Quiz.Count(q => q.IdKelas == id);

            return Ok(ApiResponse<object>.Ok(new
            {
                k.IdKelas,
                k.NamaKelas,
                k.KodeKelas,
                Kategori = k.Kategori != null ? k.Kategori.NamaKategori : "-",
                GuruPengampu = k.Guru != null ? k.Guru.NamaLengkap : "-",
                Thumbnail = k.Thumbnail,
                k.Deskripsi,
                TotalSiswa = totalSiswa,
                TotalMateri = totalMateri,
                TotalTugas = totalTugas,
                TotalQuiz = totalQuiz,
                k.CreatedAt
            }, "Detail kelas berhasil diambil."));
        }

        /// <summary>
        /// Membuat kelas pembelajaran baru (Role: Guru / Admin)
        /// </summary>
        [HttpPost]
        [Route("")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult BuatKelas([FromBody] BuatKelasRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var claims = User as ClaimsPrincipal;
            string userIdStr = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claims?.FindFirst("userId")?.Value;
            int idGuru = int.TryParse(userIdStr, out int uid) ? uid : 1;

            var kelasBaru = new Kelas
            {
                NamaKelas = dto.NamaKelas,
                IdKategori = dto.IdKategori,
                IdGuru = idGuru,
                Deskripsi = dto.Deskripsi,
                Thumbnail = string.IsNullOrWhiteSpace(dto.ThumbnailUrl) ? "/Content/images/kelas/default-course.jpg" : dto.ThumbnailUrl,
                IsPublish = true,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            db.Kelas.Add(kelasBaru);
            db.SaveChanges();

            return Ok(ApiResponse<object>.Ok(new
            {
                idKelas = kelasBaru.IdKelas,
                kodeKelas = kelasBaru.KodeKelas,
                namaKelas = kelasBaru.NamaKelas
            }, "Kelas berhasil dibuat."));
        }

        /// <summary>
        /// Memperbarui informasi ruang kelas (Role: Guru / Admin)
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult UpdateKelas(int id, [FromBody] UpdateKelasRequestDto dto)
        {
            if (dto == null) return BadRequest();

            var kelas = db.Kelas.FirstOrDefault(k => k.IdKelas == id && !k.IsDeleted);
            if (kelas == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.NamaKelas)) kelas.NamaKelas = dto.NamaKelas;
            if (dto.IdKategori.HasValue) kelas.IdKategori = dto.IdKategori.Value;
            if (dto.Deskripsi != null) kelas.Deskripsi = dto.Deskripsi;
            if (!string.IsNullOrWhiteSpace(dto.ThumbnailUrl)) kelas.Thumbnail = dto.ThumbnailUrl;

            kelas.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            return Ok(ApiResponse.Ok("Data kelas berhasil diperbarui."));
        }

        /// <summary>
        /// Menghapus ruang kelas secara Soft Delete (IsDeleted = true)
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult SoftDeleteKelas(int id)
        {
            var kelas = db.Kelas.FirstOrDefault(k => k.IdKelas == id && !k.IsDeleted);
            if (kelas == null) return NotFound();

            kelas.IsDeleted = true;
            kelas.DeletedAt = DateTime.Now;
            db.SaveChanges();

            return Ok(ApiResponse.Ok("Kelas berhasil dihapus secara soft-delete."));
        }

        /// <summary>
        /// Siswa mendaftar dan bergabung ke dalam ruang kelas
        /// </summary>
        [HttpPost]
        [Route("{id:int}/join")]
        [JwtAuthorize("Siswa")]
        public IHttpActionResult JoinKelas(int id)
        {
            var claims = User as ClaimsPrincipal;
            string userIdStr = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claims?.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out int idSiswa)) return Unauthorized();

            var kelas = db.Kelas.FirstOrDefault(k => k.IdKelas == id && !k.IsDeleted && k.IsPublish);
            if (kelas == null) return NotFound();

            if (db.MemberKelas.Any(m => m.IdKelas == id && m.IdSiswa == idSiswa))
                return BadRequest("Anda sudah terdaftar dalam kelas ini.");

            var member = new MemberKelas
            {
                IdKelas = id,
                IdSiswa = idSiswa,
                TglJoin = DateTime.Now,
                Status = "Active"
            };

            db.MemberKelas.Add(member);
            db.SaveChanges();

            return Ok(ApiResponse.Ok("Berhasil bergabung ke kelas " + kelas.NamaKelas));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
