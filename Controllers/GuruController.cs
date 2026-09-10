using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Learning_Management_System.Helpers;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Controllers
{
    [AuthorizeRole("Guru")]
    public class GuruController : Controller
    {
        private readonly LmsDbContext _db = new LmsDbContext();

        // GET: /Guru/Dashboard
        [HttpGet]
        public ActionResult Dashboard()
        {
            ViewBag.Title = "Dashboard Ruang Pengajar";

            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            // 1. Ambil list ID kelas yang diampu guru ini (yang tidak terhapus)
            var guruKelasList = _db.Kelas
                .Include(k => k.Kategori)
                .Where(k => k.IdGuru == currentUserId && !k.IsDeleted)
                .ToList();

            var guruKelasIds = guruKelasList.Select(k => k.IdKelas).ToList();

            // 2. Aggregate Metrics
            int totalKelasDiampu = guruKelasList.Count;

            int totalSiswaTerdaftar = _db.MemberKelas
                .Where(mk => guruKelasIds.Contains(mk.IdKelas) && mk.Status == "Active")
                .Select(mk => mk.IdSiswa)
                .Distinct()
                .Count();

            int totalTugasPerluDikoreksi = _db.PengumpulanTugas
                .Where(pt => guruKelasIds.Contains(pt.Tugas.IdKelas) && !pt.Tugas.IsDeleted && pt.Nilai == null)
                .Count();

            int totalKuisAktif = _db.Quiz
                .Where(q => guruKelasIds.Contains(q.IdKelas) && q.IsActive)
                .Count();

            // 3. Mapping Daftar Kelas Guru
            var daftarKelasDto = new List<GuruDashboardKelasDto>();
            foreach (var k in guruKelasList)
            {
                int jumlahSiswa = _db.MemberKelas.Count(mk => mk.IdKelas == k.IdKelas && mk.Status == "Active");
                int jumlahMateri = _db.Materi.Count(m => m.IdKelas == k.IdKelas && !m.IsDeleted);
                int jumlahTugas = _db.Tugas.Count(t => t.IdKelas == k.IdKelas && !t.IsDeleted);

                daftarKelasDto.Add(new GuruDashboardKelasDto
                {
                    IdKelas = k.IdKelas,
                    NamaKelas = k.NamaKelas,
                    KodeKelas = $"KLS-{k.IdKelas:D3}",
                    Kategori = k.Kategori != null ? k.Kategori.NamaKategori : "Umum",
                    ThumbnailUrl = !string.IsNullOrEmpty(k.Thumbnail) ? k.Thumbnail : "/Content/images/default-course.jpg",
                    JumlahSiswa = jumlahSiswa,
                    JumlahMateri = jumlahMateri,
                    JumlahTugas = jumlahTugas,
                    CreatedAt = k.CreatedAt
                });
            }

            // 4. Activity Submisi Tugas Terbaru Siswa (Take 10)
            var aktivitasTerbaru = _db.PengumpulanTugas
                .Include(pt => pt.Siswa)
                .Include(pt => pt.Tugas)
                .Include(pt => pt.Tugas.Kelas)
                .Where(pt => guruKelasIds.Contains(pt.Tugas.IdKelas) && !pt.Tugas.IsDeleted)
                .OrderByDescending(pt => pt.WaktuKumpul ?? pt.CreatedAt)
                .Take(10)
                .ToList()
                .Select(pt => new GuruDashboardAktivitasDto
                {
                    IdSubmisi = pt.IdKumpul,
                    NamaSiswa = pt.Siswa != null ? pt.Siswa.NamaLengkap : "Siswa",
                    FotoSiswa = pt.Siswa != null ? pt.Siswa.FotoProfile : null,
                    NamaKelas = pt.Tugas != null && pt.Tugas.Kelas != null ? pt.Tugas.Kelas.NamaKelas : "-",
                    JudulTugas = pt.Tugas != null ? pt.Tugas.JudulTugas : "Tugas",
                    TanggalKumpul = pt.WaktuKumpul ?? pt.CreatedAt,
                    StatusNilai = pt.Nilai.HasValue ? $"Sudah Dinilai ({pt.Nilai.Value:0.##})" : "Belum Dinilai",
                    Nilai = pt.Nilai
                })
                .ToList();

            var viewModel = new GuruDashboardViewModel
            {
                TotalKelasDiampu = totalKelasDiampu,
                TotalSiswaTerdaftar = totalSiswaTerdaftar,
                TotalTugasPerluDikoreksi = totalTugasPerluDikoreksi,
                TotalKuisAktif = totalKuisAktif,
                DaftarKelas = daftarKelasDto,
                AktivitasTerbaruSiswa = aktivitasTerbaru
            };

            return View("Dashboard", viewModel);
        }

        // GET: /Guru
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // GET: /Guru/Kelas
        [HttpGet]
        public ActionResult Kelas()
        {
            ViewBag.Title = "Daftar Kelas Saya";

            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var guruKelasList = _db.Kelas
                .Include(k => k.Kategori)
                .Where(k => k.IdGuru == currentUserId && !k.IsDeleted)
                .OrderByDescending(k => k.CreatedAt)
                .ToList();

            var daftarKelasDto = new List<GuruDashboardKelasDto>();
            foreach (var k in guruKelasList)
            {
                int jumlahSiswa = _db.MemberKelas.Count(mk => mk.IdKelas == k.IdKelas && mk.Status == "Active");
                int jumlahMateri = _db.Materi.Count(m => m.IdKelas == k.IdKelas && !m.IsDeleted);
                int jumlahTugas = _db.Tugas.Count(t => t.IdKelas == k.IdKelas && !t.IsDeleted);

                daftarKelasDto.Add(new GuruDashboardKelasDto
                {
                    IdKelas = k.IdKelas,
                    NamaKelas = k.NamaKelas,
                    KodeKelas = $"KLS-{k.IdKelas:D3}",
                    Kategori = k.Kategori != null ? k.Kategori.NamaKategori : "Umum",
                    ThumbnailUrl = !string.IsNullOrEmpty(k.Thumbnail) ? k.Thumbnail : "/Content/images/default-course.jpg",
                    JumlahSiswa = jumlahSiswa,
                    JumlahMateri = jumlahMateri,
                    JumlahTugas = jumlahTugas,
                    CreatedAt = k.CreatedAt
                });
            }

            return View("Kelas", daftarKelasDto);
        }

        // GET: /Guru/BuatKelas
        [HttpGet]
        public ActionResult BuatKelas()
        {
            ViewBag.Title = "Buat Kelas Pembelajaran Baru";

            var viewModel = new GuruBuatKelasViewModel();
            PopulateBuatKelasDropdowns(viewModel);

            return View("BuatKelas", viewModel);
        }

        // POST: /Guru/BuatKelas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuatKelas(GuruBuatKelasViewModel model)
        {
            ViewBag.Title = "Buat Kelas Pembelajaran Baru";

            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            if (!ModelState.IsValid)
            {
                PopulateBuatKelasDropdowns(model);
                return View("BuatKelas", model);
            }

            // Validasi kategori
            var kategori = _db.Kategori.FirstOrDefault(k => k.IdKategori == model.IdKategori);
            if (kategori == null)
            {
                ModelState.AddModelError("IdKategori", "Kategori yang dipilih tidak valid.");
                PopulateBuatKelasDropdowns(model);
                return View("BuatKelas", model);
            }

            // Tangani upload thumbnail gambar kelas
            string thumbnailPath = "/Content/images/default-course.jpg";
            if (model.ThumbnailUpload != null && model.ThumbnailUpload.ContentLength > 0)
            {
                try
                {
                    string ext = System.IO.Path.GetExtension(model.ThumbnailUpload.FileName).ToLower();
                    string[] allowedExts = { ".jpg", ".jpeg", ".png", ".webp" };

                    if (allowedExts.Contains(ext))
                    {
                        string fileName = $"kelas_{currentUserId}_{Guid.NewGuid():N}{ext}";
                        string folderPath = Server.MapPath("~/Content/uploads/thumbnails/");

                        if (!System.IO.Directory.Exists(folderPath))
                        {
                            System.IO.Directory.CreateDirectory(folderPath);
                        }

                        string savePath = System.IO.Path.Combine(folderPath, fileName);
                        model.ThumbnailUpload.SaveAs(savePath);

                        thumbnailPath = $"/Content/uploads/thumbnails/{fileName}";
                    }
                    else
                    {
                        ModelState.AddModelError("ThumbnailUpload", "Format file thumbnail harus .jpg, .jpeg, .png, atau .webp.");
                        PopulateBuatKelasDropdowns(model);
                        return View("BuatKelas", model);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Gagal mengunggah gambar cover kelas: " + ex.Message);
                    PopulateBuatKelasDropdowns(model);
                    return View("BuatKelas", model);
                }
            }

            // Buat entitas Kelas baru
            var newKelas = new Learning_Management_System.Models.Entity.Kelas
            {
                IdKategori = model.IdKategori,
                IdGuru = currentUserId,
                NamaKelas = model.NamaKelas.Trim(),
                Deskripsi = model.Deskripsi != null ? model.Deskripsi.Trim() : null,
                Thumbnail = thumbnailPath,
                Level = !string.IsNullOrEmpty(model.Level) ? model.Level : "Pemula",
                Durasi = model.Durasi,
                IsPublish = true,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            _db.Kelas.Add(newKelas);
            _db.SaveChanges();

            // Catat Notifikasi Audit Log
            try
            {
                var notif = new Learning_Management_System.Models.Entity.Notifikasi
                {
                    IdUser = currentUserId,
                    Judul = "Kelas Baru Dibuat",
                    Pesan = $"Selamat! Kelas '{newKelas.NamaKelas}' (Kode: KLS-{newKelas.IdKelas:D3}) telah berhasil dibuat dan aktif.",
                    TipeNotif = "Kelas",
                    UrlTujuan = $"/Guru/DetailKelas/{newKelas.IdKelas}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _db.Notifikasi.Add(notif);
                _db.SaveChanges();
            }
            catch
            {
                // Silence notification error if any
            }

            TempData["SuccessMessage"] = $"Kelas '{newKelas.NamaKelas}' berhasil dibuat!";
            TempData["Success"] = $"Kelas '{newKelas.NamaKelas}' berhasil dibuat!";

            return RedirectToAction("Dashboard");
        }

        private void PopulateBuatKelasDropdowns(GuruBuatKelasViewModel model)
        {
            var kategoriList = _db.Kategori
                .OrderBy(k => k.NamaKategori)
                .ToList()
                .Select(k => new SelectListItem
                {
                    Value = k.IdKategori.ToString(),
                    Text = k.NamaKategori
                }).ToList();

            model.KategoriOptions = kategoriList;

            model.LevelOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pemula", Text = "Pemula (Beginner)" },
                new SelectListItem { Value = "Menengah", Text = "Menengah (Intermediate)" },
                new SelectListItem { Value = "Lanjutan", Text = "Lanjutan (Advanced)" }
            };
        }

        // GET: /Guru/RuangKelas/{id} or /Guru/DetailKelas/{id}
        [HttpGet]
        public ActionResult RuangKelas(int id, string tab = "stream")
        {
            return DetailKelas(id, tab);
        }

        [HttpGet]
        public ActionResult DetailKelas(int id, string tab = "stream")
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            // Validasi kepemilikan kelas
            var kelas = _db.Kelas
                .Include(k => k.Kategori)
                .FirstOrDefault(k => k.IdKelas == id && k.IdGuru == currentUserId && !k.IsDeleted);

            if (kelas == null)
            {
                TempData["ErrorMessage"] = "Ruang kelas tidak ditemukan atau Anda tidak memiliki akses ke kelas ini.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Title = $"Ruang Kelas: {kelas.NamaKelas}";

            // Hitung statistik kelas
            int jumlahSiswa = _db.MemberKelas.Count(mk => mk.IdKelas == id && mk.Status == "Active");
            int jumlahMateri = _db.Materi.Count(m => m.IdKelas == id && !m.IsDeleted);
            int jumlahTugas = _db.Tugas.Count(t => t.IdKelas == id && !t.IsDeleted);
            int jumlahQuiz = _db.Quiz.Count(q => q.IdKelas == id);

            // Ambil daftar postingan stream
            var posts = _db.StreamPostingan
                .Include(p => p.User)
                .Include(p => p.User.Role)
                .Where(p => p.IdKelas == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var postIds = posts.Select(p => p.IdStream).ToList();

            // Ambil daftar komentar stream
            var comments = _db.KomentarStream
                .Include(c => c.User)
                .Include(c => c.User.Role)
                .Where(c => postIds.Contains(c.IdStream))
                .OrderBy(c => c.CreatedAt)
                .ToList();

            var daftarPostDto = posts.Select(p => new GuruStreamPostDto
            {
                IdPost = p.IdStream,
                IdUser = p.IdUser,
                NamaPenulis = p.User != null ? p.User.NamaLengkap : "Pengajar",
                FotoPenulis = p.User != null ? p.User.FotoProfile : null,
                RolePenulis = p.User != null && p.User.Role != null ? p.User.Role.NamaRole : "Guru",
                Konten = p.Pesan,
                FileAttachment = p.AttachmentUrl,
                NamaFile = !string.IsNullOrEmpty(p.AttachmentUrl) ? System.IO.Path.GetFileName(p.AttachmentUrl) : null,
                CreatedAt = p.CreatedAt,
                IsAuthor = p.IdUser == currentUserId,
                ListKomentar = comments
                    .Where(c => c.IdStream == p.IdStream)
                    .Select(c => new GuruStreamCommentDto
                    {
                        IdComment = c.IdKomentar,
                        IdPost = c.IdStream,
                        IdUser = c.IdUser,
                        NamaPenulis = c.User != null ? c.User.NamaLengkap : "Pengguna",
                        FotoPenulis = c.User != null ? c.User.FotoProfile : null,
                        RolePenulis = c.User != null && c.User.Role != null ? c.User.Role.NamaRole : "Pengguna",
                        Konten = c.Komentar,
                        CreatedAt = c.CreatedAt,
                        IsAuthor = c.IdUser == currentUserId
                    }).ToList()
            }).ToList();

            // Ambil daftar materi kelas
            var rawMateri = _db.Materi
                .Where(m => m.IdKelas == id && !m.IsDeleted)
                .OrderBy(m => m.PertemuanKe)
                .ThenByDescending(m => m.CreatedAt)
                .ToList();

            ViewBag.DaftarMateri = rawMateri.Select(m => new GuruMateriItemDto
            {
                IdMateri = m.IdMateri,
                PertemuanKe = m.PertemuanKe,
                JudulMateri = m.JudulMateri,
                Deskripsi = m.Deskripsi,
                NamaFile = m.NamaFile,
                FilePath = m.FilePath,
                TipeMateri = m.TipeMateri,
                FileSize = m.FileSize,
                FileSizeFormatted = FormatBytes(m.FileSize),
                CreatedAt = m.CreatedAt
            }).ToList();

            // Ambil daftar tugas kelas
            var rawTugas = _db.Tugas
                .Where(t => t.IdKelas == id && !t.IsDeleted)
                .OrderBy(t => t.PertemuanKe)
                .ThenByDescending(t => t.CreatedAt)
                .ToList();

            var tugasIds = rawTugas.Select(t => t.IdTugas).ToList();
            var submissions = _db.PengumpulanTugas
                .Where(pt => tugasIds.Contains(pt.IdTugas))
                .ToList();

            int totalSiswaKelas = _db.MemberKelas.Count(mk => mk.IdKelas == id && mk.Status == "Active");

            ViewBag.DaftarTugas = rawTugas.Select(t => new GuruTugasItemDto
            {
                IdTugas = t.IdTugas,
                PertemuanKe = t.PertemuanKe,
                JudulTugas = t.JudulTugas,
                Instruksi = t.Deskripsi,
                Deadline = t.Deadline,
                FileLampiranUrl = t.FileAttachment,
                NamaFileLampiran = !string.IsNullOrEmpty(t.FileAttachment) ? System.IO.Path.GetFileName(t.FileAttachment) : null,
                TotalSiswa = totalSiswaKelas,
                JumlahMengumpulkan = submissions.Count(s => s.IdTugas == t.IdTugas && s.WaktuKumpul.HasValue),
                JumlahDinilai = submissions.Count(s => s.IdTugas == t.IdTugas && s.Nilai.HasValue),
                IsOverdue = DateTime.Now > t.Deadline,
                CreatedAt = t.CreatedAt
            }).ToList();

            // Ambil daftar quiz kelas
            var rawQuiz = _db.Quiz
                .Where(q => q.IdKelas == id)
                .OrderBy(q => q.PertemuanKe)
                .ThenByDescending(q => q.CreatedAt)
                .ToList();

            var quizIds = rawQuiz.Select(q => q.IdQuiz).ToList();
            var soalCounts = _db.SoalQuiz
                .Where(s => quizIds.Contains(s.IdQuiz))
                .GroupBy(s => s.IdQuiz)
                .ToDictionary(g => g.Key, g => g.Count());

            var attemptCounts = _db.NilaiQuiz
                .Where(n => quizIds.Contains(n.IdQuiz))
                .GroupBy(n => n.IdQuiz)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.DaftarQuiz = rawQuiz.Select(q => new GuruQuizItemDto
            {
                IdQuiz = q.IdQuiz,
                PertemuanKe = q.PertemuanKe,
                JudulQuiz = q.JudulQuiz,
                Deskripsi = q.Deskripsi,
                DurasiMenit = q.Durasi,
                PassingScore = q.PassingScore,
                TotalSoal = soalCounts.ContainsKey(q.IdQuiz) ? soalCounts[q.IdQuiz] : 0,
                TotalPesertaMengerjakan = attemptCounts.ContainsKey(q.IdQuiz) ? attemptCounts[q.IdQuiz] : 0,
                StatusAktif = q.IsActive,
                WaktuMulai = q.WaktuMulai,
                WaktuSelesai = q.WaktuSelesai,
                CreatedAt = q.CreatedAt
            }).ToList();

            // Ambil daftar sesi absensi kelas
            var rawSesi = _db.JadwalAbsen
                .Where(j => j.IdKelas == id)
                .OrderBy(j => j.PertemuanKe)
                .ThenByDescending(j => j.Tanggal)
                .ToList();

            var sesiIds = rawSesi.Select(j => j.IdJadwal).ToList();
            var absensiRecords = _db.Absensi
                .Where(a => sesiIds.Contains(a.IdJadwal) && a.Status == "Hadir")
                .GroupBy(a => a.IdJadwal)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.DaftarSesiAbsensi = rawSesi.Select(j => new SesiAbsensiItemDto
            {
                SesiAbsensiId = j.IdJadwal,
                PertemuanKe = j.PertemuanKe,
                Tanggal = j.Tanggal,
                JamMulai = j.JamMulai,
                JamSelesai = j.JamSelesai,
                TokenPresensi = j.TokenPresensi,
                IsOpen = j.IsOpen,
                StatusSesi = j.IsOpen ? "Open" : "Closed",
                TotalHadir = absensiRecords.ContainsKey(j.IdJadwal) ? absensiRecords[j.IdJadwal] : 0,
                TotalSiswa = jumlahSiswa,
                CreatedAt = j.CreatedAt
            }).ToList();

            // Ambil data kalkulasi Gradebook / Buku Nilai
            ViewBag.Gradebook = GetGradebookViewModel(id, currentUserId);
            ViewBag.KelasId = id;

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
                JumlahSiswa = jumlahSiswa,
                JumlahMateri = jumlahMateri,
                JumlahTugas = jumlahTugas,
                JumlahQuiz = jumlahQuiz,
                DaftarPostingan = daftarPostDto
            };

            return View("RuangKelas", viewModel);
        }

        // POST: /Guru/CreatePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(CreatePostViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            // Validasi kepemilikan kelas
            bool isOwner = _db.Kelas.Any(k => k.IdKelas == model.KelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            if (string.IsNullOrWhiteSpace(model.KontenTeks))
            {
                TempData["ErrorMessage"] = "Pesan pengumuman atau postingan tidak boleh kosong.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId });
            }

            string attachmentUrl = null;
            if (model.LampiranFile != null && model.LampiranFile.ContentLength > 0)
            {
                try
                {
                    string ext = System.IO.Path.GetExtension(model.LampiranFile.FileName);
                    string fileName = $"stream_{model.KelasId}_{Guid.NewGuid():N}{ext}";
                    string folderPath = Server.MapPath("~/Content/uploads/stream/");

                    if (!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }

                    string savePath = System.IO.Path.Combine(folderPath, fileName);
                    model.LampiranFile.SaveAs(savePath);

                    attachmentUrl = $"/Content/uploads/stream/{fileName}";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Gagal mengunggah berkas lampiran: " + ex.Message;
                    return RedirectToAction("RuangKelas", new { id = model.KelasId });
                }
            }

            var newPost = new Learning_Management_System.Models.Entity.StreamPostingan
            {
                IdKelas = model.KelasId,
                IdUser = currentUserId,
                Pesan = model.KontenTeks.Trim(),
                AttachmentUrl = attachmentUrl,
                CreatedAt = DateTime.Now
            };

            _db.StreamPostingan.Add(newPost);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Pengumuman/postingan berhasil diterbitkan!";
            return RedirectToAction("RuangKelas", new { id = model.KelasId });
        }

        // POST: /Guru/DeletePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(int postId, int kelasId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var post = _db.StreamPostingan.FirstOrDefault(p => p.IdStream == postId && p.IdKelas == kelasId);
            if (post != null)
            {
                // Cek otorisasi
                bool isOwner = _db.Kelas.Any(k => k.IdKelas == kelasId && k.IdGuru == currentUserId && !k.IsDeleted);
                if (post.IdUser == currentUserId || isOwner)
                {
                    // Hapus komentar terkait
                    var relatedComments = _db.KomentarStream.Where(c => c.IdStream == postId).ToList();
                    if (relatedComments.Any())
                    {
                        _db.KomentarStream.RemoveRange(relatedComments);
                    }

                    _db.StreamPostingan.Remove(post);
                    _db.SaveChanges();

                    TempData["SuccessMessage"] = "Postingan berhasil dihapus.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Anda tidak memiliki wewenang untuk menghapus postingan ini.";
                }
            }

            return RedirectToAction("RuangKelas", new { id = kelasId });
        }

        // POST: /Guru/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(CreateCommentViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            if (string.IsNullOrWhiteSpace(model.KontenKomentar))
            {
                TempData["ErrorMessage"] = "Komentar tidak boleh kosong.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId });
            }

            var post = _db.StreamPostingan.FirstOrDefault(p => p.IdStream == model.PostId && p.IdKelas == model.KelasId);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Postingan tidak ditemukan.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId });
            }

            var newComment = new Learning_Management_System.Models.Entity.KomentarStream
            {
                IdStream = model.PostId,
                IdUser = currentUserId,
                Komentar = model.KontenKomentar.Trim(),
                CreatedAt = DateTime.Now
            };

            _db.KomentarStream.Add(newComment);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Komentar berhasil ditambahkan.";
            return RedirectToAction("RuangKelas", new { id = model.KelasId });
        }

        // POST: /Guru/UploadMateri
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadMateri(CreateMateriViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            // Validasi kepemilikan kelas
            bool isOwner = _db.Kelas.Any(k => k.IdKelas == model.KelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Mohon lengkapi formulir unggah materi dengan benar.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "materi" });
            }

            string filePath = null;
            string originalFileName = null;
            string tipeMateri = "DOC";
            long? fileSize = null;

            // Handle file upload
            if (model.FileUpload != null && model.FileUpload.ContentLength > 0)
            {
                try
                {
                    originalFileName = System.IO.Path.GetFileName(model.FileUpload.FileName);
                    string ext = System.IO.Path.GetExtension(originalFileName).ToLower();
                    string[] allowedExts = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".mp4", ".zip" };

                    if (!allowedExts.Contains(ext))
                    {
                        TempData["ErrorMessage"] = "Format file materi harus berupa PDF, DOCX, PPTX, XLSX, MP4, atau ZIP.";
                        return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "materi" });
                    }

                    if (ext == ".pdf") tipeMateri = "PDF";
                    else if (ext == ".mp4") tipeMateri = "VIDEO";
                    else if (ext == ".ppt" || ext == ".pptx") tipeMateri = "PPTX";
                    else if (ext == ".zip") tipeMateri = "ZIP";
                    else tipeMateri = "DOCX";

                    string fileName = $"materi_{model.KelasId}_p{model.PertemuanKe}_{Guid.NewGuid():N}{ext}";
                    string folderPath = Server.MapPath("~/Content/uploads/materi/");

                    if (!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }

                    string savePath = System.IO.Path.Combine(folderPath, fileName);
                    model.FileUpload.SaveAs(savePath);

                    filePath = $"/Content/uploads/materi/{fileName}";
                    fileSize = model.FileUpload.ContentLength;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Gagal mengunggah berkas materi: " + ex.Message;
                    return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "materi" });
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.VideoUrlOpsional))
            {
                filePath = model.VideoUrlOpsional.Trim();
                originalFileName = "Tautan Video Pembelajaran";
                tipeMateri = "LINK";
            }
            else
            {
                TempData["ErrorMessage"] = "Wajib mengunggah berkas file materi atau memasukkan tautan video.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "materi" });
            }

            var newMateri = new Learning_Management_System.Models.Entity.Materi
            {
                IdKelas = model.KelasId,
                PertemuanKe = model.PertemuanKe,
                JudulMateri = model.JudulMateri.Trim(),
                Deskripsi = model.Deskripsi != null ? model.Deskripsi.Trim() : null,
                NamaFile = originalFileName,
                FilePath = filePath,
                TipeMateri = tipeMateri,
                FileSize = fileSize,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            _db.Materi.Add(newMateri);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Modul materi '{newMateri.JudulMateri}' berhasil ditambahkan ke kelas!";
            return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "materi" });
        }

        // POST: /Guru/DeleteMateri
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMateri(int id, int kelasId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            bool isOwner = _db.Kelas.Any(k => k.IdKelas == kelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            var materi = _db.Materi.FirstOrDefault(m => m.IdMateri == id && m.IdKelas == kelasId && !m.IsDeleted);
            if (materi != null)
            {
                materi.IsDeleted = true;
                materi.DeletedAt = DateTime.Now;
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Modul materi berhasil dihapus.";
            }

            return RedirectToAction("RuangKelas", new { id = kelasId, tab = "materi" });
        }

        // GET: /Guru/Tugas/{id?}
        [HttpGet]
        public ActionResult Tugas(int? id = null)
        {
            if (id.HasValue && id.Value > 0)
            {
                return RedirectToAction("RuangKelas", new { id = id.Value, tab = "tugas" });
            }
            return RedirectToAction("KoreksiTugas");
        }

        // GET: /Guru/KoreksiTugas
        [HttpGet]
        public ActionResult KoreksiTugas(int? id = null, int? kelasId = null, string status = "PerluDikoreksi", string q = null)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            // Jika id diberikan (misal dari dashboard klik 'Koreksi' dengan submisiId atau tugasId)
            if (id.HasValue && id.Value > 0)
            {
                var sub = _db.PengumpulanTugas.Include(pt => pt.Tugas).FirstOrDefault(pt => pt.IdKumpul == id.Value);
                if (sub != null && sub.Tugas != null)
                {
                    return RedirectToAction("SubmisiTugas", new { tugasId = sub.IdTugas });
                }

                var t = _db.Tugas.FirstOrDefault(x => x.IdTugas == id.Value && !x.IsDeleted);
                if (t != null)
                {
                    return RedirectToAction("SubmisiTugas", new { tugasId = t.IdTugas });
                }
            }

            ViewBag.Title = "Koreksi Tugas Siswa";

            var guruKelasList = _db.Kelas
                .Where(k => k.IdGuru == currentUserId && !k.IsDeleted)
                .OrderBy(k => k.NamaKelas)
                .ToList();

            var guruKelasIds = guruKelasList.Select(k => k.IdKelas).ToList();

            // Ambil seluruh submisi tugas dari kelas-kelas yang diampu guru
            var query = _db.PengumpulanTugas
                .Include(pt => pt.Siswa)
                .Include(pt => pt.Tugas)
                .Include(pt => pt.Tugas.Kelas)
                .Where(pt => guruKelasIds.Contains(pt.Tugas.IdKelas) && !pt.Tugas.IsDeleted);

            int totalSubmisi = query.Count();
            int totalPerluDikoreksi = query.Count(pt => pt.Nilai == null);
            int totalSudahDinilai = query.Count(pt => pt.Nilai != null);
            int totalTugas = _db.Tugas.Count(t => guruKelasIds.Contains(t.IdKelas) && !t.IsDeleted);

            // Filter Kelas
            if (kelasId.HasValue && kelasId.Value > 0)
            {
                query = query.Where(pt => pt.Tugas.IdKelas == kelasId.Value);
            }

            // Filter Status
            if (string.Equals(status, "PerluDikoreksi", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(pt => pt.Nilai == null);
            }
            else if (string.Equals(status, "SudahDinilai", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(pt => pt.Nilai != null);
            }

            // Filter Pencarian
            if (!string.IsNullOrWhiteSpace(q))
            {
                string searchLower = q.Trim().ToLower();
                query = query.Where(pt => (pt.Siswa != null && pt.Siswa.NamaLengkap.ToLower().Contains(searchLower)) ||
                                          (pt.Tugas != null && pt.Tugas.JudulTugas.ToLower().Contains(searchLower)));
            }

            var submissions = query
                .OrderByDescending(pt => pt.WaktuKumpul ?? pt.CreatedAt)
                .ToList();

            var listDto = submissions.Select(pt =>
            {
                bool isSubmitted = pt.WaktuKumpul.HasValue;
                bool isLate = isSubmitted && pt.Tugas != null && pt.WaktuKumpul > pt.Tugas.Deadline;

                return new GuruKoreksiTugasItemDto
                {
                    SubmissionId = pt.IdKumpul,
                    TugasId = pt.IdTugas,
                    KelasId = pt.Tugas != null ? pt.Tugas.IdKelas : 0,
                    NamaKelas = pt.Tugas != null && pt.Tugas.Kelas != null ? pt.Tugas.Kelas.NamaKelas : "-",
                    JudulTugas = pt.Tugas != null ? pt.Tugas.JudulTugas : "-",
                    PertemuanKe = pt.Tugas != null ? pt.Tugas.PertemuanKe : 1,
                    Deadline = pt.Tugas != null ? pt.Tugas.Deadline : DateTime.MinValue,
                    NamaSiswa = pt.Siswa != null ? pt.Siswa.NamaLengkap : "Siswa",
                    EmailSiswa = pt.Siswa != null ? pt.Siswa.Email : "-",
                    FotoSiswa = pt.Siswa != null ? pt.Siswa.FotoProfile : null,
                    FileSubmisiUrl = pt.FilePath,
                    NamaFileSubmisi = !string.IsNullOrEmpty(pt.FilePath) ? System.IO.Path.GetFileName(pt.FilePath) : null,
                    SubmittedAt = pt.WaktuKumpul ?? pt.CreatedAt,
                    IsLate = isLate,
                    Nilai = pt.Nilai,
                    Feedback = pt.Feedback,
                    Status = pt.Nilai.HasValue ? "Graded" : "Submitted"
                };
            }).ToList();

            var kelasOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Semua Kelas Diampu", Selected = (!kelasId.HasValue || kelasId.Value == 0) }
            };
            kelasOptions.AddRange(guruKelasList.Select(k => new SelectListItem
            {
                Value = k.IdKelas.ToString(),
                Text = k.NamaKelas,
                Selected = (kelasId.HasValue && kelasId.Value == k.IdKelas)
            }));

            var viewModel = new GuruKoreksiTugasViewModel
            {
                SelectedKelasId = kelasId,
                SelectedStatus = string.IsNullOrEmpty(status) ? "PerluDikoreksi" : status,
                SearchQuery = q,
                TotalTugas = totalTugas,
                TotalSubmisi = totalSubmisi,
                TotalPerluDikoreksi = totalPerluDikoreksi,
                TotalSudahDinilai = totalSudahDinilai,
                DaftarKelasOption = kelasOptions,
                DaftarSubmisi = listDto
            };

            return View("KoreksiTugas", viewModel);
        }

        // POST: /Guru/CreateTugas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTugas(CreateTugasViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            bool isOwner = _db.Kelas.Any(k => k.IdKelas == model.KelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Mohon lengkapi formulir pembuatan tugas dengan benar.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "tugas" });
            }

            string attachmentUrl = null;
            if (model.FileLampiran != null && model.FileLampiran.ContentLength > 0)
            {
                try
                {
                    string ext = System.IO.Path.GetExtension(model.FileLampiran.FileName);
                    string fileName = $"tugas_{model.KelasId}_p{model.PertemuanKe}_{Guid.NewGuid():N}{ext}";
                    string folderPath = Server.MapPath("~/Content/uploads/tugas/");

                    if (!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }

                    string savePath = System.IO.Path.Combine(folderPath, fileName);
                    model.FileLampiran.SaveAs(savePath);

                    attachmentUrl = $"/Content/uploads/tugas/{fileName}";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Gagal mengunggah berkas lampiran tugas: " + ex.Message;
                    return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "tugas" });
                }
            }

            var newTugas = new Learning_Management_System.Models.Entity.Tugas
            {
                IdKelas = model.KelasId,
                PertemuanKe = model.PertemuanKe,
                JudulTugas = model.Judul.Trim(),
                Deskripsi = model.Instruksi != null ? model.Instruksi.Trim() : null,
                Deadline = model.Deadline,
                FileAttachment = attachmentUrl,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            _db.Tugas.Add(newTugas);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Penugasan '{newTugas.JudulTugas}' berhasil diterbitkan!";
            return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "tugas" });
        }

        // GET: /Guru/SubmisiTugas/{tugasId}
        [HttpGet]
        public ActionResult SubmisiTugas(int tugasId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var tugas = _db.Tugas
                .Include(t => t.Kelas)
                .FirstOrDefault(t => t.IdTugas == tugasId && !t.IsDeleted);

            if (tugas == null)
            {
                TempData["ErrorMessage"] = "Penugasan tidak ditemukan.";
                return RedirectToAction("Dashboard");
            }

            if (tugas.Kelas == null || tugas.Kelas.IdGuru != currentUserId || tugas.Kelas.IsDeleted)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Title = $"Submisi Tugas: {tugas.JudulTugas}";

            // Ambil seluruh siswa terdaftar di kelas
            var enrolledMembers = _db.MemberKelas
                .Include(m => m.Siswa)
                .Where(m => m.IdKelas == tugas.IdKelas && m.Status == "Active")
                .ToList();

            var submissions = _db.PengumpulanTugas
                .Where(pt => pt.IdTugas == tugasId)
                .ToList();

            var studentSubmissionsDto = enrolledMembers.Select(m =>
            {
                var sub = submissions.FirstOrDefault(s => s.IdSiswa == m.IdSiswa);
                bool isSubmitted = sub != null && sub.WaktuKumpul.HasValue;
                bool isLate = isSubmitted && sub.WaktuKumpul > tugas.Deadline;

                return new SubmisiTugasItemDto
                {
                    SubmissionId = sub != null ? sub.IdKumpul : 0,
                    StudentId = m.IdSiswa,
                    NamaSiswa = m.Siswa != null ? m.Siswa.NamaLengkap : "Siswa",
                    Email = m.Siswa != null ? m.Siswa.Email : null,
                    FotoSiswa = m.Siswa != null ? m.Siswa.FotoProfile : null,
                    FileSubmisiUrl = sub != null ? sub.FilePath : null,
                    NamaFileSubmisi = sub != null && !string.IsNullOrEmpty(sub.FilePath) ? System.IO.Path.GetFileName(sub.FilePath) : null,
                    SubmittedAt = sub != null ? sub.WaktuKumpul : null,
                    IsLate = isLate,
                    Nilai = sub != null ? sub.Nilai : null,
                    Feedback = sub != null ? sub.Feedback : null,
                    Status = sub != null ? sub.Status : "Belum Mengumpulkan"
                };
            }).OrderByDescending(s => s.SubmittedAt.HasValue).ThenBy(s => s.NamaSiswa).ToList();

            var viewModel = new DetailSubmisiTugasViewModel
            {
                TugasId = tugas.IdTugas,
                KelasId = tugas.IdKelas,
                NamaKelas = tugas.Kelas.NamaKelas,
                JudulTugas = tugas.JudulTugas,
                PertemuanKe = tugas.PertemuanKe,
                Deadline = tugas.Deadline,
                TotalSiswa = enrolledMembers.Count,
                JumlahMengumpulkan = submissions.Count(s => s.WaktuKumpul.HasValue),
                JumlahDinilai = submissions.Count(s => s.Nilai.HasValue),
                ListSubmisi = studentSubmissionsDto
            };

            return View("SubmisiTugas", viewModel);
        }

        // POST: /Guru/BeriNilai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BeriNilai(BeriNilaiViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            bool isOwner = _db.Kelas.Any(k => k.IdKelas == model.KelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            var sub = _db.PengumpulanTugas.FirstOrDefault(s => s.IdKumpul == model.SubmissionId && s.IdTugas == model.TugasId);
            if (sub == null)
            {
                TempData["ErrorMessage"] = "Data pengumpulan tugas siswa tidak ditemukan.";
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("SubmisiTugas", new { tugasId = model.TugasId });
            }

            sub.Nilai = model.Nilai;
            sub.Feedback = model.Feedback != null ? model.Feedback.Trim() : null;
            sub.Status = "Graded";
            sub.UpdatedAt = DateTime.Now;

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Penilaian dan umpan balik berhasil disimpan!";
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("SubmisiTugas", new { tugasId = model.TugasId });
        }

        // POST: /Guru/DeleteTugas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTugas(int id, int kelasId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            bool isOwner = _db.Kelas.Any(k => k.IdKelas == kelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            var tugas = _db.Tugas.FirstOrDefault(t => t.IdTugas == id && t.IdKelas == kelasId && !t.IsDeleted);
            if (tugas != null)
            {
                tugas.IsDeleted = true;
                tugas.DeletedAt = DateTime.Now;
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Penugasan berhasil dihapus.";
            }

            return RedirectToAction("RuangKelas", new { id = kelasId, tab = "tugas" });
        }

        // GET: /Guru/Quiz/{id}
        [HttpGet]
        public ActionResult Quiz(int id)
        {
            return RedirectToAction("RuangKelas", new { id = id, tab = "quiz" });
        }

        // POST: /Guru/CreateQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateQuiz(CreateQuizViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            bool isOwner = _db.Kelas.Any(k => k.IdKelas == model.KelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Mohon lengkapi formulir pembuatan kuis dengan benar.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "quiz" });
            }

            var newQuiz = new Learning_Management_System.Models.Entity.Quiz
            {
                IdKelas = model.KelasId,
                PertemuanKe = model.PertemuanKe,
                JudulQuiz = model.JudulKuis.Trim(),
                Deskripsi = model.Deskripsi != null ? model.Deskripsi.Trim() : null,
                Durasi = model.DurasiMenit,
                PassingScore = model.PassingScore,
                WaktuMulai = model.BatasWaktuMulai,
                WaktuSelesai = model.BatasWaktuSelesai,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.Quiz.Add(newQuiz);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Kuis '{newQuiz.JudulQuiz}' berhasil dibuat! Silakan kelola bank soal di bawah.";
            return RedirectToAction("KelolaSoal", new { quizId = newQuiz.IdQuiz });
        }

        // GET: /Guru/KelolaSoal/{quizId}
        [HttpGet]
        public ActionResult KelolaSoal(int quizId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var quiz = _db.Quiz
                .Include(q => q.Kelas)
                .FirstOrDefault(q => q.IdQuiz == quizId);

            if (quiz == null)
            {
                TempData["ErrorMessage"] = "Kuis tidak ditemukan.";
                return RedirectToAction("Dashboard");
            }

            if (quiz.Kelas == null || quiz.Kelas.IdGuru != currentUserId || quiz.Kelas.IsDeleted)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Title = $"Kelola Soal: {quiz.JudulQuiz}";

            var rawSoal = _db.SoalQuiz.Where(s => s.IdQuiz == quizId).ToList();
            var soalIds = rawSoal.Select(s => s.IdSoal).ToList();

            var rawOpsi = _db.OpsiJawaban.Where(o => soalIds.Contains(o.IdSoal)).ToList();

            var listSoalDto = rawSoal.Select(s =>
            {
                var opsiList = rawOpsi.Where(o => o.IdSoal == s.IdSoal).ToList();
                var correctOpsi = opsiList.FirstOrDefault(o => o.IsCorrect);

                string pilihanA = opsiList.ElementAtOrDefault(0) != null ? opsiList.ElementAtOrDefault(0).TeksOpsi : "";
                string pilihanB = opsiList.ElementAtOrDefault(1) != null ? opsiList.ElementAtOrDefault(1).TeksOpsi : "";
                string pilihanC = opsiList.ElementAtOrDefault(2) != null ? opsiList.ElementAtOrDefault(2).TeksOpsi : "";
                string pilihanD = opsiList.ElementAtOrDefault(3) != null ? opsiList.ElementAtOrDefault(3).TeksOpsi : "";

                string kunciJawaban = "A";
                if (correctOpsi != null)
                {
                    int index = opsiList.IndexOf(correctOpsi);
                    if (index == 1) kunciJawaban = "B";
                    else if (index == 2) kunciJawaban = "C";
                    else if (index == 3) kunciJawaban = "D";
                }

                return new SoalQuizItemDto
                {
                    IdSoal = s.IdSoal,
                    Pertanyaan = s.Pertanyaan,
                    PilihanA = pilihanA,
                    PilihanB = pilihanB,
                    PilihanC = pilihanC,
                    PilihanD = pilihanD,
                    KunciJawaban = kunciJawaban,
                    BobotNilai = 10m
                };
            }).ToList();

            var viewModel = new KelolaSoalViewModel
            {
                QuizId = quiz.IdQuiz,
                KelasId = quiz.IdKelas,
                NamaKelas = quiz.Kelas.NamaKelas,
                JudulKuis = quiz.JudulQuiz,
                DurasiMenit = quiz.Durasi,
                TotalSoal = listSoalDto.Count,
                ListSoal = listSoalDto
            };

            return View("KelolaSoal", viewModel);
        }

        // POST: /Guru/TambahSoal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TambahSoal(CreateSoalViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            bool isOwner = _db.Kelas.Any(k => k.IdKelas == model.KelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Mohon isi seluruh pertanyaan dan 4 pilihan jawaban kuis.";
                return RedirectToAction("KelolaSoal", new { quizId = model.QuizId });
            }

            var newSoal = new Learning_Management_System.Models.Entity.SoalQuiz
            {
                IdQuiz = model.QuizId,
                Pertanyaan = model.Pertanyaan.Trim(),
                TipeSoal = "PilihanGanda"
            };

            _db.SoalQuiz.Add(newSoal);
            _db.SaveChanges();

            // Tambahkan 4 Opsi Jawaban (A, B, C, D)
            var listOpsi = new List<Learning_Management_System.Models.Entity.OpsiJawaban>
            {
                new Learning_Management_System.Models.Entity.OpsiJawaban { IdSoal = newSoal.IdSoal, TeksOpsi = model.PilihanA.Trim(), IsCorrect = model.KunciJawaban == "A" },
                new Learning_Management_System.Models.Entity.OpsiJawaban { IdSoal = newSoal.IdSoal, TeksOpsi = model.PilihanB.Trim(), IsCorrect = model.KunciJawaban == "B" },
                new Learning_Management_System.Models.Entity.OpsiJawaban { IdSoal = newSoal.IdSoal, TeksOpsi = model.PilihanC.Trim(), IsCorrect = model.KunciJawaban == "C" },
                new Learning_Management_System.Models.Entity.OpsiJawaban { IdSoal = newSoal.IdSoal, TeksOpsi = model.PilihanD.Trim(), IsCorrect = model.KunciJawaban == "D" }
            };

            _db.OpsiJawaban.AddRange(listOpsi);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Butir soal pilihan ganda berhasil ditambahkan ke bank soal!";
            return RedirectToAction("KelolaSoal", new { quizId = model.QuizId });
        }

        // POST: /Guru/DeleteSoal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSoal(int id, int quizId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var quiz = _db.Quiz.Include(q => q.Kelas).FirstOrDefault(q => q.IdQuiz == quizId);
            if (quiz == null || quiz.Kelas == null || quiz.Kelas.IdGuru != currentUserId || quiz.Kelas.IsDeleted)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            var soal = _db.SoalQuiz.FirstOrDefault(s => s.IdSoal == id && s.IdQuiz == quizId);
            if (soal != null)
            {
                var relatedOpsi = _db.OpsiJawaban.Where(o => o.IdSoal == id).ToList();
                if (relatedOpsi.Any())
                {
                    _db.OpsiJawaban.RemoveRange(relatedOpsi);
                }

                _db.SoalQuiz.Remove(soal);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Butir soal berhasil dihapus.";
            }

            return RedirectToAction("KelolaSoal", new { quizId = quizId });
        }

        // POST: /Guru/DeleteQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteQuiz(int id, int kelasId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            bool isOwner = _db.Kelas.Any(k => k.IdKelas == kelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            var quiz = _db.Quiz.FirstOrDefault(q => q.IdQuiz == id && q.IdKelas == kelasId);
            if (quiz != null)
            {
                var relatedAttempts = _db.NilaiQuiz.Where(n => n.IdQuiz == id).ToList();
                if (relatedAttempts.Any()) _db.NilaiQuiz.RemoveRange(relatedAttempts);

                var relatedSoal = _db.SoalQuiz.Where(s => s.IdQuiz == id).ToList();
                var soalIds = relatedSoal.Select(s => s.IdSoal).ToList();
                var relatedOpsi = _db.OpsiJawaban.Where(o => soalIds.Contains(o.IdSoal)).ToList();

                if (relatedOpsi.Any()) _db.OpsiJawaban.RemoveRange(relatedOpsi);
                if (relatedSoal.Any()) _db.SoalQuiz.RemoveRange(relatedSoal);

                _db.Quiz.Remove(quiz);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Kuis online berhasil dihapus.";
            }

            return RedirectToAction("RuangKelas", new { id = kelasId, tab = "quiz" });
        }

        // GET: /Guru/HasilQuiz/{quizId}
        [HttpGet]
        public ActionResult HasilQuiz(int quizId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var quiz = _db.Quiz
                .Include(q => q.Kelas)
                .FirstOrDefault(q => q.IdQuiz == quizId);

            if (quiz == null)
            {
                TempData["ErrorMessage"] = "Kuis tidak ditemukan.";
                return RedirectToAction("Dashboard");
            }

            if (quiz.Kelas == null || quiz.Kelas.IdGuru != currentUserId || quiz.Kelas.IsDeleted)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Title = $"Hasil Kuis: {quiz.JudulQuiz}";

            var enrolledMembers = _db.MemberKelas
                .Include(m => m.Siswa)
                .Where(m => m.IdKelas == quiz.IdKelas && m.Status == "Active")
                .ToList();

            var attempts = _db.NilaiQuiz
                .Where(n => n.IdQuiz == quizId)
                .ToList();

            int totalSoalQuiz = _db.SoalQuiz.Count(s => s.IdQuiz == quizId);

            var studentHasilDto = enrolledMembers.Select(m =>
            {
                var att = attempts.FirstOrDefault(a => a.IdSiswa == m.IdSiswa);
                bool isCompleted = att != null && att.WaktuSubmit.HasValue;

                int benarCount = 0;
                int salahCount = 0;

                if (isCompleted && att.Score.HasValue && totalSoalQuiz > 0)
                {
                    // Estimasikan jumlah benar berdasarkan persentase skor
                    benarCount = (int)Math.Round((att.Score.Value / 100m) * totalSoalQuiz);
                    salahCount = totalSoalQuiz - benarCount;
                    if (salahCount < 0) salahCount = 0;
                }

                string durasiFormatted = "-";
                if (att != null && att.WaktuMulai.HasValue && att.WaktuSubmit.HasValue)
                {
                    var ts = att.WaktuSubmit.Value - att.WaktuMulai.Value;
                    durasiFormatted = $"{ts.Minutes}m {ts.Seconds}s";
                }

                return new HasilQuizItemDto
                {
                    AttemptId = att != null ? att.IdAttempt : 0,
                    StudentId = m.IdSiswa,
                    NamaSiswa = m.Siswa != null ? m.Siswa.NamaLengkap : "Siswa",
                    Email = m.Siswa != null ? m.Siswa.Email : null,
                    FotoSiswa = m.Siswa != null ? m.Siswa.FotoProfile : null,
                    JumlahBenar = benarCount,
                    JumlahSalah = salahCount,
                    TotalNilai = att != null ? att.Score : null,
                    StatusLulus = att != null && att.Score.HasValue && att.Score.Value >= quiz.PassingScore,
                    WaktuMulai = att != null ? att.WaktuMulai : null,
                    WaktuSelesai = att != null ? att.WaktuSubmit : null,
                    DurasiPengerjaanFormatted = durasiFormatted,
                    Status = att != null ? att.Status : "Belum Mengerjakan"
                };
            }).OrderByDescending(h => h.TotalNilai.HasValue).ThenBy(h => h.NamaSiswa).ToList();

            decimal rataRata = studentHasilDto.Where(h => h.TotalNilai.HasValue).Select(h => h.TotalNilai.Value).DefaultIfEmpty(0m).Average();

            var viewModel = new RekapHasilQuizViewModel
            {
                QuizId = quiz.IdQuiz,
                KelasId = quiz.IdKelas,
                NamaKelas = quiz.Kelas.NamaKelas,
                JudulKuis = quiz.JudulQuiz,
                PassingScore = quiz.PassingScore,
                TotalSiswaKelas = enrolledMembers.Count,
                TotalSudahSubmit = attempts.Count(a => a.WaktuSubmit.HasValue),
                RataRataNilai = Math.Round(rataRata, 1),
                ListHasil = studentHasilDto
            };

            return View("HasilQuiz", viewModel);
        }

        // GET: /Guru/Absensi/{id}
        [HttpGet]
        public ActionResult Absensi(int id)
        {
            return RedirectToAction("RuangKelas", new { id = id, tab = "absensi" });
        }

        // POST: /Guru/CreateSesiAbsensi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSesiAbsensi(CreateSesiAbsensiViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            bool isOwner = _db.Kelas.Any(k => k.IdKelas == model.KelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Mohon lengkapi formulir pembuatan sesi presensi dengan benar.";
                return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "absensi" });
            }

            TimeSpan jamMulai = TimeSpan.FromHours(8);
            TimeSpan jamSelesai = TimeSpan.FromHours(10);

            if (!string.IsNullOrEmpty(model.JamMulaiStr) && TimeSpan.TryParse(model.JamMulaiStr, out TimeSpan parsedMulai))
            {
                jamMulai = parsedMulai;
            }

            if (!string.IsNullOrEmpty(model.JamSelesaiStr) && TimeSpan.TryParse(model.JamSelesaiStr, out TimeSpan parsedSelesai))
            {
                jamSelesai = parsedSelesai;
            }

            string token = !string.IsNullOrWhiteSpace(model.TokenCustom) ? model.TokenCustom.Trim().ToUpper() : GenerateRandomToken(6);

            var newSesi = new Learning_Management_System.Models.Entity.JadwalAbsen
            {
                IdKelas = model.KelasId,
                PertemuanKe = model.PertemuanKe,
                Tanggal = model.Tanggal.Date,
                JamMulai = jamMulai,
                JamSelesai = jamSelesai,
                TokenPresensi = token,
                IsOpen = true,
                CreatedAt = DateTime.Now
            };

            _db.JadwalAbsen.Add(newSesi);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Sesi presensi Pertemuan {newSesi.PertemuanKe} berhasil dibuka! Token: '{token}'";
            return RedirectToAction("RuangKelas", new { id = model.KelasId, tab = "absensi" });
        }

        // GET: /Guru/DetailPresensi/{sesiId}
        [HttpGet]
        public ActionResult DetailPresensi(int sesiId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var sesi = _db.JadwalAbsen
                .Include(j => j.Kelas)
                .FirstOrDefault(j => j.IdJadwal == sesiId);

            if (sesi == null)
            {
                TempData["ErrorMessage"] = "Sesi presensi tidak ditemukan.";
                return RedirectToAction("Dashboard");
            }

            if (sesi.Kelas == null || sesi.Kelas.IdGuru != currentUserId || sesi.Kelas.IsDeleted)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Title = $"Detail Presensi: Pertemuan {sesi.PertemuanKe}";

            var enrolledMembers = _db.MemberKelas
                .Include(m => m.Siswa)
                .Where(m => m.IdKelas == sesi.IdKelas && m.Status == "Active")
                .ToList();

            var absensiRecords = _db.Absensi
                .Where(a => a.IdJadwal == sesiId)
                .ToList();

            var rekapSiswaDto = enrolledMembers.Select(m =>
            {
                var rec = absensiRecords.FirstOrDefault(a => a.IdSiswa == m.IdSiswa);

                return new RekapPresensiSiswaItemDto
                {
                    PresensiId = rec != null ? rec.IdAbsen : 0,
                    StudentId = m.IdSiswa,
                    NamaSiswa = m.Siswa != null ? m.Siswa.NamaLengkap : "Siswa",
                    Email = m.Siswa != null ? m.Siswa.Email : null,
                    FotoSiswa = m.Siswa != null ? m.Siswa.FotoProfile : null,
                    StatusKehadiran = rec != null && !string.IsNullOrEmpty(rec.Status) ? rec.Status : "Belum Presensi",
                    WaktuPresensi = rec != null ? rec.WaktuAbsen : null,
                    Keterangan = rec != null ? rec.Catatan : null
                };
            }).OrderBy(r => r.NamaSiswa).ToList();

            var viewModel = new RekapPresensiKelasViewModel
            {
                SesiAbsensiId = sesi.IdJadwal,
                KelasId = sesi.IdKelas,
                NamaKelas = sesi.Kelas.NamaKelas,
                PertemuanKe = sesi.PertemuanKe,
                Tanggal = sesi.Tanggal,
                JamMulai = sesi.JamMulai,
                JamSelesai = sesi.JamSelesai,
                TokenPresensi = sesi.TokenPresensi,
                IsOpen = sesi.IsOpen,
                TotalSiswa = enrolledMembers.Count,
                TotalHadir = rekapSiswaDto.Count(r => r.StatusKehadiran.Equals("Hadir", StringComparison.OrdinalIgnoreCase)),
                TotalIzin = rekapSiswaDto.Count(r => r.StatusKehadiran.Equals("Izin", StringComparison.OrdinalIgnoreCase)),
                TotalSakit = rekapSiswaDto.Count(r => r.StatusKehadiran.Equals("Sakit", StringComparison.OrdinalIgnoreCase)),
                TotalAlpa = rekapSiswaDto.Count(r => r.StatusKehadiran.Equals("Alpa", StringComparison.OrdinalIgnoreCase)),
                ListRekapSiswa = rekapSiswaDto
            };

            return View("DetailPresensi", viewModel);
        }

        // POST: /Guru/UpdateStatusPresensi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatusPresensi(UpdateStatusPresensiViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            bool isOwner = _db.Kelas.Any(k => k.IdKelas == model.KelasId && k.IdGuru == currentUserId && !k.IsDeleted);
            if (!isOwner)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            var record = _db.Absensi.FirstOrDefault(a => a.IdJadwal == model.SesiAbsensiId && a.IdSiswa == model.StudentId);
            if (record != null)
            {
                record.Status = model.StatusKehadiran;
                record.Catatan = model.Keterangan != null ? model.Keterangan.Trim() : null;
                record.WaktuAbsen = DateTime.Now;
            }
            else
            {
                var newAbsen = new Learning_Management_System.Models.Entity.Absensi
                {
                    IdJadwal = model.SesiAbsensiId,
                    IdSiswa = model.StudentId,
                    Status = model.StatusKehadiran,
                    Catatan = model.Keterangan != null ? model.Keterangan.Trim() : null,
                    WaktuAbsen = DateTime.Now
                };
                _db.Absensi.Add(newAbsen);
            }

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Status kehadiran siswa berhasil diperbarui!";
            return RedirectToAction("DetailPresensi", new { sesiId = model.SesiAbsensiId });
        }

        // POST: /Guru/TutupSesiAbsensi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TutupSesiAbsensi(int sesiId)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var sesi = _db.JadwalAbsen.Include(j => j.Kelas).FirstOrDefault(j => j.IdJadwal == sesiId);
            if (sesi == null || sesi.Kelas == null || sesi.Kelas.IdGuru != currentUserId || sesi.Kelas.IsDeleted)
            {
                TempData["ErrorMessage"] = "Akses ditolak. Anda bukan pengampu kelas ini.";
                return RedirectToAction("Dashboard");
            }

            sesi.IsOpen = false;
            sesi.UpdatedAt = DateTime.Now;

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Sesi presensi telah resmi ditutup.";
            return RedirectToAction("DetailPresensi", new { sesiId = sesiId });
        }

        private static string GenerateRandomToken(int length = 6)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }

        // GET: /Guru/Gradebook
        [HttpGet]
        public ActionResult Gradebook(int? kelasId = null)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            ViewBag.Title = "Rekap Buku Nilai (Gradebook)";

            var guruKelasList = _db.Kelas
                .Where(k => k.IdGuru == currentUserId && !k.IsDeleted)
                .OrderBy(k => k.NamaKelas)
                .ToList();

            if (!guruKelasList.Any())
            {
                var emptyVm = new GuruGradebookPageViewModel
                {
                    HasClasses = false
                };
                return View("Gradebook", emptyVm);
            }

            int targetKelasId = (kelasId.HasValue && guruKelasList.Any(k => k.IdKelas == kelasId.Value))
                ? kelasId.Value
                : guruKelasList.First().IdKelas;

            var kelasOptions = guruKelasList.Select(k => new SelectListItem
            {
                Value = k.IdKelas.ToString(),
                Text = $"{k.NamaKelas} (KLS-{k.IdKelas:D3})",
                Selected = (k.IdKelas == targetKelasId)
            }).ToList();

            var gradebookVm = GetGradebookViewModel(targetKelasId, currentUserId);

            var pageVm = new GuruGradebookPageViewModel
            {
                SelectedKelasId = targetKelasId,
                DaftarKelasOption = kelasOptions,
                GradebookData = gradebookVm,
                HasClasses = true
            };

            return View("Gradebook", pageVm);
        }

        // GET: /Guru/Nilai/{id?}
        [HttpGet]
        public ActionResult Nilai(int? id = null)
        {
            if (id.HasValue && id.Value > 0)
            {
                return RedirectToAction("Gradebook", new { kelasId = id.Value });
            }
            return RedirectToAction("Gradebook");
        }

        // GET: /Guru/ExportNilaiExcel/{id}
        [HttpGet]
        public ActionResult ExportNilaiExcel(int id)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            var vm = GetGradebookViewModel(id, currentUserId);
            if (vm == null)
            {
                TempData["ErrorMessage"] = "Kelas tidak ditemukan atau akses ditolak.";
                return RedirectToAction("Dashboard");
            }

            var sb = new System.Text.StringBuilder();

            // Header Info Spreadsheet
            sb.AppendLine($"\"REKAPITULASI BUKU NILAI KELAS - {vm.NamaKelas.ToUpper()}\"");
            sb.AppendLine($"\"Kode Kelas: {vm.KodeKelas}\";\"Kategori: {vm.Kategori}\";\"Pengampu: {vm.NamaGuru}\"");
            sb.AppendLine($"\"Tanggal Ekspor: {DateTime.Now:dd MMMM yyyy HH:mm}\"");
            sb.AppendLine();

            // Header Kolom Tabel
            var headers = new List<string> { "No", "Nama Siswa", "Email", "Absensi (15%)", "Materi (15%)" };
            foreach (var t in vm.DaftarTugasHeader) headers.Add($"\"{t.KodeShort}: {t.Judul}\"");
            headers.Add("Rata Tugas (30%)");
            foreach (var q in vm.DaftarQuizHeader) headers.Add($"\"{q.KodeShort}: {q.Judul}\"");
            headers.Add("Rata Quiz (40%)");
            headers.Add("Nilai Akhir");
            headers.Add("Predikat");
            headers.Add("Status Kelulusan");

            sb.AppendLine(string.Join(";", headers));

            int no = 1;
            foreach (var row in vm.RekapBarisSiswa)
            {
                var line = new List<string>
                {
                    no++.ToString(),
                    $"\"{row.NamaSiswa}\"",
                    $"\"{row.Email}\"",
                    $"{row.PersentaseAbsensi}%",
                    $"{row.PersentaseMateri}%"
                };

                foreach (var t in vm.DaftarTugasHeader)
                {
                    var val = row.NilaiTugasMap.ContainsKey(t.Id) && row.NilaiTugasMap[t.Id].HasValue ? row.NilaiTugasMap[t.Id].Value.ToString("0.0") : "-";
                    line.Add(val);
                }
                line.Add(row.RataRataTugas.ToString("0.0"));

                foreach (var q in vm.DaftarQuizHeader)
                {
                    var val = row.NilaiQuizMap.ContainsKey(q.Id) && row.NilaiQuizMap[q.Id].HasValue ? row.NilaiQuizMap[q.Id].Value.ToString("0.0") : "-";
                    line.Add(val);
                }
                line.Add(row.RataRataQuiz.ToString("0.0"));

                line.Add(row.NilaiAkhirAkumulasi.ToString("0.0"));
                line.Add(row.PredikatHuruf);
                line.Add(row.StatusLulus ? "LULUS" : "TIDAK LULUS");

                sb.AppendLine(string.Join(";", line));
            }

            var preamble = System.Text.Encoding.UTF8.GetPreamble();
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var fileBytes = new byte[preamble.Length + bytes.Length];
            Buffer.BlockCopy(preamble, 0, fileBytes, 0, preamble.Length);
            Buffer.BlockCopy(bytes, 0, fileBytes, preamble.Length, bytes.Length);

            string fileName = $"Buku_Nilai_{vm.KodeKelas}_{DateTime.Now:yyyyMMdd}.csv";
            return File(fileBytes, "text/csv; charset=utf-8", fileName);
        }

        // GET: /Guru/ExportNilaiPdf/{id}
        [HttpGet]
        public ActionResult ExportNilaiPdf(int id)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            var vm = GetGradebookViewModel(id, currentUserId);
            if (vm == null)
            {
                TempData["ErrorMessage"] = "Kelas tidak ditemukan atau akses ditolak.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Title = $"Cetak Laporan Nilai - {vm.NamaKelas}";
            return View("ExportNilaiPrint", vm);
        }

        private GuruGradebookViewModel GetGradebookViewModel(int id, int currentUserId)
        {
            var kelas = _db.Kelas
                .Include(k => k.Kategori)
                .Include(k => k.Guru)
                .FirstOrDefault(k => k.IdKelas == id && !k.IsDeleted);

            if (kelas == null || kelas.IdGuru != currentUserId)
            {
                return null;
            }

            var tugasList = _db.Tugas
                .Where(t => t.IdKelas == id && !t.IsDeleted)
                .OrderBy(t => t.PertemuanKe)
                .ToList();

            var quizList = _db.Quiz
                .Where(q => q.IdKelas == id)
                .OrderBy(q => q.PertemuanKe)
                .ToList();

            var tugasHeaders = tugasList.Select((t, index) => new HeaderKolomItemDto
            {
                Id = t.IdTugas,
                Judul = t.JudulTugas,
                PertemuanKe = t.PertemuanKe,
                KodeShort = $"T{index + 1}"
            }).ToList();

            var quizHeaders = quizList.Select((q, index) => new HeaderKolomItemDto
            {
                Id = q.IdQuiz,
                Judul = q.JudulQuiz,
                PertemuanKe = q.PertemuanKe,
                KodeShort = $"Q{index + 1}"
            }).ToList();

            var members = _db.MemberKelas
                .Include(m => m.Siswa)
                .Where(m => m.IdKelas == id && m.Status == "Active")
                .ToList();

            var memberIds = members.Select(m => m.IdSiswa).ToList();

            var tugasSubmissions = _db.PengumpulanTugas
                .Where(s => memberIds.Contains(s.IdSiswa) && s.Nilai.HasValue)
                .ToList();

            var quizAttempts = _db.NilaiQuiz
                .Where(n => memberIds.Contains(n.IdSiswa) && n.Score.HasValue)
                .ToList();

            var totalJadwalAbsen = _db.JadwalAbsen.Count(j => j.IdKelas == id);
            var absensiHadir = _db.Absensi
                .Include(a => a.JadwalAbsen)
                .Where(a => a.JadwalAbsen.IdKelas == id && memberIds.Contains(a.IdSiswa) && a.Status == "Hadir")
                .ToList();

            var totalMateri = _db.Materi.Count(m => m.IdKelas == id && !m.IsDeleted);
            var materiProgress = _db.ProgresMateri
                .Include(p => p.Materi)
                .Where(p => p.Materi.IdKelas == id && memberIds.Contains(p.IdSiswa) && p.IsSelesai)
                .ToList();

            var studentRows = members.Select(m =>
            {
                var tugasMap = new Dictionary<int, decimal?>();
                foreach (var t in tugasList)
                {
                    var sub = tugasSubmissions.FirstOrDefault(s => s.IdTugas == t.IdTugas && s.IdSiswa == m.IdSiswa);
                    tugasMap[t.IdTugas] = sub != null ? sub.Nilai : null;
                }

                var quizMap = new Dictionary<int, decimal?>();
                foreach (var q in quizList)
                {
                    var att = quizAttempts.FirstOrDefault(a => a.IdQuiz == q.IdQuiz && a.IdSiswa == m.IdSiswa);
                    quizMap[q.IdQuiz] = att != null ? att.Score : null;
                }

                int countHadir = absensiHadir.Count(a => a.IdSiswa == m.IdSiswa);
                decimal pctAbsen = totalJadwalAbsen > 0 ? ((decimal)countHadir / totalJadwalAbsen) * 100m : 100m;
                if (pctAbsen > 100m) pctAbsen = 100m;

                int countMateri = materiProgress.Count(p => p.IdSiswa == m.IdSiswa);
                decimal pctMateri = totalMateri > 0 ? ((decimal)countMateri / totalMateri) * 100m : 100m;
                if (pctMateri > 100m) pctMateri = 100m;

                var validTugasVals = tugasMap.Values.Where(v => v.HasValue).Select(v => v.Value).ToList();
                decimal avgTugas = validTugasVals.Any() ? validTugasVals.Average() : 0m;

                var validQuizVals = quizMap.Values.Where(v => v.HasValue).Select(v => v.Value).ToList();
                decimal avgQuiz = validQuizVals.Any() ? validQuizVals.Average() : 0m;

                // Formula Bobot: 15% Absen, 15% Materi, 40% Quiz, 30% Tugas
                decimal finalScore = (pctAbsen * 0.15m) + (pctMateri * 0.15m) + (avgQuiz * 0.40m) + (avgTugas * 0.30m);
                finalScore = Math.Round(finalScore, 1);

                string predikat = "E";
                if (finalScore >= 85m) predikat = "A";
                else if (finalScore >= 75m) predikat = "B";
                else if (finalScore >= 65m) predikat = "C";
                else if (finalScore >= 55m) predikat = "D";

                return new GradebookRowSiswaDto
                {
                    StudentId = m.IdSiswa,
                    NamaSiswa = m.Siswa != null ? m.Siswa.NamaLengkap : "Siswa",
                    Email = m.Siswa != null ? m.Siswa.Email : null,
                    FotoSiswa = m.Siswa != null ? m.Siswa.FotoProfile : null,
                    NilaiTugasMap = tugasMap,
                    NilaiQuizMap = quizMap,
                    PersentaseAbsensi = Math.Round(pctAbsen, 1),
                    PersentaseMateri = Math.Round(pctMateri, 1),
                    RataRataTugas = Math.Round(avgTugas, 1),
                    RataRataQuiz = Math.Round(avgQuiz, 1),
                    NilaiAkhirAkumulasi = finalScore,
                    PredikatHuruf = predikat,
                    StatusLulus = finalScore >= 70m
                };
            }).OrderByDescending(r => r.NilaiAkhirAkumulasi).ThenBy(r => r.NamaSiswa).ToList();

            return new GuruGradebookViewModel
            {
                KelasId = kelas.IdKelas,
                NamaKelas = kelas.NamaKelas,
                KodeKelas = $"KLS-{kelas.IdKelas:D3}",
                Kategori = kelas.Kategori != null ? kelas.Kategori.NamaKategori : "Umum",
                NamaGuru = kelas.Guru != null ? kelas.Guru.NamaLengkap : "Guru Pengampu",
                DaftarTugasHeader = tugasHeaders,
                DaftarQuizHeader = quizHeaders,
                RekapBarisSiswa = studentRows
            };
        }

        private static string FormatBytes(long? bytes)
        {
            if (!bytes.HasValue || bytes.Value <= 0) return "-";
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblSByte = bytes.Value;
            while (dblSByte >= 1024 && i < suffixes.Length - 1)
            {
                dblSByte /= 1024;
                i++;
            }
            return $"{dblSByte:0.##} {suffixes[i]}";
        }

        // GET: /Guru/RekapNilaiKalkulasi/{id}
        [HttpGet]
        public ActionResult RekapNilaiKalkulasi(int id)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var kelas = _db.Kelas
                .Include(k => k.Kategori)
                .FirstOrDefault(k => k.IdKelas == id && k.IdGuru == currentUserId && !k.IsDeleted);

            if (kelas == null)
            {
                TempData["ErrorMessage"] = "Kelas tidak ditemukan atau akses ditolak.";
                return RedirectToAction("Dashboard");
            }

            var members = _db.MemberKelas
                .Include(m => m.Siswa)
                .Where(m => m.IdKelas == id && m.Status == "Active")
                .ToList();

            var memberIds = members.Select(m => m.IdSiswa).ToList();

            int totalSesiPresensi = _db.JadwalAbsen.Count(j => j.IdKelas == id);
            int totalMateriKelas = _db.Materi.Count(m => m.IdKelas == id && !m.IsDeleted);
            int totalQuizKelas = _db.Quiz.Count(q => q.IdKelas == id);
            int totalTugasKelas = _db.Tugas.Count(t => t.IdKelas == id && !t.IsDeleted);

            var absensiHadir = _db.Absensi
                .Include(a => a.JadwalAbsen)
                .Where(a => a.JadwalAbsen.IdKelas == id && memberIds.Contains(a.IdSiswa) && a.Status == "Hadir")
                .ToList();

            var materiProgress = _db.ProgresMateri
                .Include(p => p.Materi)
                .Where(p => p.Materi.IdKelas == id && memberIds.Contains(p.IdSiswa) && p.IsSelesai)
                .ToList();

            var quizAttempts = _db.NilaiQuiz
                .Where(n => memberIds.Contains(n.IdSiswa) && n.Score.HasValue)
                .ToList();

            var tugasSubmissions = _db.PengumpulanTugas
                .Where(s => memberIds.Contains(s.IdSiswa) && s.Nilai.HasValue)
                .ToList();

            var rekapList = members.Select(m =>
            {
                // 1. Absensi (15%)
                int countHadir = absensiHadir.Count(a => a.IdSiswa == m.IdSiswa);
                decimal skorAbsen = totalSesiPresensi > 0 ? ((decimal)countHadir / totalSesiPresensi) * 100m : 100m;
                if (skorAbsen > 100m) skorAbsen = 100m;
                decimal bobotAbsen = skorAbsen * 0.15m;

                // 2. Progres Materi (15%)
                int countMateri = materiProgress.Count(p => p.IdSiswa == m.IdSiswa);
                decimal skorMateri = totalMateriKelas > 0 ? ((decimal)countMateri / totalMateriKelas) * 100m : 100m;
                if (skorMateri > 100m) skorMateri = 100m;
                decimal bobotMateri = skorMateri * 0.15m;

                // 3. Rata-rata Kuis (40%)
                var studentQuizzes = quizAttempts.Where(q => q.IdSiswa == m.IdSiswa).Select(q => q.Score.Value).ToList();
                decimal rataQuiz = studentQuizzes.Any() ? studentQuizzes.Average() : 0m;
                decimal bobotQuiz = rataQuiz * 0.40m;

                // 4. Rata-rata Tugas (30%)
                var studentTugas = tugasSubmissions.Where(t => t.IdSiswa == m.IdSiswa).Select(t => t.Nilai.Value).ToList();
                decimal rataTugas = studentTugas.Any() ? studentTugas.Average() : 0m;
                decimal bobotTugas = rataTugas * 0.30m;

                // Hasil Akhir Kumulatif
                decimal finalScore = Math.Round(bobotAbsen + bobotMateri + bobotQuiz + bobotTugas, 1);

                string predikat = "E";
                if (finalScore >= 85m) predikat = "A";
                else if (finalScore >= 75m) predikat = "B";
                else if (finalScore >= 60m) predikat = "C";
                else if (finalScore >= 50m) predikat = "D";

                return new SiswaNilaiKomprehensifDto
                {
                    StudentId = m.IdSiswa,
                    NamaSiswa = m.Siswa != null ? m.Siswa.NamaLengkap : "Siswa",
                    Email = m.Siswa != null ? m.Siswa.Email : null,
                    FotoSiswa = m.Siswa != null ? m.Siswa.FotoProfile : null,
                    TotalKehadiran = countHadir,
                    TotalSesiPresensi = totalSesiPresensi,
                    SkorAbsensi = Math.Round(skorAbsen, 1),
                    NilaiBobotAbsensi = Math.Round(bobotAbsen, 2),
                    TotalMateriDibaca = countMateri,
                    TotalMateriKelas = totalMateriKelas,
                    SkorMateri = Math.Round(skorMateri, 1),
                    NilaiBobotMateri = Math.Round(bobotMateri, 2),
                    RataRataSkorQuiz = Math.Round(rataQuiz, 1),
                    NilaiBobotQuiz = Math.Round(bobotQuiz, 2),
                    RataRataSkorTugas = Math.Round(rataTugas, 1),
                    NilaiBobotTugas = Math.Round(bobotTugas, 2),
                    NilaiAkhirKumulatif = finalScore,
                    PredikatHuruf = predikat,
                    StatusKelulusan = finalScore >= 70m ? "Lulus" : "Tidak Lulus"
                };
            }).OrderByDescending(r => r.NilaiAkhirKumulatif).ThenBy(r => r.NamaSiswa).ToList();

            var viewModel = new GuruKalkulasiNilaiViewModel
            {
                KelasId = kelas.IdKelas,
                NamaKelas = kelas.NamaKelas,
                KodeKelas = $"KLS-{kelas.IdKelas:D3}",
                Kategori = kelas.Kategori != null ? kelas.Kategori.NamaKategori : "Umum",
                TotalPertemuan = totalSesiPresensi,
                TotalMateri = totalMateriKelas,
                TotalQuiz = totalQuizKelas,
                TotalTugas = totalTugasKelas,
                RekapSiswa = rekapList
            };

            ViewBag.Title = $"Kalkulasi Nilai Komprehensif - {kelas.NamaKelas}";
            return View("RekapNilaiKalkulasi", viewModel);
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
