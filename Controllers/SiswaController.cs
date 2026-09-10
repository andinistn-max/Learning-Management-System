using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Learning_Management_System.Models.Entity;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Controllers
{
    public class SiswaController : Controller
    {
        private readonly LmsDbContext _db = new LmsDbContext();

        // GET: /Siswa/Dashboard
        [HttpGet]
        public ActionResult Dashboard()
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Ambil seluruh kelas yang diikuti siswa (Status == Active)
            var memberKelasList = _db.MemberKelas
                .Include(m => m.Kelas)
                .Include(m => m.Kelas.Kategori)
                .Include(m => m.Kelas.Guru)
                .Where(m => m.IdSiswa == currentUserId && m.Status == "Active" && !m.Kelas.IsDeleted)
                .OrderByDescending(m => m.TglJoin)
                .ToList();

            var kelasIds = memberKelasList.Select(m => m.IdKelas).ToList();

            // Total Kelas
            int totalKelas = memberKelasList.Count;

            // Ambil semua materi di kelas-kelas tersebut
            var materiList = _db.Materi
                .Where(m => kelasIds.Contains(m.IdKelas) && !m.IsDeleted)
                .ToList();

            var progresMateriList = _db.ProgresMateri
                .Where(p => p.IdSiswa == currentUserId && p.IsSelesai)
                .ToList();

            // Ambil semua tugas di kelas-kelas tersebut
            var tugasList = _db.Tugas
                .Where(t => kelasIds.Contains(t.IdKelas) && !t.IsDeleted)
                .ToList();

            var allTugasIds = tugasList.Select(t => t.IdTugas).ToList();

            var tugasSubmissions = _db.PengumpulanTugas
                .Where(s => s.IdSiswa == currentUserId && allTugasIds.Contains(s.IdTugas))
                .ToList();

            // Tugas Pending: Tugas yang belum dikumpulkan dan deadline-nya belum lewat
            var submittedTugasIds = tugasSubmissions.Where(s => s.WaktuKumpul.HasValue).Select(s => s.IdTugas).ToList();
            int totalTugasPending = tugasList.Count(t => !submittedTugasIds.Contains(t.IdTugas) && t.Deadline >= DateTime.Now);

            // Ambil semua quiz di kelas-kelas tersebut
            var quizList = _db.Quiz
                .Where(q => kelasIds.Contains(q.IdKelas))
                .ToList();

            var quizAttempts = _db.NilaiQuiz
                .Where(n => n.IdSiswa == currentUserId && n.Score.HasValue)
                .ToList();

            var attemptedQuizIds = quizAttempts.Select(n => n.IdQuiz).ToList();
            int totalKuisTersedia = quizList.Count(q => !attemptedQuizIds.Contains(q.IdQuiz) && q.IsActive);

            // Kalkulasi Rata-rata Nilai Keseluruhan (Tugas + Quiz)
            var validTugasScores = tugasSubmissions.Where(s => s.Nilai.HasValue).Select(s => s.Nilai.Value).ToList();
            var validQuizScores = quizAttempts.Where(q => q.Score.HasValue).Select(q => q.Score.Value).ToList();
            var allScores = validTugasScores.Concat(validQuizScores).ToList();

            decimal avgAll = allScores.Any() ? Math.Round(allScores.Average(), 1) : 0m;

            // Map Daftar Kelas Siswa + Progress Belajar
            var daftarKelasDto = memberKelasList.Select(m =>
            {
                var k = m.Kelas;
                int totalMateriKelas = materiList.Count(mat => mat.IdKelas == k.IdKelas);
                int countMateriSelesai = progresMateriList.Count(p => materiList.Where(mat => mat.IdKelas == k.IdKelas).Select(mat => mat.IdMateri).Contains(p.IdMateri));

                decimal progressPct = totalMateriKelas > 0 ? ((decimal)countMateriSelesai / totalMateriKelas) * 100m : 100m;
                if (progressPct > 100m) progressPct = 100m;

                return new SiswaKelasItemDto
                {
                    KelasId = k.IdKelas,
                    NamaKelas = k.NamaKelas,
                    KodeKelas = $"KLS-{k.IdKelas:D3}",
                    Kategori = k.Kategori != null ? k.Kategori.NamaKategori : "Umum",
                    NamaGuru = k.Guru != null ? k.Guru.NamaLengkap : "Pengajar",
                    FotoGuru = k.Guru != null ? k.Guru.FotoProfile : null,
                    ThumbnailUrl = !string.IsNullOrEmpty(k.Thumbnail) ? k.Thumbnail : "/Content/images/default-course.jpg",
                    PersentaseProgresBelajar = Math.Round(progressPct, 1),
                    TotalMateri = totalMateriKelas,
                    TotalTugas = tugasList.Count(t => t.IdKelas == k.IdKelas),
                    TotalQuiz = quizList.Count(q => q.IdKelas == k.IdKelas),
                    TglJoin = m.TglJoin
                };
            }).ToList();

            // Daftar Deadline Mendekat (Tugas Pending & Quiz Aktif)
            var deadlineItems = new List<DeadlineItemDto>();

            foreach (var t in tugasList.Where(t => !submittedTugasIds.Contains(t.IdTugas)))
            {
                var k = memberKelasList.FirstOrDefault(m => m.IdKelas == t.IdKelas)?.Kelas;
                bool overdue = DateTime.Now > t.Deadline;

                string timeFormatted = overdue ? "Telah Berakhir" : GetTimeRemainingFormatted(t.Deadline);

                deadlineItems.Add(new DeadlineItemDto
                {
                    ItemType = "Tugas",
                    ItemId = t.IdTugas,
                    KelasId = t.IdKelas,
                    NamaKelas = k != null ? k.NamaKelas : "Kelas",
                    Judul = t.JudulTugas,
                    Deadline = t.Deadline,
                    IsOverdue = overdue,
                    TimeRemainingFormatted = timeFormatted
                });
            }

            foreach (var q in quizList.Where(q => !attemptedQuizIds.Contains(q.IdQuiz) && q.IsActive && q.WaktuSelesai.HasValue))
            {
                var k = memberKelasList.FirstOrDefault(m => m.IdKelas == q.IdKelas)?.Kelas;
                bool overdue = DateTime.Now > q.WaktuSelesai.Value;

                deadlineItems.Add(new DeadlineItemDto
                {
                    ItemType = "Quiz",
                    ItemId = q.IdQuiz,
                    KelasId = q.IdKelas,
                    NamaKelas = k != null ? k.NamaKelas : "Kelas",
                    Judul = q.JudulQuiz,
                    Deadline = q.WaktuSelesai,
                    IsOverdue = overdue,
                    TimeRemainingFormatted = overdue ? "Telah Berakhir" : GetTimeRemainingFormatted(q.WaktuSelesai.Value)
                });
            }

            var sortedDeadlineItems = deadlineItems.OrderBy(d => d.IsOverdue).ThenBy(d => d.Deadline).Take(6).ToList();

            var viewModel = new SiswaDashboardViewModel
            {
                TotalKelasDiikuti = totalKelas,
                TotalTugasPending = totalTugasPending,
                TotalKuisTersedia = totalKuisTersedia,
                RataRataNilaiKeseluruhan = avgAll,
                DaftarKelasSiswa = daftarKelasDto,
                DaftarDeadlineMendekat = sortedDeadlineItems
            };

            ViewBag.Title = "Dashboard Siswa";
            return View(viewModel);
        }

        // GET: /Siswa/KatalogKelas
        [HttpGet]
        public ActionResult KatalogKelas(string search = "", string kategori = "")
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "Katalog & Cari Kelas";

            var enrolledClassIds = _db.MemberKelas
                .Where(m => m.IdSiswa == currentUserId && m.Status == "Active")
                .Select(m => m.IdKelas)
                .ToList();

            var query = _db.Kelas
                .Include(k => k.Kategori)
                .Include(k => k.Guru)
                .Where(k => !k.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLower();
                query = query.Where(k => k.NamaKelas.ToLower().Contains(searchLower) ||
                                         (k.Deskripsi != null && k.Deskripsi.ToLower().Contains(searchLower)) ||
                                         (k.Guru != null && k.Guru.NamaLengkap.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrWhiteSpace(kategori))
            {
                query = query.Where(k => k.Kategori != null && k.Kategori.NamaKategori == kategori);
            }

            var allCategories = _db.Kategori
                .Select(c => c.NamaKategori)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var rawKelasList = query.OrderByDescending(k => k.CreatedAt).ToList();

            var allActiveMembers = _db.MemberKelas
                .Where(m => m.Status == "Active")
                .GroupBy(m => m.IdKelas)
                .ToDictionary(g => g.Key, g => g.Count());

            var allActiveMateri = _db.Materi
                .Where(m => !m.IsDeleted)
                .GroupBy(m => m.IdKelas)
                .ToDictionary(g => g.Key, g => g.Count());

            var allActiveTugas = _db.Tugas
                .Where(t => !t.IsDeleted)
                .GroupBy(t => t.IdKelas)
                .ToDictionary(g => g.Key, g => g.Count());

            var allActiveQuiz = _db.Quiz
                .GroupBy(q => q.IdKelas)
                .ToDictionary(g => g.Key, g => g.Count());

            var dtoList = rawKelasList.Select(k => new KatalogKelasItemDto
            {
                IdKelas = k.IdKelas,
                NamaKelas = k.NamaKelas,
                KodeKelas = $"KLS-{k.IdKelas:D3}",
                Deskripsi = k.Deskripsi,
                BannerImage = k.Thumbnail,
                NamaKategori = k.Kategori != null ? k.Kategori.NamaKategori : "Umum",
                NamaGuru = k.Guru != null ? k.Guru.NamaLengkap : "Pengajar PUB",
                FotoGuru = k.Guru != null ? k.Guru.FotoProfile : null,
                TotalSiswa = allActiveMembers.ContainsKey(k.IdKelas) ? allActiveMembers[k.IdKelas] : 0,
                TotalMateri = allActiveMateri.ContainsKey(k.IdKelas) ? allActiveMateri[k.IdKelas] : 0,
                TotalTugas = allActiveTugas.ContainsKey(k.IdKelas) ? allActiveTugas[k.IdKelas] : 0,
                TotalQuiz = allActiveQuiz.ContainsKey(k.IdKelas) ? allActiveQuiz[k.IdKelas] : 0,
                IsEnrolled = enrolledClassIds.Contains(k.IdKelas)
            }).ToList();

            var viewModel = new SiswaKatalogKelasViewModel
            {
                SearchKeyword = search,
                FilterKategori = kategori,
                DaftarKategoriOptions = allCategories,
                TotalKelasTersedia = rawKelasList.Count,
                TotalKelasDiikuti = enrolledClassIds.Count,
                DaftarKelas = dtoList
            };

            return View(viewModel);
        }

        // POST: /Siswa/GabungKelas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GabungKelas(GabungKelasViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                TempData["ErrorMessage"] = "Silakan login terlebih dahulu untuk bergabung ke kelas.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.KodeKelas))
            {
                TempData["ErrorMessage"] = "Mohon masukkan kode akses kelas dengan benar.";
                return RedirectToAction("Dashboard");
            }

            string inputCode = model.KodeKelas.Trim().ToUpper();

            int parsedId = 0;
            if (inputCode.StartsWith("KLS-", StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(inputCode.Substring(4), out parsedId);
            }
            else
            {
                int.TryParse(inputCode, out parsedId);
            }

            // Cari kelas berdasarkan KodeKelas di database
            var listKelas = _db.Kelas.Where(k => !k.IsDeleted).ToList();
            var kelas = listKelas.FirstOrDefault(k => 
                k.IdKelas == parsedId || 
                ("KLS-" + k.IdKelas.ToString("D3")).Equals(inputCode, StringComparison.OrdinalIgnoreCase) ||
                (k.KodeKelas != null && k.KodeKelas.Equals(inputCode, StringComparison.OrdinalIgnoreCase)));

            if (kelas == null)
            {
                TempData["ErrorMessage"] = $"Kode kelas '{inputCode}' tidak ditemukan atau kelas sudah tidak aktif.";
                return RedirectToAction("Dashboard");
            }

            // Validasi apakah siswa sudah terdaftar sebelumnya
            bool alreadyEnrolled = _db.MemberKelas.Any(m => m.IdKelas == kelas.IdKelas && m.IdSiswa == currentUserId && m.Status == "Active");
            if (alreadyEnrolled)
            {
                TempData["ErrorMessage"] = $"Anda sudah terdaftar di kelas '{kelas.NamaKelas}'.";
                return RedirectToAction("RuangKelas", new { id = kelas.IdKelas });
            }

            // Tambahkan record baru ke MemberKelas
            var newEnrollment = new MemberKelas
            {
                IdSiswa = currentUserId,
                IdKelas = kelas.IdKelas,
                TglJoin = DateTime.Now,
                Status = "Active"
            };

            _db.MemberKelas.Add(newEnrollment);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Selamat! Anda telah berhasil bergabung ke kelas '{kelas.NamaKelas}'.";
            return RedirectToAction("RuangKelas", new { id = kelas.IdKelas });
        }

        // GET: /Siswa/RuangKelas/{id}
        [HttpGet]
        public ActionResult RuangKelas(int id, string tab = "stream")
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = _db.MemberKelas
                .Include(m => m.Kelas)
                .Include(m => m.Kelas.Kategori)
                .Include(m => m.Kelas.Guru)
                .FirstOrDefault(m => m.IdKelas == id && m.IdSiswa == currentUserId && m.Status == "Active" && !m.Kelas.IsDeleted);

            if (member == null || member.Kelas == null)
            {
                TempData["ErrorMessage"] = "Anda tidak terdaftar di kelas ini atau kelas sudah ditutup.";
                return RedirectToAction("Dashboard");
            }

            var kelas = member.Kelas;

            // Load Stream Posts
            var posts = _db.StreamPostingan
                .Include(p => p.User)
                .Include(p => p.User.Role)
                .Where(p => p.IdKelas == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var postIds = posts.Select(p => p.IdStream).ToList();

            var comments = _db.KomentarStream
                .Include(c => c.User)
                .Where(c => postIds.Contains(c.IdStream))
                .OrderBy(c => c.CreatedAt)
                .ToList();

            var postDtos = posts.Select(p => new GuruStreamPostDto
            {
                IdPost = p.IdStream,
                IdUser = p.IdUser,
                NamaPenulis = p.User != null ? p.User.NamaLengkap : "User",
                FotoPenulis = p.User != null ? p.User.FotoProfile : null,
                RolePenulis = p.User != null && p.User.Role != null ? p.User.Role.NamaRole : "User",
                Konten = p.Pesan,
                FileAttachment = p.AttachmentUrl,
                NamaFile = !string.IsNullOrEmpty(p.AttachmentUrl) ? System.IO.Path.GetFileName(p.AttachmentUrl) : null,
                CreatedAt = p.CreatedAt,
                IsAuthor = p.IdUser == currentUserId,
                ListKomentar = comments.Where(c => c.IdStream == p.IdStream).Select(c => new GuruStreamCommentDto
                {
                    IdComment = c.IdKomentar,
                    IdPost = c.IdStream,
                    IdUser = c.IdUser,
                    NamaPenulis = c.User != null ? c.User.NamaLengkap : "User",
                    FotoPenulis = c.User != null ? c.User.FotoProfile : null,
                    RolePenulis = c.User != null && c.User.Role != null ? c.User.Role.NamaRole : "User",
                    Konten = c.Komentar,
                    CreatedAt = c.CreatedAt,
                    IsAuthor = c.IdUser == currentUserId
                }).ToList()
            }).ToList();

            // Query Data Materi jika tab == "materi"
            if (tab.Equals("materi", StringComparison.OrdinalIgnoreCase))
            {
                var rawMateri = _db.Materi
                    .Where(m => m.IdKelas == id && !m.IsDeleted)
                    .OrderBy(m => m.PertemuanKe)
                    .ThenByDescending(m => m.CreatedAt)
                    .ToList();

                var materiIds = rawMateri.Select(m => m.IdMateri).ToList();

                var progresList = _db.ProgresMateri
                    .Where(p => p.IdSiswa == currentUserId && materiIds.Contains(p.IdMateri) && p.IsSelesai)
                    .ToList();

                var materiItemDtos = rawMateri.Select(m =>
                {
                    var prg = progresList.FirstOrDefault(p => p.IdMateri == m.IdMateri);
                    return new SiswaMateriItemDto
                    {
                        IdMateri = m.IdMateri,
                        PertemuanKe = m.PertemuanKe,
                        Judul = m.JudulMateri,
                        Deskripsi = m.Deskripsi,
                        TipeFile = m.TipeMateri,
                        FileUrl = m.FilePath,
                        NamaFile = !string.IsNullOrEmpty(m.NamaFile) ? m.NamaFile : (!string.IsNullOrEmpty(m.FilePath) ? System.IO.Path.GetFileName(m.FilePath) : "Berkas Materi"),
                        UkuranFileFormatted = FormatBytes(m.FileSize),
                        VideoUrl = m.TipeMateri != null && m.TipeMateri.Equals("Video", StringComparison.OrdinalIgnoreCase) ? m.FilePath : null,
                        CreatedAt = m.CreatedAt,
                        IsSudahDibaca = prg != null,
                        TanggalDibaca = prg != null ? prg.DiselesaikanPada : null
                    };
                }).ToList();

                int totalMateriCount = rawMateri.Count;
                int totalSelesaiCount = materiItemDtos.Count(dto => dto.IsSudahDibaca);
                decimal pct = totalMateriCount > 0 ? ((decimal)totalSelesaiCount / totalMateriCount) * 100m : 100m;

                ViewBag.MateriViewModel = new SiswaMateriListViewModel
                {
                    KelasId = kelas.IdKelas,
                    NamaKelas = kelas.NamaKelas,
                    KodeKelas = $"KLS-{kelas.IdKelas:D3}",
                    Kategori = kelas.Kategori != null ? kelas.Kategori.NamaKategori : "Umum",
                    NamaGuru = kelas.Guru != null ? kelas.Guru.NamaLengkap : "Guru",
                    FotoGuru = kelas.Guru != null ? kelas.Guru.FotoProfile : null,
                    TotalMateri = totalMateriCount,
                    TotalMateriSelesai = totalSelesaiCount,
                    PersentaseProgres = Math.Round(pct, 1),
                    DaftarMateri = materiItemDtos
                };
            }

            // Query Data Tugas jika tab == "tugas"
            if (tab.Equals("tugas", StringComparison.OrdinalIgnoreCase))
            {
                var rawTugas = _db.Tugas
                    .Where(t => t.IdKelas == id && !t.IsDeleted)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();

                var tugasIds = rawTugas.Select(t => t.IdTugas).ToList();

                var submissions = _db.PengumpulanTugas
                    .Where(s => s.IdSiswa == currentUserId && tugasIds.Contains(s.IdTugas))
                    .ToList();

                var tugasDtos = rawTugas.Select(t =>
                {
                    var sub = submissions.FirstOrDefault(s => s.IdTugas == t.IdTugas);
                    bool overdue = DateTime.Now > t.Deadline;

                    string statusStr = "Belum Dikumpulkan";
                    bool isLate = false;
                    bool isGraded = false;

                    if (sub != null && sub.WaktuKumpul.HasValue)
                    {
                        if (sub.Nilai.HasValue)
                        {
                            statusStr = "Sudah Dinilai";
                            isGraded = true;
                        }
                        else if (sub.WaktuKumpul > t.Deadline || sub.Status.Equals("Late", StringComparison.OrdinalIgnoreCase))
                        {
                            statusStr = "Terlambat";
                            isLate = true;
                        }
                        else
                        {
                            statusStr = "Diserahkan";
                        }
                    }
                    else if (overdue)
                    {
                        statusStr = "Belum Dikumpulkan (Lewat Deadline)";
                    }

                    return new SiswaTugasItemDto
                    {
                        IdTugas = t.IdTugas,
                        JudulTugas = t.JudulTugas,
                        Instruksi = t.Deskripsi,
                        Deadline = t.Deadline,
                        FileLampiranSoalUrl = t.FileAttachment,
                        NamaFileLampiran = !string.IsNullOrEmpty(t.FileAttachment) ? System.IO.Path.GetFileName(t.FileAttachment) : null,
                        StatusPengumpulan = statusStr,
                        IsLate = isLate,
                        IsOverdue = overdue,
                        SubmissionId = sub != null ? (int?)sub.IdKumpul : null,
                        FileSubmisiUrl = sub != null ? sub.FilePath : null,
                        NamaFileSubmisi = sub != null && !string.IsNullOrEmpty(sub.FilePath) ? System.IO.Path.GetFileName(sub.FilePath) : null,
                        WaktuKumpul = sub != null ? sub.WaktuKumpul : null,
                        Nilai = sub != null ? sub.Nilai : null,
                        FeedbackGuru = sub != null ? sub.Feedback : null,
                        IsSudahDinilai = isGraded,
                        CanCancelSubmission = sub != null && sub.WaktuKumpul.HasValue && !isGraded,
                        CreatedAt = t.CreatedAt
                    };
                }).ToList();

                int totalTugasCount = rawTugas.Count;
                int totalSudahCount = tugasDtos.Count(dto => dto.SubmissionId.HasValue && dto.WaktuKumpul.HasValue);
                int totalBelumCount = totalTugasCount - totalSudahCount;

                ViewBag.TugasViewModel = new SiswaTugasListViewModel
                {
                    KelasId = kelas.IdKelas,
                    NamaKelas = kelas.NamaKelas,
                    KodeKelas = $"KLS-{kelas.IdKelas:D3}",
                    Kategori = kelas.Kategori != null ? kelas.Kategori.NamaKategori : "Umum",
                    NamaGuru = kelas.Guru != null ? kelas.Guru.NamaLengkap : "Guru",
                    FotoGuru = kelas.Guru != null ? kelas.Guru.FotoProfile : null,
                    TotalTugas = totalTugasCount,
                    TotalSudahDikumpulkan = totalSudahCount,
                    TotalBelumDikumpulkan = totalBelumCount < 0 ? 0 : totalBelumCount,
                    DaftarTugas = tugasDtos
                };
            }

            // Query Data Absensi jika tab == "absensi"
            if (tab.Equals("absensi", StringComparison.OrdinalIgnoreCase))
            {
                var rawJadwal = _db.JadwalAbsen
                    .Where(j => j.IdKelas == id)
                    .OrderBy(j => j.PertemuanKe)
                    .ToList();

                var jadwalIds = rawJadwal.Select(j => j.IdJadwal).ToList();

                var absensiRecords = _db.Absensi
                    .Where(a => a.IdSiswa == currentUserId && jadwalIds.Contains(a.IdJadwal))
                    .ToList();

                DateTime today = DateTime.Today;
                TimeSpan nowTime = DateTime.Now.TimeOfDay;

                var sesiDtos = rawJadwal.Select(j =>
                {
                    var record = absensiRecords.FirstOrDefault(a => a.IdJadwal == j.IdJadwal);
                    bool isToday = j.Tanggal.Date == today;
                    bool isWithinTime = isToday && nowTime >= j.JamMulai && nowTime <= j.JamSelesai;

                    string statusSesiStr = "Tutup";
                    if (j.IsOpen || isWithinTime)
                    {
                        statusSesiStr = "Buka";
                    }
                    else if (j.Tanggal.Date > today || (isToday && nowTime < j.JamMulai))
                    {
                        statusSesiStr = "Mendatang";
                    }

                    string statusHadirStr = "Belum Presensi";
                    if (record != null && !string.IsNullOrEmpty(record.Status))
                    {
                        statusHadirStr = record.Status;
                    }
                    else if (statusSesiStr == "Tutup")
                    {
                        statusHadirStr = "Alpa";
                    }

                    bool canSubmit = statusSesiStr == "Buka" && (record == null || !record.Status.Equals("Hadir", StringComparison.OrdinalIgnoreCase));

                    return new SiswaAbsensiItemDto
                    {
                        IdJadwal = j.IdJadwal,
                        PertemuanKe = j.PertemuanKe,
                        JudulPertemuan = $"Pertemuan Ke-{j.PertemuanKe}",
                        Tanggal = j.Tanggal,
                        JamMulai = j.JamMulai,
                        JamSelesai = j.JamSelesai,
                        TokenPresensi = j.TokenPresensi,
                        IsOpen = j.IsOpen,
                        StatusSesi = statusSesiStr,
                        StatusKehadiranSiswa = statusHadirStr,
                        WaktuPresensi = record != null ? record.WaktuAbsen : null,
                        CanSubmitPresensi = canSubmit,
                        IsRequiresToken = !string.IsNullOrEmpty(j.TokenPresensi)
                    };
                }).ToList();

                int totalSesiCount = rawJadwal.Count;
                int totalHadirCount = sesiDtos.Count(s => s.StatusKehadiranSiswa.Equals("Hadir", StringComparison.OrdinalIgnoreCase));
                int totalSakitCount = sesiDtos.Count(s => s.StatusKehadiranSiswa.Equals("Sakit", StringComparison.OrdinalIgnoreCase));
                int totalIzinCount = sesiDtos.Count(s => s.StatusKehadiranSiswa.Equals("Izin", StringComparison.OrdinalIgnoreCase));
                int totalAlpaCount = sesiDtos.Count(s => s.StatusKehadiranSiswa.Equals("Alpa", StringComparison.OrdinalIgnoreCase));

                decimal pctKehadiran = totalSesiCount > 0 ? ((decimal)totalHadirCount / totalSesiCount) * 100m : 100m;

                ViewBag.AbsensiViewModel = new SiswaAbsensiListViewModel
                {
                    KelasId = kelas.IdKelas,
                    NamaKelas = kelas.NamaKelas,
                    KodeKelas = $"KLS-{kelas.IdKelas:D3}",
                    Kategori = kelas.Kategori != null ? kelas.Kategori.NamaKategori : "Umum",
                    NamaGuru = kelas.Guru != null ? kelas.Guru.NamaLengkap : "Guru",
                    FotoGuru = kelas.Guru != null ? kelas.Guru.FotoProfile : null,
                    TotalHadir = totalHadirCount,
                    TotalSakit = totalSakitCount,
                    TotalIzin = totalIzinCount,
                    TotalAlpa = totalAlpaCount,
                    TotalSesi = totalSesiCount,
                    PersentaseKehadiran = Math.Round(pctKehadiran, 1),
                    DaftarSesiAbsensi = sesiDtos
                };
            }

            // Query Data Quiz jika tab == "quiz"
            if (tab.Equals("quiz", StringComparison.OrdinalIgnoreCase))
            {
                var rawQuiz = _db.Quiz
                    .Where(q => q.IdKelas == id && q.IsActive)
                    .OrderBy(q => q.PertemuanKe)
                    .ToList();

                var quizIds = rawQuiz.Select(q => q.IdQuiz).ToList();

                var attempts = _db.NilaiQuiz
                    .Where(n => n.IdSiswa == currentUserId && quizIds.Contains(n.IdQuiz))
                    .ToList();

                var soalCounts = _db.SoalQuiz
                    .Where(s => quizIds.Contains(s.IdQuiz))
                    .GroupBy(s => s.IdQuiz)
                    .Select(g => new { IdQuiz = g.Key, Count = g.Count() })
                    .ToDictionary(g => g.IdQuiz, g => g.Count);

                var quizDtos = rawQuiz.Select(q =>
                {
                    var att = attempts.FirstOrDefault(a => a.IdQuiz == q.IdQuiz);
                    int countSoal = soalCounts.ContainsKey(q.IdQuiz) ? soalCounts[q.IdQuiz] : 0;
                    bool isSubmitted = att != null && att.WaktuSubmit.HasValue;

                    string statusStr = "Belum Dikerjakan";
                    if (isSubmitted)
                    {
                        statusStr = "Sudah Dikerjakan";
                    }
                    else if (q.WaktuSelesai.HasValue && DateTime.Now > q.WaktuSelesai.Value)
                    {
                        statusStr = "Waktu Habis";
                    }

                    return new SiswaQuizItemDto
                    {
                        QuizId = q.IdQuiz,
                        PertemuanKe = q.PertemuanKe,
                        JudulQuiz = q.JudulQuiz,
                        Deskripsi = q.Deskripsi,
                        DurasiMenit = q.Durasi ?? 30,
                        PassingScore = q.PassingScore,
                        TotalSoal = countSoal,
                        StatusPengerjaan = statusStr,
                        Nilai = att != null ? att.Score : null,
                        WaktuSubmit = att != null ? att.WaktuSubmit : null,
                        AttemptId = att != null ? (int?)att.IdAttempt : null,
                        CanAttempt = !isSubmitted && statusStr != "Waktu Habis"
                    };
                }).ToList();

                int totalQuizCount = rawQuiz.Count;
                int totalDoneCount = quizDtos.Count(q => q.WaktuSubmit.HasValue);
                int totalPendingCount = totalQuizCount - totalDoneCount;

                ViewBag.QuizViewModel = new SiswaQuizListViewModel
                {
                    KelasId = kelas.IdKelas,
                    NamaKelas = kelas.NamaKelas,
                    KodeKelas = $"KLS-{kelas.IdKelas:D3}",
                    Kategori = kelas.Kategori != null ? kelas.Kategori.NamaKategori : "Umum",
                    NamaGuru = kelas.Guru != null ? kelas.Guru.NamaLengkap : "Guru",
                    FotoGuru = kelas.Guru != null ? kelas.Guru.FotoProfile : null,
                    TotalQuiz = totalQuizCount,
                    TotalSudahDikerjakan = totalDoneCount,
                    TotalBelumDikerjakan = totalPendingCount < 0 ? 0 : totalPendingCount,
                    DaftarQuiz = quizDtos
                };
            }

            // Query Data Nilai Khusus Kelas jika tab == "nilai"
            if (tab.Equals("nilai", StringComparison.OrdinalIgnoreCase))
            {
                // 1. Absensi (15%)
                int totalJadwalAbsen = _db.JadwalAbsen.Count(j => j.IdKelas == id);
                var totalHadir = _db.Absensi.Count(a => a.IdSiswa == currentUserId && a.Status == "Hadir" && _db.JadwalAbsen.Any(j => j.IdJadwal == a.IdJadwal && j.IdKelas == id));
                decimal skorAbsen = totalJadwalAbsen > 0 ? ((decimal)totalHadir / totalJadwalAbsen) * 100m : 100m;
                decimal poinAbsen = Math.Round(skorAbsen * 0.15m, 2);

                // 2. Materi (15%)
                var materiList = _db.Materi.Where(m => m.IdKelas == id && !m.IsDeleted).ToList();
                int totalMateri = materiList.Count;
                var materiIds = materiList.Select(m => m.IdMateri).ToList();
                int totalMateriDibaca = _db.ProgresMateri.Count(p => p.IdSiswa == currentUserId && materiIds.Contains(p.IdMateri) && p.IsSelesai);
                decimal skorMateri = totalMateri > 0 ? ((decimal)totalMateriDibaca / totalMateri) * 100m : 100m;
                decimal poinMateri = Math.Round(skorMateri * 0.15m, 2);

                // 3. Quiz (40%)
                var quizList = _db.Quiz.Where(q => q.IdKelas == id && q.IsActive).OrderBy(q => q.PertemuanKe).ToList();
                var quizIds = quizList.Select(q => q.IdQuiz).ToList();
                var quizScores = _db.NilaiQuiz
                    .Where(n => n.IdSiswa == currentUserId && quizIds.Contains(n.IdQuiz))
                    .ToList();

                var itemQuizDtos = quizList.Select(q =>
                {
                    var att = quizScores.FirstOrDefault(n => n.IdQuiz == q.IdQuiz);
                    return new ItemNilaiQuizDto
                    {
                        JudulQuiz = q.JudulQuiz,
                        Tanggal = att != null ? att.WaktuSubmit : null,
                        SkorPerolehan = att != null ? att.Score : null
                    };
                }).ToList();

                var quizScoreValues = quizScores.Where(n => n.Score.HasValue).Select(n => n.Score.Value).ToList();
                decimal rataQuiz = quizScoreValues.Any() ? quizScoreValues.Average() : 0m;
                decimal poinQuiz = Math.Round(rataQuiz * 0.40m, 2);

                // 4. Tugas (30%)
                var tugasList = _db.Tugas.Where(t => t.IdKelas == id && !t.IsDeleted).OrderByDescending(t => t.CreatedAt).ToList();
                var tugasIds = tugasList.Select(t => t.IdTugas).ToList();
                var tugasSubmissions = _db.PengumpulanTugas
                    .Where(p => p.IdSiswa == currentUserId && tugasIds.Contains(p.IdTugas))
                    .ToList();

                var itemTugasDtos = tugasList.Select(t =>
                {
                    var sub = tugasSubmissions.FirstOrDefault(p => p.IdTugas == t.IdTugas);
                    string statusStr = sub != null ? (sub.Nilai.HasValue ? "Sudah Dinilai" : "Diserahkan") : (DateTime.Now > t.Deadline ? "Terlewat" : "Belum Mengumpulkan");
                    return new ItemNilaiTugasDto
                    {
                        JudulTugas = t.JudulTugas,
                        TanggalSubmisi = sub != null ? sub.WaktuKumpul : null,
                        Deadline = t.Deadline,
                        Status = statusStr,
                        NilaiTugas = sub != null ? sub.Nilai : null,
                        FeedbackGuru = sub != null ? sub.Feedback : null
                    };
                }).ToList();

                var tugasScoreValues = tugasSubmissions.Where(p => p.Nilai.HasValue).Select(p => p.Nilai.Value).ToList();
                decimal rataTugas = tugasScoreValues.Any() ? tugasScoreValues.Average() : 0m;
                decimal poinTugas = Math.Round(rataTugas * 0.30m, 2);

                // Hasil Akhir
                decimal nilaiAkhir = Math.Min(100m, Math.Round(poinAbsen + poinMateri + poinQuiz + poinTugas, 2));

                string predikat = "E";
                if (nilaiAkhir >= 85m) predikat = "A";
                else if (nilaiAkhir >= 75m) predikat = "B";
                else if (nilaiAkhir >= 65m) predikat = "C";
                else if (nilaiAkhir >= 50m) predikat = "D";

                string statusLulus = nilaiAkhir >= 70m ? "Lulus" : "Remedial";

                ViewBag.RincianNilaiViewModel = new SiswaRincianNilaiKelasViewModel
                {
                    KelasId = kelas.IdKelas,
                    NamaKelas = kelas.NamaKelas,
                    KodeKelas = $"KLS-{kelas.IdKelas:D3}",
                    NamaGuru = kelas.Guru != null ? kelas.Guru.NamaLengkap : "Guru",
                    FotoGuru = kelas.Guru != null ? kelas.Guru.FotoProfile : null,
                    Kategori = kelas.Kategori != null ? kelas.Kategori.NamaKategori : "Umum",
                    ThumbnailUrl = !string.IsNullOrEmpty(kelas.Thumbnail) ? kelas.Thumbnail : "/Content/images/default-course.jpg",

                    TotalSesi = totalJadwalAbsen,
                    TotalHadir = totalHadir,
                    SkorAbsensi = Math.Round(skorAbsen, 1),
                    PoinBobotAbsensi = poinAbsen,

                    TotalMateri = totalMateri,
                    TotalMateriSelesai = totalMateriDibaca,
                    SkorMateri = Math.Round(skorMateri, 1),
                    PoinBobotMateri = poinMateri,

                    DaftarNilaiQuiz = itemQuizDtos,
                    RataRataQuiz = Math.Round(rataQuiz, 1),
                    PoinBobotQuiz = poinQuiz,

                    DaftarNilaiTugas = itemTugasDtos,
                    RataRataTugas = Math.Round(rataTugas, 1),
                    PoinBobotTugas = poinTugas,

                    NilaiAkhirKumulatif = nilaiAkhir,
                    PredikatHuruf = predikat,
                    StatusKelulusan = statusLulus
                };
            }

            var viewModel = new GuruKelasStreamViewModel
            {
                IdKelas = kelas.IdKelas,
                NamaKelas = kelas.NamaKelas,
                KodeKelas = $"KLS-{kelas.IdKelas:D3}",
                Kategori = kelas.Kategori != null ? kelas.Kategori.NamaKategori : "Umum",
                Deskripsi = kelas.Deskripsi,
                Level = kelas.Level,
                Durasi = kelas.Durasi,
                ThumbnailUrl = !string.IsNullOrEmpty(kelas.Thumbnail) ? kelas.Thumbnail : "/Content/images/default-course.jpg",
                ActiveTab = tab,
                JumlahSiswa = _db.MemberKelas.Count(m => m.IdKelas == id && m.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)),
                JumlahMateri = _db.Materi.Count(m => m.IdKelas == id && !m.IsDeleted),
                JumlahTugas = _db.Tugas.Count(t => t.IdKelas == id && !t.IsDeleted),
                JumlahQuiz = _db.Quiz.Count(q => q.IdKelas == id && q.IsActive),
                DaftarPostingan = postDtos
            };

            ViewBag.Title = $"{kelas.NamaKelas} - Ruang Kelas Siswa";
            return View("RuangKelas", viewModel);
        }

        // GET: /Siswa/Tugas/{id}
        [HttpGet]
        public ActionResult Tugas(int id)
        {
            return RedirectToAction("RuangKelas", new { id = id, tab = "tugas" });
        }

        // POST: /Siswa/KumpulkanTugas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KumpulkanTugas(SubmitTugasViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                TempData["ErrorMessage"] = "Silakan login terlebih dahulu untuk mengumpulkan tugas.";
                return RedirectToAction("Login", "Account");
            }

            if (model.FileJawaban == null || model.FileJawaban.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Mohon sertakan file berkas jawaban Anda.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "tugas" });
            }

            var tugas = _db.Tugas.FirstOrDefault(t => t.IdTugas == model.TugasId && !t.IsDeleted);
            if (tugas == null)
            {
                TempData["ErrorMessage"] = "Data penugasan tidak ditemukan.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "tugas" });
            }

            // Process File Upload
            string fileName = System.IO.Path.GetFileName(model.FileJawaban.FileName);
            string fileExt = System.IO.Path.GetExtension(fileName).ToLower();

            var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".zip", ".rar", ".png", ".jpg", ".jpeg", ".xlsx", ".pptx" };
            if (!allowedExtensions.Contains(fileExt))
            {
                TempData["ErrorMessage"] = "Format file tidak didukung. Harap upload PDF, DOCX, ZIP, gambar, XLSX, atau PPTX.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "tugas" });
            }

            string targetFolder = Server.MapPath("~/Content/uploads/tugas_submissions/");
            if (!System.IO.Directory.Exists(targetFolder))
            {
                System.IO.Directory.CreateDirectory(targetFolder);
            }

            string uniqueFileName = $"submisi_{tugas.IdTugas}_{currentUserId}_{DateTime.Now:yyyyMMddHHmmss}{fileExt}";
            string physicalPath = System.IO.Path.Combine(targetFolder, uniqueFileName);
            model.FileJawaban.SaveAs(physicalPath);

            string fileRelativeUrl = "/Content/uploads/tugas_submissions/" + uniqueFileName;
            bool isLate = DateTime.Now > tugas.Deadline;

            var submission = _db.PengumpulanTugas.FirstOrDefault(s => s.IdTugas == model.TugasId && s.IdSiswa == currentUserId);
            if (submission == null)
            {
                submission = new PengumpulanTugas
                {
                    IdTugas = model.TugasId,
                    IdSiswa = currentUserId,
                    FilePath = fileRelativeUrl,
                    WaktuKumpul = DateTime.Now,
                    Status = isLate ? "Late" : "Submitted",
                    CreatedAt = DateTime.Now
                };
                _db.PengumpulanTugas.Add(submission);
            }
            else
            {
                submission.FilePath = fileRelativeUrl;
                submission.WaktuKumpul = DateTime.Now;
                submission.Status = isLate ? "Late" : "Submitted";
                submission.UpdatedAt = DateTime.Now;
            }

            _db.SaveChanges();

            TempData["SuccessMessage"] = isLate 
                ? "Jawaban tugas berhasil dikumpulkan! (Catatan: Pengumpulkan melewati batas deadline)." 
                : "Selamat! Jawaban tugas Anda telah berhasil diserahkan secara tepat waktu.";

            return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "tugas" });
        }

        // POST: /Siswa/BatalkanPengumpulan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BatalkanPengumpulan(int submissionId, int kelasId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                TempData["ErrorMessage"] = "Sesi Anda telah berakhir, silakan login ulang.";
                return RedirectToAction("Login", "Account");
            }

            var submission = _db.PengumpulanTugas.FirstOrDefault(s => s.IdKumpul == submissionId && s.IdSiswa == currentUserId);
            if (submission == null)
            {
                TempData["ErrorMessage"] = "Data pengumpulan tugas tidak ditemukan.";
                return RedirectToAction("RuangKelas", new { id = kelasId, tab = "tugas" });
            }

            if (submission.Nilai.HasValue)
            {
                TempData["ErrorMessage"] = "Pengumpulan tidak dapat dibatalkan karena sudah dinilai oleh guru pengajar.";
                return RedirectToAction("RuangKelas", new { id = kelasId, tab = "tugas" });
            }

            submission.WaktuKumpul = null;
            submission.Status = "Withdrawn";
            submission.UpdatedAt = DateTime.Now;

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Status pengumpulan tugas berhasil dibatalkan. Anda dapat mengunggah berkas jawaban baru.";
            return RedirectToAction("RuangKelas", new { id = kelasId, tab = "tugas" });
        }

        // GET: /Siswa/Materi/{id}
        [HttpGet]
        public ActionResult Materi(int id)
        {
            return RedirectToAction("RuangKelas", new { id = id, tab = "materi" });
        }

        // POST: /Siswa/TandaiSelesaiBaca
        [HttpPost]
        public JsonResult TandaiSelesaiBaca(int materiId, int kelasId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return Json(new { success = false, message = "Sesi telah berakhir, silakan login ulang." });
            }

            var member = _db.MemberKelas.FirstOrDefault(m => m.IdKelas == kelasId && m.IdSiswa == currentUserId && m.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
            if (member == null)
            {
                return Json(new { success = false, message = "Anda tidak terdaftar di kelas ini." });
            }

            var progres = _db.ProgresMateri.FirstOrDefault(p => p.IdMateri == materiId && p.IdSiswa == currentUserId);
            if (progres == null)
            {
                progres = new ProgresMateri
                {
                    IdMateri = materiId,
                    IdSiswa = currentUserId,
                    IsSelesai = true,
                    DiselesaikanPada = DateTime.Now
                };
                _db.ProgresMateri.Add(progres);
            }
            else
            {
                progres.IsSelesai = true;
                if (!progres.DiselesaikanPada.HasValue)
                {
                    progres.DiselesaikanPada = DateTime.Now;
                }
            }

            _db.SaveChanges();
            return Json(new { success = true, message = "Materi berhasil ditandai selesai dipelajari!" });
        }

        private static string GetTimeRemainingFormatted(DateTime deadline)
        {
            var ts = deadline - DateTime.Now;
            if (ts.TotalHours < 24)
            {
                return $"{ts.Hours} Jam {ts.Minutes} Menit Lagi";
            }
            return $"{ts.Days} Hari Lagi";
        }

        private static string FormatBytes(long? bytes)
        {
            if (!bytes.HasValue || bytes.Value <= 0) return "-";
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int i = 0;
            double dblSByte = bytes.Value;
            while (dblSByte >= 1024 && i < suffixes.Length - 1)
            {
                dblSByte /= 1024;
                i++;
            }
            return $"{dblSByte:0.##} {suffixes[i]}";
        }

        // GET: /Siswa/SemuaTugas
        [HttpGet]
        public ActionResult SemuaTugas(string status = "Semua")
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var memberKelasList = _db.MemberKelas
                .Include(m => m.Kelas)
                .Where(m => m.IdSiswa == currentUserId && m.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) && !m.Kelas.IsDeleted)
                .ToList();

            var kelasIds = memberKelasList.Select(m => m.IdKelas).ToList();

            var rawTugas = _db.Tugas
                .Include(t => t.Kelas)
                .Where(t => kelasIds.Contains(t.IdKelas) && !t.IsDeleted)
                .OrderBy(t => t.Deadline)
                .ToList();

            var tugasIds = rawTugas.Select(t => t.IdTugas).ToList();

            var submissions = _db.PengumpulanTugas
                .Where(s => s.IdSiswa == currentUserId && tugasIds.Contains(s.IdTugas))
                .ToList();

            var allItemDtos = rawTugas.Select(t =>
            {
                var sub = submissions.FirstOrDefault(s => s.IdTugas == t.IdTugas);
                bool overdue = DateTime.Now > t.Deadline;
                var ts = t.Deadline - DateTime.Now;

                string statusDeadlineStr = "Aktif";
                if (overdue)
                {
                    statusDeadlineStr = "Lewat";
                }
                else if (ts.TotalHours <= 24)
                {
                    statusDeadlineStr = "Mendekati";
                }

                string statusPengumpulanStr = "Belum Dikumpulkan";
                if (sub != null && sub.WaktuKumpul.HasValue)
                {
                    if (sub.Nilai.HasValue)
                    {
                        statusPengumpulanStr = "Sudah Dinilai";
                    }
                    else if (sub.WaktuKumpul > t.Deadline || sub.Status.Equals("Late", StringComparison.OrdinalIgnoreCase))
                    {
                        statusPengumpulanStr = "Terlambat";
                    }
                    else
                    {
                        statusPengumpulanStr = "Diserahkan";
                    }
                }
                else if (overdue)
                {
                    statusPengumpulanStr = "Terlewat";
                }

                return new GlobalTugasItemDto
                {
                    TugasId = t.IdTugas,
                    KelasId = t.IdKelas,
                    NamaKelas = t.Kelas != null ? t.Kelas.NamaKelas : "Kelas",
                    JudulTugas = t.JudulTugas,
                    Deadline = t.Deadline,
                    StatusDeadline = statusDeadlineStr,
                    StatusPengumpulan = statusPengumpulanStr,
                    Nilai = sub != null ? sub.Nilai : null,
                    WaktuKumpul = sub != null ? sub.WaktuKumpul : null,
                    IsOverdue = overdue,
                    TimeRemainingFormatted = overdue ? "Lewat Batas Waktu" : GetTimeRemainingFormatted(t.Deadline)
                };
            }).ToList();

            int totalSemua = allItemDtos.Count;
            int totalBelum = allItemDtos.Count(d => d.StatusPengumpulan == "Belum Dikumpulkan");
            int totalDinilai = allItemDtos.Count(d => d.StatusPengumpulan == "Sudah Dinilai");
            int totalTerlewat = allItemDtos.Count(d => d.IsOverdue && d.StatusPengumpulan.Contains("Belum") || d.StatusPengumpulan == "Terlewat");

            IEnumerable<GlobalTugasItemDto> filteredList = allItemDtos;

            if (!string.IsNullOrEmpty(status) && !status.Equals("Semua", StringComparison.OrdinalIgnoreCase))
            {
                if (status.Equals("BelumSelesai", StringComparison.OrdinalIgnoreCase))
                {
                    filteredList = filteredList.Where(d => d.StatusPengumpulan == "Belum Dikumpulkan");
                }
                else if (status.Equals("SudahDinilai", StringComparison.OrdinalIgnoreCase))
                {
                    filteredList = filteredList.Where(d => d.StatusPengumpulan == "Sudah Dinilai");
                }
                else if (status.Equals("Terlewat", StringComparison.OrdinalIgnoreCase))
                {
                    filteredList = filteredList.Where(d => d.IsOverdue && d.StatusPengumpulan.Contains("Belum") || d.StatusPengumpulan == "Terlewat");
                }
            }

            var viewModel = new SiswaGlobalTugasViewModel
            {
                FilterStatus = status,
                TotalSemua = totalSemua,
                TotalBelumSelesai = totalBelum,
                TotalSudahDinilai = totalDinilai,
                TotalTerlewat = totalTerlewat,
                DaftarTugas = filteredList.ToList()
            };

            ViewBag.Title = "Semua Penugasan Siswa";
            return View("SemuaTugas", viewModel);
        }

        // GET: /Siswa/SemuaMateri
        [HttpGet]
        public ActionResult SemuaMateri(string search = "", string kategori = "")
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var memberKelasList = _db.MemberKelas
                .Include(m => m.Kelas)
                .Include(m => m.Kelas.Kategori)
                .Where(m => m.IdSiswa == currentUserId && m.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) && !m.Kelas.IsDeleted)
                .ToList();

            var kelasIds = memberKelasList.Select(m => m.IdKelas).ToList();

            var rawMateri = _db.Materi
                .Include(m => m.Kelas)
                .Where(m => kelasIds.Contains(m.IdKelas) && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .ToList();

            var materiIds = rawMateri.Select(m => m.IdMateri).ToList();

            var progresList = _db.ProgresMateri
                .Where(p => p.IdSiswa == currentUserId && materiIds.Contains(p.IdMateri) && p.IsSelesai)
                .ToList();

            var categories = memberKelasList
                .Where(m => m.Kelas != null && m.Kelas.Kategori != null)
                .Select(m => m.Kelas.Kategori.NamaKategori)
                .Distinct()
                .ToList();

            var allItemDtos = rawMateri.Select(m =>
            {
                var prg = progresList.FirstOrDefault(p => p.IdMateri == m.IdMateri);
                return new GlobalMateriItemDto
                {
                    MateriId = m.IdMateri,
                    KelasId = m.IdKelas,
                    NamaKelas = m.Kelas != null ? m.Kelas.NamaKelas : "Kelas",
                    JudulMateri = m.JudulMateri,
                    TipeFile = m.TipeMateri,
                    FileUrl = m.FilePath,
                    NamaFile = !string.IsNullOrEmpty(m.NamaFile) ? m.NamaFile : (!string.IsNullOrEmpty(m.FilePath) ? System.IO.Path.GetFileName(m.FilePath) : "Materi"),
                    TanggalUpload = m.CreatedAt,
                    IsSudahDipelajari = prg != null,
                    TanggalDipelajari = prg != null ? prg.DiselesaikanPada : null
                };
            }).ToList();

            IEnumerable<GlobalMateriItemDto> filteredList = allItemDtos;

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim().ToLower();
                filteredList = filteredList.Where(m => m.JudulMateri.ToLower().Contains(kw) || m.NamaKelas.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(kategori))
            {
                var kelasIdsWithCat = memberKelasList
                    .Where(m => m.Kelas != null && m.Kelas.Kategori != null && m.Kelas.Kategori.NamaKategori.Equals(kategori, StringComparison.OrdinalIgnoreCase))
                    .Select(m => m.IdKelas)
                    .ToList();

                filteredList = filteredList.Where(m => kelasIdsWithCat.Contains(m.KelasId));
            }

            var listResult = filteredList.ToList();

            var viewModel = new SiswaGlobalMateriViewModel
            {
                FilterKategori = kategori,
                SearchKeyword = search,
                DaftarKategoriOptions = categories,
                TotalMateri = allItemDtos.Count,
                TotalSudahDipelajari = allItemDtos.Count(m => m.IsSudahDipelajari),
                DaftarMateri = listResult
            };

            ViewBag.Title = "Semua Materi Modul Siswa";
            return View("SemuaMateri", viewModel);
        }

        // GET: /Siswa/SemuaQuiz
        [HttpGet]
        public ActionResult SemuaQuiz()
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var memberKelasList = _db.MemberKelas
                .Include(m => m.Kelas)
                .Where(m => m.IdSiswa == currentUserId && m.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) && !m.Kelas.IsDeleted)
                .ToList();

            var kelasIds = memberKelasList.Select(m => m.IdKelas).ToList();

            var rawQuiz = _db.Quiz
                .Include(q => q.Kelas)
                .Where(q => kelasIds.Contains(q.IdKelas) && q.IsActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            var quizIds = rawQuiz.Select(q => q.IdQuiz).ToList();

            var nilaiQuizList = _db.NilaiQuiz
                .Where(n => n.IdSiswa == currentUserId && quizIds.Contains(n.IdQuiz))
                .ToList();

            var soalCounts = _db.SoalQuiz
                .Where(s => quizIds.Contains(s.IdQuiz))
                .GroupBy(s => s.IdQuiz)
                .Select(g => new { IdQuiz = g.Key, Count = g.Count() })
                .ToDictionary(g => g.IdQuiz, g => g.Count);

            var allItemDtos = rawQuiz.Select(q =>
            {
                var nq = nilaiQuizList.FirstOrDefault(n => n.IdQuiz == q.IdQuiz);
                int totalSoal = soalCounts.ContainsKey(q.IdQuiz) ? soalCounts[q.IdQuiz] : 0;

                string statusStr = "Belum Dikerjakan";
                if (nq != null && nq.WaktuSubmit.HasValue)
                {
                    statusStr = "Sudah Dikerjakan";
                }
                else if (q.WaktuSelesai.HasValue && DateTime.Now > q.WaktuSelesai.Value)
                {
                    statusStr = "Waktu Habis";
                }

                string deadlineStr = q.WaktuSelesai.HasValue ? q.WaktuSelesai.Value.ToString("dd MMM yyyy HH:mm WIB") : "Tidak Ada Batas";

                return new GlobalQuizItemDto
                {
                    QuizId = q.IdQuiz,
                    KelasId = q.IdKelas,
                    NamaKelas = q.Kelas != null ? q.Kelas.NamaKelas : "Kelas",
                    JudulQuiz = q.JudulQuiz,
                    DurasiMenit = q.Durasi ?? 30,
                    TotalSoal = totalSoal,
                    BatasWaktuMulai = q.WaktuMulai,
                    BatasWaktuSelesai = q.WaktuSelesai,
                    StatusPengerjaan = statusStr,
                    Skor = nq != null ? nq.Score : null,
                    WaktuSelesaiFormat = deadlineStr
                };
            }).ToList();

            var viewModel = new SiswaGlobalQuizViewModel
            {
                TotalKuis = allItemDtos.Count,
                TotalSudahDikerjakan = allItemDtos.Count(q => q.StatusPengerjaan == "Sudah Dikerjakan"),
                TotalBelumDikerjakan = allItemDtos.Count(q => q.StatusPengerjaan == "Belum Dikerjakan"),
                DaftarQuiz = allItemDtos
            };

            ViewBag.Title = "Semua Kuis Online Siswa";
            return View("SemuaQuiz", viewModel);
        }

        // GET: /Siswa/RekapNilai
        [HttpGet]
        public ActionResult RekapNilai()
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var memberKelasList = _db.MemberKelas
                .Include(m => m.Kelas)
                .Include(m => m.Kelas.Guru)
                .Include(m => m.Kelas.Kategori)
                .Where(m => m.IdSiswa == currentUserId && m.Status == "Active" && !m.Kelas.IsDeleted)
                .ToList();

            var cardDtos = new List<SiswaNilaiKelasCardDto>();
            int totalTugasDinilaiGlobal = 0;
            int totalQuizSelesaiGlobal = 0;

            foreach (var mk in memberKelasList)
            {
                var k = mk.Kelas;
                if (k == null) continue;

                // 1. Absensi (15%)
                int totalJadwalAbsen = _db.JadwalAbsen.Count(j => j.IdKelas == k.IdKelas);
                var totalHadir = _db.Absensi.Count(a => a.IdSiswa == currentUserId && a.Status == "Hadir" && _db.JadwalAbsen.Any(j => j.IdJadwal == a.IdJadwal && j.IdKelas == k.IdKelas));
                decimal skorAbsen = totalJadwalAbsen > 0 ? ((decimal)totalHadir / totalJadwalAbsen) * 100m : 100m;
                decimal poinAbsen = Math.Round(skorAbsen * 0.15m, 2);

                // 2. Materi (15%)
                var materiList = _db.Materi.Where(m => m.IdKelas == k.IdKelas && !m.IsDeleted).ToList();
                int totalMateri = materiList.Count;
                var materiIds = materiList.Select(m => m.IdMateri).ToList();
                int totalMateriDibaca = _db.ProgresMateri.Count(p => p.IdSiswa == currentUserId && materiIds.Contains(p.IdMateri) && p.IsSelesai);
                decimal skorMateri = totalMateri > 0 ? ((decimal)totalMateriDibaca / totalMateri) * 100m : 100m;
                decimal poinMateri = Math.Round(skorMateri * 0.15m, 2);

                // 3. Quiz (40%)
                var quizIds = _db.Quiz.Where(q => q.IdKelas == k.IdKelas && q.IsActive).Select(q => q.IdQuiz).ToList();
                var quizScores = _db.NilaiQuiz
                    .Where(n => n.IdSiswa == currentUserId && quizIds.Contains(n.IdQuiz) && n.Score.HasValue)
                    .Select(n => n.Score.Value)
                    .ToList();
                decimal rataQuiz = quizScores.Any() ? quizScores.Average() : 0m;
                decimal poinQuiz = Math.Round(rataQuiz * 0.40m, 2);
                int quizDoneCount = quizScores.Count;
                totalQuizSelesaiGlobal += quizDoneCount;

                // 4. Tugas (30%)
                var tugasIds = _db.Tugas.Where(t => t.IdKelas == k.IdKelas && !t.IsDeleted).Select(t => t.IdTugas).ToList();
                var tugasScores = _db.PengumpulanTugas
                    .Where(p => p.IdSiswa == currentUserId && tugasIds.Contains(p.IdTugas) && p.Nilai.HasValue)
                    .Select(p => p.Nilai.Value)
                    .ToList();
                decimal rataTugas = tugasScores.Any() ? tugasScores.Average() : 0m;
                decimal poinTugas = Math.Round(rataTugas * 0.30m, 2);
                int tugasGradedCount = tugasScores.Count;
                totalTugasDinilaiGlobal += tugasGradedCount;

                // Hasil Akhir
                decimal nilaiAkhir = Math.Min(100m, Math.Round(poinAbsen + poinMateri + poinQuiz + poinTugas, 2));

                string predikat = "E";
                if (nilaiAkhir >= 85m) predikat = "A";
                else if (nilaiAkhir >= 75m) predikat = "B";
                else if (nilaiAkhir >= 65m) predikat = "C";
                else if (nilaiAkhir >= 50m) predikat = "D";

                string statusLulus = nilaiAkhir >= 70m ? "Lulus" : "Belum Lulus";

                cardDtos.Add(new SiswaNilaiKelasCardDto
                {
                    KelasId = k.IdKelas,
                    NamaKelas = k.NamaKelas,
                    KodeKelas = $"KLS-{k.IdKelas:D3}",
                    NamaGuru = k.Guru != null ? k.Guru.NamaLengkap : "Guru",
                    FotoGuru = k.Guru != null ? k.Guru.FotoProfile : null,
                    Kategori = k.Kategori != null ? k.Kategori.NamaKategori : "Umum",
                    ThumbnailUrl = !string.IsNullOrEmpty(k.Thumbnail) ? k.Thumbnail : "/Content/images/default-course.jpg",

                    TotalKehadiran = totalHadir,
                    TotalSesiAbsen = totalJadwalAbsen,
                    SkorKehadiran = Math.Round(skorAbsen, 1),
                    NilaiBobotAbsen = poinAbsen,

                    TotalMateriDibaca = totalMateriDibaca,
                    TotalMateri = totalMateri,
                    SkorMateri = Math.Round(skorMateri, 1),
                    NilaiBobotMateri = poinMateri,

                    RataRataQuiz = Math.Round(rataQuiz, 1),
                    NilaiBobotQuiz = poinQuiz,
                    QuizDiselesaikan = quizDoneCount,

                    RataRataTugas = Math.Round(rataTugas, 1),
                    NilaiBobotTugas = poinTugas,
                    TugasDinilai = tugasGradedCount,

                    NilaiAkhirKelas = nilaiAkhir,
                    PredikatHuruf = predikat,
                    StatusKelulusan = statusLulus
                });
            }

            decimal ipkGlobal = cardDtos.Any() ? Math.Round(cardDtos.Average(c => c.NilaiAkhirKelas), 2) : 0m;

            var viewModel = new SiswaRekapNilaiGlobalViewModel
            {
                IPKKumulatif = ipkGlobal,
                TotalKelasDiikuti = cardDtos.Count,
                TotalTugasDinilai = totalTugasDinilaiGlobal,
                TotalQuizDiselesaikan = totalQuizSelesaiGlobal,
                DaftarNilaiKelas = cardDtos
            };

            ViewBag.Title = "Rekap Nilai Akumulasi Siswa";
            return View("RekapNilai", viewModel);
        }

        // GET: /Siswa/Absensi/{id}
        [HttpGet]
        public ActionResult Absensi(int id)
        {
            return RedirectToAction("RuangKelas", new { id = id, tab = "absensi" });
        }

        // POST: /Siswa/LakukanPresensi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LakukanPresensi(SubmitPresensiViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                TempData["ErrorMessage"] = "Silakan login terlebih dahulu untuk melakukan presensi.";
                return RedirectToAction("Login", "Account");
            }

            var jadwal = _db.JadwalAbsen.FirstOrDefault(j => j.IdJadwal == model.SesiAbsensiId && j.IdKelas == model.KelasId);
            if (jadwal == null)
            {
                TempData["ErrorMessage"] = "Jadwal presensi tidak ditemukan.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "absensi" });
            }

            DateTime today = DateTime.Today;
            TimeSpan nowTime = DateTime.Now.TimeOfDay;
            bool isToday = jadwal.Tanggal.Date == today;
            bool isWithinTime = isToday && nowTime >= jadwal.JamMulai && nowTime <= jadwal.JamSelesai;
            bool isOpen = jadwal.IsOpen || isWithinTime;

            if (!isOpen)
            {
                TempData["ErrorMessage"] = "Sesi presensi ini telah ditutup atau belum dimulai.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "absensi" });
            }

            // Validate token if teacher set token
            if (!string.IsNullOrEmpty(jadwal.TokenPresensi))
            {
                if (string.IsNullOrWhiteSpace(model.TokenDiinput) || !model.TokenDiinput.Trim().Equals(jadwal.TokenPresensi.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Kode token presensi yang Anda masukkan salah. Silakan tanyakan token resmi kepada guru.";
                    return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "absensi" });
                }
            }

            var record = _db.Absensi.FirstOrDefault(a => a.IdJadwal == model.SesiAbsensiId && a.IdSiswa == currentUserId);
            if (record == null)
            {
                record = new Absensi
                {
                    IdJadwal = model.SesiAbsensiId,
                    IdSiswa = currentUserId,
                    WaktuAbsen = DateTime.Now,
                    Status = "Hadir",
                    Catatan = "Presensi Mandiri Siswa"
                };
                _db.Absensi.Add(record);
            }
            else
            {
                record.WaktuAbsen = DateTime.Now;
                record.Status = "Hadir";
                record.Catatan = "Presensi Mandiri Siswa (Diperbarui)";
            }

            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Selamat! Presensi Hadir untuk Pertemuan Ke-{jadwal.PertemuanKe} telah berhasil dicatat.";
            return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "absensi" });
        }

        // GET: /Siswa/Quiz/{id}
        [HttpGet]
        public ActionResult Quiz(int id)
        {
            return RedirectToAction("RuangKelas", new { id = id, tab = "quiz" });
        }

        // GET: /Siswa/KerjakanQuiz/{quizId}
        [HttpGet]
        public ActionResult KerjakanQuiz(int quizId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                TempData["ErrorMessage"] = "Silakan login terlebih dahulu untuk mengerjakan kuis.";
                return RedirectToAction("Login", "Account");
            }

            var quiz = _db.Quiz.Include(q => q.Kelas).FirstOrDefault(q => q.IdQuiz == quizId && q.IsActive);
            if (quiz == null)
            {
                TempData["ErrorMessage"] = "Kuis tidak ditemukan atau telah dinonaktifkan oleh guru.";
                return RedirectToAction("Dashboard");
            }

            // Verify student enrollment
            bool isEnrolled = _db.MemberKelas.Any(m => m.IdKelas == quiz.IdKelas && m.IdSiswa == currentUserId && m.Status == "Active");
            if (!isEnrolled)
            {
                TempData["ErrorMessage"] = "Anda belum terdaftar pada kelas ini.";
                return RedirectToAction("Dashboard");
            }

            // Check existing attempt
            var attempt = _db.NilaiQuiz.FirstOrDefault(n => n.IdQuiz == quizId && n.IdSiswa == currentUserId);
            if (attempt != null && attempt.WaktuSubmit.HasValue)
            {
                TempData["ErrorMessage"] = "Anda sudah pernah menyelesaikan kuis ini.";
                return RedirectToAction("HasilQuiz", new { attemptId = attempt.IdAttempt });
            }

            if (attempt == null)
            {
                attempt = new NilaiQuiz
                {
                    IdQuiz = quizId,
                    IdSiswa = currentUserId,
                    WaktuMulai = DateTime.Now,
                    Status = "InProgress"
                };
                _db.NilaiQuiz.Add(attempt);
                _db.SaveChanges();
            }

            // Load questions & options (without revealing IsCorrect!)
            var rawSoal = _db.SoalQuiz.Where(s => s.IdQuiz == quizId).OrderBy(s => s.IdSoal).ToList();
            var soalIds = rawSoal.Select(s => s.IdSoal).ToList();

            var opsiList = _db.OpsiJawaban.Where(o => soalIds.Contains(o.IdSoal)).ToList();

            var soalDtos = new List<SiswaSoalQuizItemDto>();
            int no = 1;

            foreach (var s in rawSoal)
            {
                var opsis = opsiList.Where(o => o.IdSoal == s.IdSoal).OrderBy(o => o.IdOpsi).ToList();

                var dto = new SiswaSoalQuizItemDto
                {
                    SoalId = s.IdSoal,
                    NomorUrut = no,
                    Pertanyaan = s.Pertanyaan,
                    OpsiA_Id = opsis.Count > 0 ? opsis[0].IdOpsi : 0,
                    PilihanA = opsis.Count > 0 ? opsis[0].TeksOpsi : "",
                    OpsiB_Id = opsis.Count > 1 ? opsis[1].IdOpsi : 0,
                    PilihanB = opsis.Count > 1 ? opsis[1].TeksOpsi : "",
                    OpsiC_Id = opsis.Count > 2 ? opsis[2].IdOpsi : 0,
                    PilihanC = opsis.Count > 2 ? opsis[2].TeksOpsi : "",
                    OpsiD_Id = opsis.Count > 3 ? opsis[3].IdOpsi : 0,
                    PilihanD = opsis.Count > 3 ? opsis[3].TeksOpsi : ""
                };

                soalDtos.Add(dto);
                no++;
            }

            int durasiMenit = quiz.Durasi ?? 30;
            DateTime endTime = attempt.WaktuMulai.Value.AddMinutes(durasiMenit);
            int sisaDetik = (int)Math.Max(0, (endTime - DateTime.Now).TotalSeconds);

            var viewModel = new KerjakanQuizViewModel
            {
                QuizId = quiz.IdQuiz,
                KelasId = quiz.IdKelas,
                JudulQuiz = quiz.JudulQuiz,
                Deskripsi = quiz.Deskripsi,
                DurasiMenit = durasiMenit,
                SisaDetik = sisaDetik,
                AttemptId = attempt.IdAttempt,
                DaftarSoal = soalDtos
            };

            ViewBag.Title = $"{quiz.JudulQuiz} - Lembar Ujian Kuis";
            return View("KerjakanQuiz", viewModel);
        }

        // POST: /Siswa/SubmitQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitQuiz(SubmitJawabanQuizViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                TempData["ErrorMessage"] = "Silakan login terlebih dahulu.";
                return RedirectToAction("Login", "Account");
            }

            var attempt = _db.NilaiQuiz.FirstOrDefault(n => n.IdAttempt == model.AttemptId && n.IdSiswa == currentUserId);
            if (attempt == null)
            {
                TempData["ErrorMessage"] = "Data ujian kuis tidak ditemukan.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "quiz" });
            }

            var rawSoal = _db.SoalQuiz.Where(s => s.IdQuiz == model.QuizId).ToList();
            var soalIds = rawSoal.Select(s => s.IdSoal).ToList();
            var correctOpsis = _db.OpsiJawaban.Where(o => soalIds.Contains(o.IdSoal) && o.IsCorrect).ToList();

            int totalSoal = rawSoal.Count;
            int jumlahBenar = 0;

            if (model.DaftarJawaban != null)
            {
                foreach (var j in model.DaftarJawaban)
                {
                    var correctOpt = correctOpsis.FirstOrDefault(o => o.IdSoal == j.SoalId);
                    if (correctOpt != null && correctOpt.IdOpsi == j.OpsiDipilihId)
                    {
                        jumlahBenar++;
                    }
                }
            }

            decimal totalSkor = totalSoal > 0 ? Math.Round(((decimal)jumlahBenar / totalSoal) * 100m, 2) : 0m;

            attempt.Score = totalSkor;
            attempt.WaktuSubmit = DateTime.Now;
            attempt.Status = "Completed";

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Ujian Kuis berhasil diselesaikan & nilai otomatis dikalkulasikan!";
            return RedirectToAction("HasilQuiz", new { attemptId = attempt.IdAttempt });
        }

        // GET: /Siswa/HasilQuiz/{attemptId}
        [HttpGet]
        public ActionResult HasilQuiz(int attemptId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var attempt = _db.NilaiQuiz
                .Include(n => n.Quiz)
                .Include(n => n.Quiz.Kelas)
                .FirstOrDefault(n => n.IdAttempt == attemptId && n.IdSiswa == currentUserId);

            if (attempt == null)
            {
                TempData["ErrorMessage"] = "Data hasil kuis tidak ditemukan.";
                return RedirectToAction("Dashboard");
            }

            var quiz = attempt.Quiz;
            int totalSoal = _db.SoalQuiz.Count(s => s.IdQuiz == quiz.IdQuiz);
            decimal score = attempt.Score ?? 0m;

            int jumlahBenar = totalSoal > 0 ? (int)Math.Round((score / 100m) * totalSoal) : 0;
            int jumlahSalah = totalSoal - jumlahBenar;
            if (jumlahSalah < 0) jumlahSalah = 0;

            bool isLulus = score >= quiz.PassingScore;

            var viewModel = new HasilPengerjaanQuizViewModel
            {
                AttemptId = attempt.IdAttempt,
                QuizId = quiz.IdQuiz,
                KelasId = quiz.IdKelas,
                NamaKelas = quiz.Kelas != null ? quiz.Kelas.NamaKelas : "Kelas",
                JudulQuiz = quiz.JudulQuiz,
                TotalSoal = totalSoal,
                JumlahBenar = jumlahBenar,
                JumlahSalah = jumlahSalah,
                TotalSkor = score,
                PassingScore = quiz.PassingScore,
                StatusLulus = isLulus ? "Lulus" : "Belum Lulus",
                WaktuSubmit = attempt.WaktuSubmit
            };

            ViewBag.Title = $"Hasil Kuis: {quiz.JudulQuiz}";
            return View("HasilQuiz", viewModel);
        }

        // GET: /Siswa/NilaiKelas/{id}
        [HttpGet]
        public ActionResult NilaiKelas(int id)
        {
            return RedirectToAction("RuangKelas", new { id = id, tab = "nilai" });
        }

        // GET: /Siswa/CetakRaportKelas/{id}
        [HttpGet]
        public ActionResult CetakRaportKelas(int id)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            if (currentUserId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var kelas = _db.Kelas.Include(k => k.Guru).Include(k => k.Kategori).FirstOrDefault(k => k.IdKelas == id && !k.IsDeleted);
            if (kelas == null)
            {
                TempData["ErrorMessage"] = "Kelas tidak ditemukan.";
                return RedirectToAction("Dashboard");
            }

            // 1. Absensi (15%)
            int totalJadwalAbsen = _db.JadwalAbsen.Count(j => j.IdKelas == id);
            var totalHadir = _db.Absensi.Count(a => a.IdSiswa == currentUserId && a.Status == "Hadir" && _db.JadwalAbsen.Any(j => j.IdJadwal == a.IdJadwal && j.IdKelas == id));
            decimal skorAbsen = totalJadwalAbsen > 0 ? ((decimal)totalHadir / totalJadwalAbsen) * 100m : 100m;
            decimal poinAbsen = Math.Round(skorAbsen * 0.15m, 2);

            // 2. Materi (15%)
            var materiList = _db.Materi.Where(m => m.IdKelas == id && !m.IsDeleted).ToList();
            int totalMateri = materiList.Count;
            var materiIds = materiList.Select(m => m.IdMateri).ToList();
            int totalMateriDibaca = _db.ProgresMateri.Count(p => p.IdSiswa == currentUserId && materiIds.Contains(p.IdMateri) && p.IsSelesai);
            decimal skorMateri = totalMateri > 0 ? ((decimal)totalMateriDibaca / totalMateri) * 100m : 100m;
            decimal poinMateri = Math.Round(skorMateri * 0.15m, 2);

            // 3. Quiz (40%)
            var quizList = _db.Quiz.Where(q => q.IdKelas == id && q.IsActive).OrderBy(q => q.PertemuanKe).ToList();
            var quizIds = quizList.Select(q => q.IdQuiz).ToList();
            var quizScores = _db.NilaiQuiz
                .Where(n => n.IdSiswa == currentUserId && quizIds.Contains(n.IdQuiz))
                .ToList();

            var itemQuizDtos = quizList.Select(q =>
            {
                var att = quizScores.FirstOrDefault(n => n.IdQuiz == q.IdQuiz);
                return new ItemNilaiQuizDto
                {
                    JudulQuiz = q.JudulQuiz,
                    Tanggal = att != null ? att.WaktuSubmit : null,
                    SkorPerolehan = att != null ? att.Score : null
                };
            }).ToList();

            var quizScoreValues = quizScores.Where(n => n.Score.HasValue).Select(n => n.Score.Value).ToList();
            decimal rataQuiz = quizScoreValues.Any() ? quizScoreValues.Average() : 0m;
            decimal poinQuiz = Math.Round(rataQuiz * 0.40m, 2);

            // 4. Tugas (30%)
            var tugasList = _db.Tugas.Where(t => t.IdKelas == id && !t.IsDeleted).OrderByDescending(t => t.CreatedAt).ToList();
            var tugasIds = tugasList.Select(t => t.IdTugas).ToList();
            var tugasSubmissions = _db.PengumpulanTugas
                .Where(p => p.IdSiswa == currentUserId && tugasIds.Contains(p.IdTugas))
                .ToList();

            var itemTugasDtos = tugasList.Select(t =>
            {
                var sub = tugasSubmissions.FirstOrDefault(p => p.IdTugas == t.IdTugas);
                string statusStr = sub != null ? (sub.Nilai.HasValue ? "Sudah Dinilai" : "Diserahkan") : (DateTime.Now > t.Deadline ? "Terlewat" : "Belum Mengumpulkan");
                return new ItemNilaiTugasDto
                {
                    JudulTugas = t.JudulTugas,
                    TanggalSubmisi = sub != null ? sub.WaktuKumpul : null,
                    Deadline = t.Deadline,
                    Status = statusStr,
                    NilaiTugas = sub != null ? sub.Nilai : null,
                    FeedbackGuru = sub != null ? sub.Feedback : null
                };
            }).ToList();

            var tugasScoreValues = tugasSubmissions.Where(p => p.Nilai.HasValue).Select(p => p.Nilai.Value).ToList();
            decimal rataTugas = tugasScoreValues.Any() ? tugasScoreValues.Average() : 0m;
            decimal poinTugas = Math.Round(rataTugas * 0.30m, 2);

            // Hasil Akhir
            decimal nilaiAkhir = Math.Min(100m, Math.Round(poinAbsen + poinMateri + poinQuiz + poinTugas, 2));

            string predikat = "E";
            if (nilaiAkhir >= 85m) predikat = "A";
            else if (nilaiAkhir >= 75m) predikat = "B";
            else if (nilaiAkhir >= 65m) predikat = "C";
            else if (nilaiAkhir >= 50m) predikat = "D";

            string statusLulus = nilaiAkhir >= 70m ? "Lulus" : "Remedial";

            var user = _db.Users.FirstOrDefault(u => u.IdUser == currentUserId);
            ViewBag.NamaSiswa = user != null ? user.NamaLengkap : "Siswa";
            ViewBag.EmailSiswa = user != null ? user.Email : "";

            var viewModel = new SiswaRincianNilaiKelasViewModel
            {
                KelasId = kelas.IdKelas,
                NamaKelas = kelas.NamaKelas,
                KodeKelas = $"KLS-{kelas.IdKelas:D3}",
                NamaGuru = kelas.Guru != null ? kelas.Guru.NamaLengkap : "Guru",
                FotoGuru = kelas.Guru != null ? kelas.Guru.FotoProfile : null,
                Kategori = kelas.Kategori != null ? kelas.Kategori.NamaKategori : "Umum",
                ThumbnailUrl = !string.IsNullOrEmpty(kelas.Thumbnail) ? kelas.Thumbnail : "/Content/images/default-course.jpg",

                TotalSesi = totalJadwalAbsen,
                TotalHadir = totalHadir,
                SkorAbsensi = Math.Round(skorAbsen, 1),
                PoinBobotAbsensi = poinAbsen,

                TotalMateri = totalMateri,
                TotalMateriSelesai = totalMateriDibaca,
                SkorMateri = Math.Round(skorMateri, 1),
                PoinBobotMateri = poinMateri,

                DaftarNilaiQuiz = itemQuizDtos,
                RataRataQuiz = Math.Round(rataQuiz, 1),
                PoinBobotQuiz = poinQuiz,

                DaftarNilaiTugas = itemTugasDtos,
                RataRataTugas = Math.Round(rataTugas, 1),
                PoinBobotTugas = poinTugas,

                NilaiAkhirKumulatif = nilaiAkhir,
                PredikatHuruf = predikat,
                StatusKelulusan = statusLulus
            };

            ViewBag.Title = $"Raport Kelas: {kelas.NamaKelas}";
            return View("CetakRaportKelas", viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
