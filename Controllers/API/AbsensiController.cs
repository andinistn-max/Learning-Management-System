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
    /// Layanan Presensi dan Jadwal Absensi Siswa REST API
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/absensi")]
    public class AbsensiController : ApiController
    {
        private readonly LmsDbContext db = new LmsDbContext();

        /// <summary>
        /// Pengajar membuka sesi jadwal absensi pertemuan kelas baru (Role: Guru / Admin)
        /// </summary>
        [HttpPost]
        [Route("jadwal")]
        [JwtAuthorize("Guru", "Admin")]
        public IHttpActionResult BukaJadwalAbsen([FromBody] BukaJadwalAbsenRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            int lastPertemuan = db.JadwalAbsen.Where(j => j.IdKelas == dto.IdKelas).Select(j => (int?)j.PertemuanKe).Max() ?? 0;

            var jadwal = new JadwalAbsen
            {
                IdKelas = dto.IdKelas,
                PertemuanKe = lastPertemuan + 1,
                Tanggal = dto.WaktuMulai.Date,
                JamMulai = dto.WaktuMulai.TimeOfDay,
                JamSelesai = dto.WaktuSelesai.TimeOfDay,
                IsOpen = true,
                CreatedAt = DateTime.Now
            };

            db.JadwalAbsen.Add(jadwal);
            db.SaveChanges();

            return Ok(ApiResponse<object>.Ok(new { idJadwal = jadwal.IdJadwal, pertemuanKe = jadwal.PertemuanKe }, "Sesi absensi berhasil dibuka."));
        }

        /// <summary>
        /// Siswa melakukan submit presensi kehadiran
        /// </summary>
        [HttpPost]
        [Route("present")]
        [JwtAuthorize("Siswa")]
        public IHttpActionResult SubmitPresensi([FromBody] PresensiRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var claims = User as ClaimsPrincipal;
            string userIdStr = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claims?.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdStr, out int idSiswa)) return Unauthorized();

            var jadwal = db.JadwalAbsen.FirstOrDefault(j => j.IdJadwal == dto.IdJadwal);
            if (jadwal == null) return NotFound();

            if (!jadwal.IsOpen)
                return BadRequest("Sesi absensi telah ditutup.");

            if (db.Absensi.Any(a => a.IdJadwal == dto.IdJadwal && a.IdSiswa == idSiswa))
                return BadRequest("Anda telah melakukan presensi untuk pertemuan ini.");

            var absensi = new Absensi
            {
                IdJadwal = dto.IdJadwal,
                IdSiswa = idSiswa,
                Status = string.IsNullOrWhiteSpace(dto.Keterangan) ? "Hadir" : dto.Keterangan,
                WaktuAbsen = DateTime.Now
            };

            db.Absensi.Add(absensi);
            db.SaveChanges();

            return Ok(ApiResponse.Ok("Presensi kehadiran berhasil dicatat."));
        }

        /// <summary>
        /// Mengambil rekap kehadiran absensi seluruh siswa pada suatu kelas
        /// </summary>
        [HttpGet]
        [Route("rekap/{idKelas:int}")]
        [AllowAnonymous]
        public IHttpActionResult GetRekapAbsensi(int idKelas)
        {
            var jadwalList = db.JadwalAbsen.Where(j => j.IdKelas == idKelas).OrderBy(j => j.PertemuanKe).ToList();
            int totalSesi = jadwalList.Count;

            var anggota = db.MemberKelas.Include("Siswa").Where(m => m.IdKelas == idKelas).ToList();

            var rekap = anggota.Select(m =>
            {
                int hadir = db.Absensi.Count(a => a.IdSiswa == m.IdSiswa && jadwalList.Select(j => j.IdJadwal).Contains(a.IdJadwal) && a.Status == "Hadir");
                decimal persentase = totalSesi > 0 ? Math.Round((decimal)hadir / totalSesi * 100, 2) : 100m;
                return new
                {
                    m.IdSiswa,
                    NamaSiswa = m.Siswa != null ? m.Siswa.NamaLengkap : "-",
                    TotalSesi = totalSesi,
                    TotalHadir = hadir,
                    PersentaseKehadiran = persentase
                };
            }).ToList();

            return Ok(ApiResponse<object>.Ok(new
            {
                idKelas = idKelas,
                totalSesi = totalSesi,
                rekapSiswa = rekap
            }, "Rekap absensi berhasil dimuat."));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
