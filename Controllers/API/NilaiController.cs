using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Learning_Management_System.Helpers;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Controllers.API
{
    /// <summary>
    /// Layanan Rekapitulasi Nilai Akademik &amp; Evaluasi Siswa REST API
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/nilai")]
    public class NilaiController : ApiController
    {
        private readonly LmsDbContext db = new LmsDbContext();

        /// <summary>
        /// Mengambil rekap nilai akumulasi seluruh siswa pada kelas (Formula Resmi: Absen 15%, Materi 15%, Quiz 40%, Tugas 30%)
        /// </summary>
        [HttpGet]
        [Route("kelas/{idKelas:int}")]
        [AllowAnonymous]
        public IHttpActionResult GetRekapNilaiKelas(int idKelas)
        {
            var kelas = db.Kelas.FirstOrDefault(k => k.IdKelas == idKelas && !k.IsDeleted);
            if (kelas == null) return NotFound();

            var anggotaList = db.MemberKelas.Include("Siswa").Where(m => m.IdKelas == idKelas).ToList();

            var listJadwal = db.JadwalAbsen.Where(j => j.IdKelas == idKelas).Select(j => j.IdJadwal).ToList();
            int totalSesiAbsen = listJadwal.Count;

            var listMateri = db.Materi.Where(m => m.IdKelas == idKelas && !m.IsDeleted).Select(m => m.IdMateri).ToList();
            int totalMateri = listMateri.Count;

            var listQuiz = db.Quiz.Where(q => q.IdKelas == idKelas).Select(q => q.IdQuiz).ToList();
            var listTugas = db.Tugas.Where(t => t.IdKelas == idKelas && !t.IsDeleted).Select(t => t.IdTugas).ToList();

            var rekap = anggotaList.Select(m =>
            {
                int idSiswa = m.IdSiswa;

                // 1. Nilai Presensi (Bobot 15%)
                decimal skorAbsen = 100m;
                if (totalSesiAbsen > 0)
                {
                    int hadir = db.Absensi.Count(a => a.IdSiswa == idSiswa && listJadwal.Contains(a.IdJadwal) && a.Status == "Hadir");
                    skorAbsen = Math.Round((decimal)hadir / totalSesiAbsen * 100, 2);
                }

                // 2. Nilai Progres Materi (Bobot 15%)
                decimal skorMateri = 100m;
                if (totalMateri > 0)
                {
                    int selesai = db.ProgresMateri.Count(p => p.IdSiswa == idSiswa && listMateri.Contains(p.IdMateri) && p.IsSelesai);
                    skorMateri = Math.Round((decimal)selesai / totalMateri * 100, 2);
                }

                // 3. Nilai Quiz (Bobot 40%)
                decimal skorQuiz = 0m;
                if (listQuiz.Any())
                {
                    var nilaiQuizList = db.NilaiQuiz.Where(n => n.IdSiswa == idSiswa && listQuiz.Contains(n.IdQuiz) && n.Score.HasValue).Select(n => n.Score.Value).ToList();
                    if (nilaiQuizList.Any())
                    {
                        skorQuiz = Math.Round(nilaiQuizList.Average(), 2);
                    }
                }

                // 4. Nilai Tugas (Bobot 30%)
                decimal skorTugas = 0m;
                if (listTugas.Any())
                {
                    var tugasTerkumpul = db.PengumpulanTugas.Where(p => p.IdSiswa == idSiswa && listTugas.Contains(p.IdTugas) && p.Nilai.HasValue).Select(p => p.Nilai.Value).ToList();
                    if (tugasTerkumpul.Any())
                    {
                        skorTugas = Math.Round(tugasTerkumpul.Average(), 2);
                    }
                }

                // Kalkulasi Nilai Akhir
                decimal nilaiAkhir = Math.Round((skorAbsen * 0.15m) + (skorMateri * 0.15m) + (skorQuiz * 0.40m) + (skorTugas * 0.30m), 2);

                string grade = "E";
                if (nilaiAkhir >= 85) grade = "A";
                else if (nilaiAkhir >= 75) grade = "B";
                else if (nilaiAkhir >= 65) grade = "C";
                else if (nilaiAkhir >= 50) grade = "D";

                return new
                {
                    m.IdSiswa,
                    NamaSiswa = m.Siswa != null ? m.Siswa.NamaLengkap : "-",
                    NilaiAbsensi = skorAbsen,
                    NilaiMateri = skorMateri,
                    NilaiQuiz = skorQuiz,
                    NilaiTugas = skorTugas,
                    NilaiAkhir = nilaiAkhir,
                    Grade = grade,
                    Status = nilaiAkhir >= 65 ? "Lulus" : "Perlu Bimbingan"
                };
            }).ToList();

            return Ok(ApiResponse<object>.Ok(new
            {
                idKelas = kelas.IdKelas,
                namaKelas = kelas.NamaKelas,
                formula = "15% Absensi + 15% Materi + 40% Quiz + 30% Tugas",
                rekapNilai = rekap
            }, "Rekapitulasi nilai berhasil dihitung."));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
