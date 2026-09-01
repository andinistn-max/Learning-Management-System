using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Learning_Management_System.Helpers;
using Learning_Management_System.Models.Entity;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Learning_Management_System.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly LmsDbContext _db = new LmsDbContext();

        // GET: /Admin or /Admin/Dashboard
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // GET: /Admin/Dashboard
        [HttpGet]
        public ActionResult Dashboard()
        {
            ViewBag.Title = "Dashboard Real-Time Administrator";

            var viewModel = new AdminDashboardViewModel();

            // 1. Hitung total akun role Guru yang aktif
            viewModel.TotalGuru = _db.Users.Count(u => u.Role != null && u.Role.NamaRole == "Guru" && u.IsActive);

            // 2. Hitung total akun role Siswa yang aktif
            viewModel.TotalSiswa = _db.Users.Count(u => u.Role != null && u.Role.NamaRole == "Siswa" && u.IsActive);

            // 3. Hitung total kelas yang tersedia (publik & tidak terhapus)
            viewModel.TotalKelas = _db.Kelas.Count(k => k.IsPublish && !k.IsDeleted);

            // 4. Hitung total modul materi yang diunggah
            viewModel.TotalMateri = _db.Materi.Count(m => !m.IsDeleted);

            // 5. Hitung total kuis & tugas aktif
            int totalQuiz = _db.Quiz.Count();
            int totalTugas = _db.Tugas.Count();
            viewModel.TotalKuisTugas = totalQuiz + totalTugas;

            // 6. Query 5 kelas terpopuler berdasarkan jumlah siswa terbanyak
            var memberCounts = _db.MemberKelas
                .GroupBy(m => m.IdKelas)
                .Select(g => new { IdKelas = g.Key, Count = g.Count() })
                .ToList();

            var activeKelas = _db.Kelas
                .Include(k => k.Guru)
                .Include(k => k.Kategori)
                .Where(k => k.IsPublish && !k.IsDeleted)
                .ToList();

            viewModel.KelasTerpopuler = activeKelas
                .Select(k => new KelasPopulerItemDto
                {
                    IdKelas = k.IdKelas,
                    NamaKelas = k.NamaKelas,
                    NamaGuru = k.Guru != null ? k.Guru.NamaLengkap : "Pengajar",
                    NamaKategori = k.Kategori != null ? k.Kategori.NamaKategori : "Umum",
                    JumlahSiswa = memberCounts.FirstOrDefault(m => m.IdKelas == k.IdKelas)?.Count ?? 0,
                    ThumbnailUrl = k.Thumbnail
                })
                .OrderByDescending(k => k.JumlahSiswa)
                .ThenByDescending(k => k.IdKelas)
                .Take(5)
                .ToList();

            // 7. Query 10 aktivitas log sistem terbaru (registrasi akun baru, pembuatan kelas, dll)
            var logs = new List<LogAktivitasItemDto>();

            var notifs = _db.Notifikasi
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToList();

            foreach (var n in notifs)
            {
                logs.Add(new LogAktivitasItemDto
                {
                    Id = n.IdNotif,
                    JudulAktivitas = n.Judul,
                    Deskripsi = n.Pesan,
                    Tanggal = n.CreatedAt,
                    TipeAktivitas = !string.IsNullOrEmpty(n.TipeNotif) ? n.TipeNotif : "Notifikasi",
                    NamaUser = n.User != null ? n.User.NamaLengkap : "Sistem"
                });
            }

            // Jika Log notifikasi kurang dari 10, tambahkan dari pendaftaran akun user terbaru
            if (logs.Count < 10)
            {
                var recentUsers = _db.Users
                    .Include(u => u.Role)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10 - logs.Count)
                    .ToList();

                foreach (var u in recentUsers)
                {
                    string roleStr = u.Role != null ? u.Role.NamaRole : "Pengguna";
                    logs.Add(new LogAktivitasItemDto
                    {
                        Id = u.IdUser,
                        JudulAktivitas = $"Pendaftaran Akun {roleStr} Baru",
                        Deskripsi = $"{u.NamaLengkap} ({u.Email}) terdaftar di platform.",
                        Tanggal = u.CreatedAt,
                        TipeAktivitas = "Registrasi",
                        NamaUser = u.NamaLengkap
                    });
                }
            }

            viewModel.LogAktivitasTerbaru = logs.OrderByDescending(l => l.Tanggal).Take(10).ToList();

            return View(viewModel);
        }



        // GET: /Admin/Guru
        [HttpGet]
        public ActionResult Guru(string search, string status, string sort, int page = 1)
        {
            ViewBag.Title = "Manajemen Akun Pengajar / Guru";

            int pageSize = 10;
            if (page < 1) page = 1;

            // Query dasar: Ambil pengguna dengan Role == "Guru"
            var query = _db.Users
                .Include(u => u.Role)
                .Include(u => u.Profile)
                .Where(u => u.Role != null && u.Role.NamaRole == "Guru");

            // Filter Pencarian (Nama / Email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(u => u.NamaLengkap.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
            }

            // Filter Status (Aktif / Nonaktif / Semua)
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("Semua", StringComparison.OrdinalIgnoreCase))
            {
                if (status.Equals("Aktif", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(u => u.IsActive == true);
                }
                else if (status.Equals("Nonaktif", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(u => u.IsActive == false);
                }
            }

            // Hitung Total Record untuk Pagination
            int totalRecords = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var guruUsers = query.ToList();

            var guruUserIds = guruUsers.Select(g => g.IdUser).ToList();
            var kelasCounts = _db.Kelas
                .Where(k => !k.IsDeleted && guruUserIds.Contains(k.IdGuru))
                .GroupBy(k => k.IdGuru)
                .Select(g => new { IdGuru = g.Key, Count = g.Count() })
                .ToList();

            var dtoList = guruUsers.Select(u => new GuruItemDto
            {
                UserId = u.IdUser,
                NamaLengkap = u.NamaLengkap,
                Email = u.Email,
                NoTelepon = u.Profile != null ? u.Profile.NoHp : "-",
                FotoProfile = u.FotoProfile,
                JumlahKelasDiampu = kelasCounts.FirstOrDefault(kc => kc.IdGuru == u.IdUser)?.Count ?? 0,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).AsQueryable();

            // Sorting
            if (string.Equals(sort, "Tanggal", StringComparison.OrdinalIgnoreCase))
            {
                dtoList = dtoList.OrderByDescending(g => g.CreatedAt);
            }
            else if (string.Equals(sort, "Kelas", StringComparison.OrdinalIgnoreCase))
            {
                dtoList = dtoList.OrderByDescending(g => g.JumlahKelasDiampu);
            }
            else
            {
                dtoList = dtoList.OrderBy(g => g.NamaLengkap);
            }

            // Pagination
            var pagedList = dtoList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AdminGuruListViewModel
            {
                DaftarGuru = pagedList,
                SearchQuery = search,
                StatusFilter = string.IsNullOrWhiteSpace(status) ? "Semua" : status,
                SortBy = string.IsNullOrWhiteSpace(sort) ? "Nama" : sort,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };

            return View("Guru", viewModel);
        }

        // POST: /Admin/ToggleStatusGuru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatusGuru(int id)
        {
            var guru = _db.Users.FirstOrDefault(u => u.IdUser == id && u.Role.NamaRole == "Guru");
            if (guru == null)
            {
                TempData["ErrorMessage"] = "Data akun guru tidak ditemukan.";
                return RedirectToAction("Guru");
            }

            guru.IsActive = !guru.IsActive;
            guru.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            string statusText = guru.IsActive ? "diaktifkan" : "dinonaktifkan";
            TempData["SuccessMessage"] = $"Status akun guru '{guru.NamaLengkap}' berhasil {statusText}.";

            return RedirectToAction("Guru");
        }

        // POST: /Admin/DeleteGuru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGuru(int id)
        {
            var guru = _db.Users.FirstOrDefault(u => u.IdUser == id && u.Role.NamaRole == "Guru");
            if (guru == null)
            {
                TempData["ErrorMessage"] = "Data akun guru tidak ditemukan.";
                return RedirectToAction("Guru");
            }

            // Cek apakah guru masih mengampu kelas aktif
            int activeClassesCount = _db.Kelas.Count(k => k.IdGuru == id && !k.IsDeleted);
            if (activeClassesCount > 0)
            {
                TempData["WarningMessage"] = $"Tidak dapat menghapus akun guru '{guru.NamaLengkap}' karena masih mengampu {activeClassesCount} kelas aktif. Silakan nonaktifkan akun terlebih dahulu.";
                return RedirectToAction("Guru");
            }

            // Soft Delete: Menonaktifkan akun & memperbarui timestamp
            guru.IsActive = false;
            guru.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Akun guru '{guru.NamaLengkap}' berhasil dihapus dari sistem.";
            return RedirectToAction("Guru");
        }

        // GET: /Admin/TambahGuru
        [HttpGet]
        public ActionResult TambahGuru()
        {
            ViewBag.Title = "Tambah Akun Pengajar (Guru) Baru";
            return View(new AdminTambahGuruViewModel());
        }

        // POST: /Admin/TambahGuru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TambahGuru(AdminTambahGuruViewModel model)
        {
            ViewBag.Title = "Tambah Akun Pengajar (Guru) Baru";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Cek duplikasi email di tabel Users
            string emailClean = model.Email.Trim().ToLower();
            if (_db.Users.Any(u => u.Email.ToLower() == emailClean))
            {
                ModelState.AddModelError("Email", "Alamat email sudah terdaftar di sistem. Gunakan email lain.");
                return View(model);
            }

            // 2. Lookup Role "Guru"
            var roleGuru = _db.Roles.FirstOrDefault(r => r.NamaRole == "Guru");
            int idRoleGuru = roleGuru != null ? roleGuru.IdRole : 2;

            // 3. Hash Password dengan BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // 4. Simpan ke tabel Users
            var newGuru = new Users
            {
                IdRole = idRoleGuru,
                NamaLengkap = model.NamaLengkap.Trim(),
                Email = emailClean,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(newGuru);
            _db.SaveChanges();

            // 5. Buat Record Profil di tabel Profiles
            var newProfile = new Profiles
            {
                IdUser = newGuru.IdUser,
                NoHp = string.IsNullOrWhiteSpace(model.NoTelepon) ? null : model.NoTelepon.Trim(),
                Bio = string.IsNullOrWhiteSpace(model.Spesialisasi) ? null : model.Spesialisasi.Trim(),
                Alamat = string.IsNullOrWhiteSpace(model.Alamat) ? null : model.Alamat.Trim(),
                UpdatedAt = DateTime.Now
            };

            _db.Profiles.Add(newProfile);

            // 6. Catat Notifikasi / Log Aktivitas Sistem
            int currentAdminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : newGuru.IdUser;
            var notifLog = new Notifikasi
            {
                IdUser = currentAdminId,
                Judul = "Penambahan Akun Guru Baru",
                Pesan = $"Admin membuat akun guru baru: {newGuru.NamaLengkap} ({newGuru.Email}).",
                TipeNotif = "Registrasi",
                UrlTujuan = "/Admin/Guru",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _db.Notifikasi.Add(notifLog);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Akun guru '{newGuru.NamaLengkap}' berhasil ditambahkan!";
            TempData["Success"] = $"Akun guru '{newGuru.NamaLengkap}' berhasil ditambahkan!";

            return RedirectToAction("Guru");
        }

        // GET: /Admin/Siswa
        [HttpGet]
        public ActionResult Siswa(string search, string status, string sort, int page = 1)
        {
            ViewBag.Title = "Manajemen Akun Siswa";

            int pageSize = 10;
            if (page < 1) page = 1;

            // Query dasar: Ambil pengguna dengan Role == "Siswa"
            var query = _db.Users
                .Include(u => u.Role)
                .Include(u => u.Profile)
                .Where(u => u.Role != null && u.Role.NamaRole == "Siswa");

            // Filter Pencarian (Nama / Email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(u => u.NamaLengkap.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
            }

            // Filter Status (Aktif / Nonaktif / Semua)
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("Semua", StringComparison.OrdinalIgnoreCase))
            {
                if (status.Equals("Aktif", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(u => u.IsActive == true);
                }
                else if (status.Equals("Nonaktif", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(u => u.IsActive == false);
                }
            }

            // Hitung Total Record untuk Pagination
            int totalRecords = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var siswaUsers = query.ToList();

            var siswaUserIds = siswaUsers.Select(s => s.IdUser).ToList();
            var enrolledCounts = _db.MemberKelas
                .Where(m => siswaUserIds.Contains(m.IdSiswa))
                .GroupBy(m => m.IdSiswa)
                .Select(g => new { IdSiswa = g.Key, Count = g.Count() })
                .ToList();

            var dtoList = siswaUsers.Select(u => new SiswaItemDto
            {
                UserId = u.IdUser,
                NamaLengkap = u.NamaLengkap,
                Email = u.Email,
                NoTelepon = u.Profile != null ? u.Profile.NoHp : "-",
                FotoProfile = u.FotoProfile,
                JumlahKelasDiikuti = enrolledCounts.FirstOrDefault(ec => ec.IdSiswa == u.IdUser)?.Count ?? 0,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).AsQueryable();

            // Sorting
            if (string.Equals(sort, "Tanggal", StringComparison.OrdinalIgnoreCase))
            {
                dtoList = dtoList.OrderByDescending(s => s.CreatedAt);
            }
            else if (string.Equals(sort, "Kelas", StringComparison.OrdinalIgnoreCase))
            {
                dtoList = dtoList.OrderByDescending(s => s.JumlahKelasDiikuti);
            }
            else
            {
                dtoList = dtoList.OrderBy(s => s.NamaLengkap);
            }

            // Pagination
            var pagedList = dtoList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new AdminSiswaListViewModel
            {
                DaftarSiswa = pagedList,
                SearchQuery = search,
                StatusFilter = string.IsNullOrWhiteSpace(status) ? "Semua" : status,
                SortBy = string.IsNullOrWhiteSpace(sort) ? "Nama" : sort,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };

            return View("Siswa", viewModel);
        }

        // POST: /Admin/ToggleStatusSiswa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatusSiswa(int id)
        {
            var siswa = _db.Users.FirstOrDefault(u => u.IdUser == id && u.Role.NamaRole == "Siswa");
            if (siswa == null)
            {
                TempData["ErrorMessage"] = "Data akun siswa tidak ditemukan.";
                return RedirectToAction("Siswa");
            }

            siswa.IsActive = !siswa.IsActive;
            siswa.UpdatedAt = DateTime.Now;

            // Catat log aktivitas
            int currentAdminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : siswa.IdUser;
            string statusText = siswa.IsActive ? "diaktifkan" : "dinonaktifkan";

            var notifLog = new Notifikasi
            {
                IdUser = currentAdminId,
                Judul = "Perubahan Status Akun Siswa",
                Pesan = $"Status akun siswa '{siswa.NamaLengkap}' ({siswa.Email}) telah {statusText}.",
                TipeNotif = "UserStatus",
                UrlTujuan = "/Admin/Siswa",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _db.Notifikasi.Add(notifLog);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Status akun siswa '{siswa.NamaLengkap}' berhasil {statusText}.";
            TempData["Success"] = $"Status akun siswa '{siswa.NamaLengkap}' berhasil {statusText}.";

            return RedirectToAction("Siswa");
        }

        // POST: /Admin/DeleteSiswa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSiswa(int id)
        {
            var siswa = _db.Users.FirstOrDefault(u => u.IdUser == id && u.Role.NamaRole == "Siswa");
            if (siswa == null)
            {
                TempData["ErrorMessage"] = "Data akun siswa tidak ditemukan.";
                return RedirectToAction("Siswa");
            }

            // Soft Delete: Menonaktifkan akun & memperbarui timestamp
            siswa.IsActive = false;
            siswa.UpdatedAt = DateTime.Now;

            // Catat log aktivitas
            int currentAdminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : siswa.IdUser;
            var notifLog = new Notifikasi
            {
                IdUser = currentAdminId,
                Judul = "Penghapusan Akun Siswa",
                Pesan = $"Akun siswa '{siswa.NamaLengkap}' ({siswa.Email}) telah dihapus (soft-delete) oleh Admin.",
                TipeNotif = "UserDelete",
                UrlTujuan = "/Admin/Siswa",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _db.Notifikasi.Add(notifLog);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Akun siswa '{siswa.NamaLengkap}' berhasil dihapus dari sistem.";
            TempData["Success"] = $"Akun siswa '{siswa.NamaLengkap}' berhasil dihapus dari sistem.";

            return RedirectToAction("Siswa");
        }

        // GET: /Admin/Kelas
        [HttpGet]
        public ActionResult Kelas(string search, string kategori, string status, string sort, int page = 1)
        {
            ViewBag.Title = "Manajemen & Monitoring Katalog Kelas";

            int pageSize = 10;
            if (page < 1) page = 1;

            // Query dasar: Semua Kelas
            var query = _db.Kelas
                .Include(k => k.Guru)
                .Include(k => k.Kategori)
                .AsQueryable();

            // Filter Status (Aktif / SoftDeleted / Semua)
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("Semua", StringComparison.OrdinalIgnoreCase))
            {
                if (status.Equals("Aktif", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(k => k.IsDeleted == false);
                }
                else if (status.Equals("SoftDeleted", StringComparison.OrdinalIgnoreCase) || status.Equals("Terhapus", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(k => k.IsDeleted == true);
                }
            }
            else if (string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(k => k.IsDeleted == false);
            }

            // Filter Kategori
            if (!string.IsNullOrWhiteSpace(kategori) && !kategori.Equals("Semua", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(k => k.Kategori != null && k.Kategori.NamaKategori.Equals(kategori, StringComparison.OrdinalIgnoreCase));
            }

            // Filter Search (Nama Kelas / Nama Guru)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(k => k.NamaKelas.ToLower().Contains(term) || (k.Guru != null && k.Guru.NamaLengkap.ToLower().Contains(term)));
            }

            // Hitung Total Record untuk Pagination
            int totalRecords = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var rawKelasList = query.ToList();
            var kelasIds = rawKelasList.Select(k => k.IdKelas).ToList();

            // Hitung Jumlah Siswa Enrolled
            var memberCounts = _db.MemberKelas
                .Where(m => kelasIds.Contains(m.IdKelas))
                .GroupBy(m => m.IdKelas)
                .Select(g => new { IdKelas = g.Key, Count = g.Count() })
                .ToList();

            // Hitung Jumlah Materi
            var materiCounts = _db.Materi
                .Where(m => !m.IsDeleted && kelasIds.Contains(m.IdKelas))
                .GroupBy(m => m.IdKelas)
                .Select(g => new { IdKelas = g.Key, Count = g.Count() })
                .ToList();

            // Hitung Jumlah Kuis & Tugas
            var quizCounts = _db.Quiz
                .Where(q => kelasIds.Contains(q.IdKelas))
                .GroupBy(q => q.IdKelas)
                .Select(g => new { IdKelas = g.Key, Count = g.Count() })
                .ToList();

            var tugasCounts = _db.Tugas
                .Where(t => !t.IsDeleted && kelasIds.Contains(t.IdKelas))
                .GroupBy(t => t.IdKelas)
                .Select(g => new { IdKelas = g.Key, Count = g.Count() })
                .ToList();

            var dtoList = rawKelasList.Select(k => new AdminKelasItemDto
            {
                KelasId = k.IdKelas,
                NamaKelas = k.NamaKelas,
                KodeKelas = $"KLS-{k.IdKelas:D4}",
                NamaKategori = k.Kategori != null ? k.Kategori.NamaKategori : "Umum",
                NamaGuruPengampu = k.Guru != null ? k.Guru.NamaLengkap : "Pengajar",
                ThumbnailUrl = k.Thumbnail,
                JumlahSiswa = memberCounts.FirstOrDefault(mc => mc.IdKelas == k.IdKelas)?.Count ?? 0,
                JumlahMateri = materiCounts.FirstOrDefault(mc => mc.IdKelas == k.IdKelas)?.Count ?? 0,
                JumlahTugas = (quizCounts.FirstOrDefault(qc => qc.IdKelas == k.IdKelas)?.Count ?? 0) +
                               (tugasCounts.FirstOrDefault(tc => tc.IdKelas == k.IdKelas)?.Count ?? 0),
                IsPublish = k.IsPublish,
                IsDeleted = k.IsDeleted,
                CreatedAt = k.CreatedAt
            }).AsQueryable();

            // Sorting
            if (string.Equals(sort, "Nama", StringComparison.OrdinalIgnoreCase))
            {
                dtoList = dtoList.OrderBy(k => k.NamaKelas);
            }
            else if (string.Equals(sort, "Siswa", StringComparison.OrdinalIgnoreCase))
            {
                dtoList = dtoList.OrderByDescending(k => k.JumlahSiswa);
            }
            else
            {
                dtoList = dtoList.OrderByDescending(k => k.CreatedAt);
            }

            // Pagination
            var pagedList = dtoList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Load Kategori Options
            var kategoriOptions = _db.Kategori
                .OrderBy(c => c.NamaKategori)
                .Select(c => new SelectListItem
                {
                    Value = c.NamaKategori,
                    Text = c.NamaKategori,
                    Selected = (c.NamaKategori == kategori)
                })
                .ToList();

            kategoriOptions.Insert(0, new SelectListItem { Value = "Semua", Text = "-- Semua Kategori --" });

            var viewModel = new AdminKelasListViewModel
            {
                DaftarKelas = pagedList,
                SearchQuery = search,
                KategoriFilter = string.IsNullOrWhiteSpace(kategori) ? "Semua" : kategori,
                StatusFilter = string.IsNullOrWhiteSpace(status) ? "Aktif" : status,
                SortBy = string.IsNullOrWhiteSpace(sort) ? "Tanggal" : sort,
                KategoriOptions = kategoriOptions,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };

            return View("Kelas", viewModel);
        }

        // POST: /Admin/SoftDeleteKelas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SoftDeleteKelas(int id)
        {
            var kelas = _db.Kelas.FirstOrDefault(k => k.IdKelas == id);
            if (kelas == null)
            {
                TempData["ErrorMessage"] = "Data kelas tidak ditemukan.";
                return RedirectToAction("Kelas");
            }

            kelas.IsDeleted = true;
            kelas.DeletedAt = DateTime.Now;

            // Catat audit log
            int currentAdminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : kelas.IdGuru;
            var notifLog = new Notifikasi
            {
                IdUser = currentAdminId,
                Judul = "Soft Delete Kelas",
                Pesan = $"Admin menonaktifkan/soft-delete kelas: '{kelas.NamaKelas}'.",
                TipeNotif = "KelasDelete",
                UrlTujuan = "/Admin/Kelas",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _db.Notifikasi.Add(notifLog);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Kelas '{kelas.NamaKelas}' berhasil dinonaktifkan (Soft Delete).";
            TempData["Success"] = $"Kelas '{kelas.NamaKelas}' berhasil dinonaktifkan (Soft Delete).";

            return RedirectToAction("Kelas");
        }

        // POST: /Admin/RestoreKelas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreKelas(int id)
        {
            var kelas = _db.Kelas.FirstOrDefault(k => k.IdKelas == id);
            if (kelas == null)
            {
                TempData["ErrorMessage"] = "Data kelas tidak ditemukan.";
                return RedirectToAction("Kelas");
            }

            kelas.IsDeleted = false;
            kelas.DeletedAt = null;
            kelas.UpdatedAt = DateTime.Now;

            // Catat audit log
            int currentAdminId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : kelas.IdGuru;
            var notifLog = new Notifikasi
            {
                IdUser = currentAdminId,
                Judul = "Pulihkan Kelas",
                Pesan = $"Admin memulihkan (restore) kelas: '{kelas.NamaKelas}'.",
                TipeNotif = "KelasRestore",
                UrlTujuan = "/Admin/Kelas",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _db.Notifikasi.Add(notifLog);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Kelas '{kelas.NamaKelas}' berhasil dipulihkan dan aktif kembali!";
            TempData["Success"] = $"Kelas '{kelas.NamaKelas}' berhasil dipulihkan dan aktif kembali!";

            return RedirectToAction("Kelas");
        }

        // GET: /Admin/Laporan
        [HttpGet]
        public ActionResult Laporan()
        {
            ViewBag.Title = "Laporan Analitik & Export Platform";

            var model = new AdminLaporanViewModel();
            model.TotalGuru = _db.Users.Count(u => u.Role != null && u.Role.NamaRole == "Guru" && u.IsActive);
            model.TotalSiswa = _db.Users.Count(u => u.Role != null && u.Role.NamaRole == "Siswa" && u.IsActive);
            model.TotalKelasAktif = _db.Kelas.Count(k => k.IsPublish && !k.IsDeleted);
            model.TotalModulPembelajaran = _db.Materi.Count(m => !m.IsDeleted);

            int totalQuiz = _db.Quiz.Count();
            int totalTugas = _db.Tugas.Count(t => !t.IsDeleted);
            model.TotalKuisTugas = totalQuiz + totalTugas;

            int totalSiswaEnrolled = _db.MemberKelas.Select(m => m.IdSiswa).Distinct().Count();
            model.TingkatPartisipasiSiswa = model.TotalSiswa > 0
                ? Math.Round((double)totalSiswaEnrolled / model.TotalSiswa * 100, 1)
                : 0;

            var kategoriList = _db.Kategori.ToList();
            var activeKelas = _db.Kelas.Where(k => !k.IsDeleted).ToList();
            var members = _db.MemberKelas.ToList();

            model.RingkasanKategori = kategoriList.Select(cat => new LaporanKategoriDto
            {
                NamaKategori = cat.NamaKategori,
                TotalKelas = activeKelas.Count(k => k.IdKategori == cat.IdKategori),
                TotalSiswa = members.Count(m => activeKelas.Where(k => k.IdKategori == cat.IdKategori).Select(k => k.IdKelas).Contains(m.IdKelas))
            }).ToList();

            return View("Laporan", model);
        }

        // GET: /Admin/ExportLaporanGuru
        [HttpGet]
        public ActionResult ExportLaporanGuru()
        {
            var guruList = _db.Users
                .Include(u => u.Profile)
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.NamaRole == "Guru")
                .OrderBy(u => u.NamaLengkap)
                .ToList();

            var guruIds = guruList.Select(g => g.IdUser).ToList();
            var kelasCounts = _db.Kelas
                .Where(k => !k.IsDeleted && guruIds.Contains(k.IdGuru))
                .GroupBy(k => k.IdGuru)
                .Select(g => new { IdGuru = g.Key, Count = g.Count() })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Laporan Data Guru");

                // Header Title
                ws.Cells["A1"].Value = "ADINATA LMS - LAPORAN DATA PENGAJAR (GURU)";
                ws.Cells["A1"].Style.Font.Size = 14;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A2"].Value = $"Tanggal Export: {DateTime.Now:dd MMMM yyyy HH:mm} WIB";
                ws.Cells["A2"].Style.Font.Italic = true;

                // Table Headers
                string[] headers = { "No", "ID User", "Nama Lengkap", "Email", "No. Telepon / WA", "Gelar / Spesialisasi", "Jumlah Kelas Diampu", "Status Akun", "Tanggal Bergabung" };
                for (int col = 0; col < headers.Length; col++)
                {
                    var cell = ws.Cells[4, col + 1];
                    cell.Value = headers[col];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(37, 99, 235)); // Primary Blue
                    cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Table Rows
                int row = 5;
                for (int i = 0; i < guruList.Count; i++)
                {
                    var g = guruList[i];
                    int totalKelas = kelasCounts.FirstOrDefault(kc => kc.IdGuru == g.IdUser)?.Count ?? 0;

                    ws.Cells[row, 1].Value = i + 1;
                    ws.Cells[row, 2].Value = g.IdUser;
                    ws.Cells[row, 3].Value = g.NamaLengkap;
                    ws.Cells[row, 4].Value = g.Email;
                    ws.Cells[row, 5].Value = g.Profile != null ? g.Profile.NoHp : "-";
                    ws.Cells[row, 6].Value = g.Profile != null ? g.Profile.Bio : "-";
                    ws.Cells[row, 7].Value = totalKelas;
                    ws.Cells[row, 8].Value = g.IsActive ? "Aktif" : "Nonaktif";
                    ws.Cells[row, 9].Value = g.CreatedAt.ToString("yyyy-MM-dd HH:mm");

                    for (int col = 1; col <= headers.Length; col++)
                    {
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    row++;
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                var stream = new System.IO.MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Laporan_Data_Guru_AdinataLMS.xlsx");
            }
        }

        // GET: /Admin/ExportLaporanSiswa
        [HttpGet]
        public ActionResult ExportLaporanSiswa()
        {
            var siswaList = _db.Users
                .Include(u => u.Profile)
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.NamaRole == "Siswa")
                .OrderBy(u => u.NamaLengkap)
                .ToList();

            var siswaIds = siswaList.Select(s => s.IdUser).ToList();
            var enrolledCounts = _db.MemberKelas
                .Where(m => siswaIds.Contains(m.IdSiswa))
                .GroupBy(m => m.IdSiswa)
                .Select(g => new { IdSiswa = g.Key, Count = g.Count() })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Laporan Data Siswa");

                ws.Cells["A1"].Value = "ADINATA LMS - LAPORAN DATA SISWA / PESERTA DIDIK";
                ws.Cells["A1"].Style.Font.Size = 14;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A2"].Value = $"Tanggal Export: {DateTime.Now:dd MMMM yyyy HH:mm} WIB";
                ws.Cells["A2"].Style.Font.Italic = true;

                string[] headers = { "No", "ID User", "Nama Lengkap", "Email", "No. Telepon / WA", "Jumlah Kelas Diikuti", "Status Akun", "Tanggal Registrasi" };
                for (int col = 0; col < headers.Length; col++)
                {
                    var cell = ws.Cells[4, col + 1];
                    cell.Value = headers[col];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(16, 185, 129)); // Green
                    cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                int row = 5;
                for (int i = 0; i < siswaList.Count; i++)
                {
                    var s = siswaList[i];
                    int totalKelas = enrolledCounts.FirstOrDefault(ec => ec.IdSiswa == s.IdUser)?.Count ?? 0;

                    ws.Cells[row, 1].Value = i + 1;
                    ws.Cells[row, 2].Value = s.IdUser;
                    ws.Cells[row, 3].Value = s.NamaLengkap;
                    ws.Cells[row, 4].Value = s.Email;
                    ws.Cells[row, 5].Value = s.Profile != null ? s.Profile.NoHp : "-";
                    ws.Cells[row, 6].Value = totalKelas;
                    ws.Cells[row, 7].Value = s.IsActive ? "Aktif" : "Nonaktif";
                    ws.Cells[row, 8].Value = s.CreatedAt.ToString("yyyy-MM-dd HH:mm");

                    for (int col = 1; col <= headers.Length; col++)
                    {
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    row++;
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                var stream = new System.IO.MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Laporan_Data_Siswa_AdinataLMS.xlsx");
            }
        }

        // GET: /Admin/ExportLaporanKelas
        [HttpGet]
        public ActionResult ExportLaporanKelas()
        {
            var kelasList = _db.Kelas
                .Include(k => k.Guru)
                .Include(k => k.Kategori)
                .OrderByDescending(k => k.CreatedAt)
                .ToList();

            var kelasIds = kelasList.Select(k => k.IdKelas).ToList();

            var memberCounts = _db.MemberKelas
                .Where(m => kelasIds.Contains(m.IdKelas))
                .GroupBy(m => m.IdKelas)
                .Select(g => new { IdKelas = g.Key, Count = g.Count() })
                .ToList();

            var materiCounts = _db.Materi
                .Where(m => !m.IsDeleted && kelasIds.Contains(m.IdKelas))
                .GroupBy(m => m.IdKelas)
                .Select(g => new { IdKelas = g.Key, Count = g.Count() })
                .ToList();

            var tugasCounts = _db.Tugas
                .Where(t => !t.IsDeleted && kelasIds.Contains(t.IdKelas))
                .GroupBy(t => t.IdKelas)
                .Select(g => new { IdKelas = g.Key, Count = g.Count() })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Laporan Katalog Kelas");

                ws.Cells["A1"].Value = "ADINATA LMS - LAPORAN KATALOG KELAS PLATFORM";
                ws.Cells["A1"].Style.Font.Size = 14;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A2"].Value = $"Tanggal Export: {DateTime.Now:dd MMMM yyyy HH:mm} WIB";
                ws.Cells["A2"].Style.Font.Italic = true;

                string[] headers = { "No", "Kode Kelas", "Nama Kelas", "Kategori", "Guru Pengampu", "Jumlah Siswa", "Jumlah Modul Materi", "Jumlah Tugas", "Status Kelas", "Tanggal Dibuat" };
                for (int col = 0; col < headers.Length; col++)
                {
                    var cell = ws.Cells[4, col + 1];
                    cell.Value = headers[col];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(139, 92, 246)); // Purple
                    cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                int row = 5;
                for (int i = 0; i < kelasList.Count; i++)
                {
                    var k = kelasList[i];
                    int siswaCount = memberCounts.FirstOrDefault(mc => mc.IdKelas == k.IdKelas)?.Count ?? 0;
                    int materiCount = materiCounts.FirstOrDefault(mc => mc.IdKelas == k.IdKelas)?.Count ?? 0;
                    int tugasCount = tugasCounts.FirstOrDefault(tc => tc.IdKelas == k.IdKelas)?.Count ?? 0;

                    ws.Cells[row, 1].Value = i + 1;
                    ws.Cells[row, 2].Value = $"KLS-{k.IdKelas:D4}";
                    ws.Cells[row, 3].Value = k.NamaKelas;
                    ws.Cells[row, 4].Value = k.Kategori != null ? k.Kategori.NamaKategori : "Umum";
                    ws.Cells[row, 5].Value = k.Guru != null ? k.Guru.NamaLengkap : "Pengajar";
                    ws.Cells[row, 6].Value = siswaCount;
                    ws.Cells[row, 7].Value = materiCount;
                    ws.Cells[row, 8].Value = tugasCount;
                    ws.Cells[row, 9].Value = k.IsDeleted ? "Terhapus (Soft-Delete)" : (k.IsPublish ? "Aktif & Publik" : "Draft");
                    ws.Cells[row, 10].Value = k.CreatedAt.ToString("yyyy-MM-dd HH:mm");

                    for (int col = 1; col <= headers.Length; col++)
                    {
                        ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    row++;
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                var stream = new System.IO.MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Laporan_Katalog_Kelas_AdinataLMS.xlsx");
            }
        }

        // GET: /Admin/Notifikasi
        [HttpGet]
        public ActionResult Notifikasi()
        {
            ViewBag.Title = "Riwayat Notifikasi & Aktivitas Sistem";

            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;

            var notifList = _db.Notifikasi
                .Where(n => n.IdUser == currentUserId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            var viewModel = new AdminNotifikasiViewModel
            {
                DaftarNotifikasi = notifList.Select(n => new AdminNotifikasiItemDto
                {
                    IdNotifikasi = n.IdNotif,
                    Judul = n.Judul,
                    Pesan = n.Pesan,
                    TipeNotif = n.TipeNotif,
                    UrlTujuan = n.UrlTujuan,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToList(),
                UnreadCount = notifList.Count(n => !n.IsRead)
            };

            return View("Notifikasi", viewModel);
        }

        // POST: /Admin/MarkNotifikasiRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkNotifikasiRead(int id)
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            var notif = _db.Notifikasi.FirstOrDefault(n => n.IdNotif == id && n.IdUser == currentUserId);
            if (notif != null)
            {
                notif.IsRead = true;
                _db.SaveChanges();
            }

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Notifikasi");
        }

        // POST: /Admin/MarkAllNotifikasiRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAllNotifikasiRead()
        {
            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            var unreadList = _db.Notifikasi.Where(n => n.IdUser == currentUserId && !n.IsRead).ToList();

            foreach (var item in unreadList)
            {
                item.IsRead = true;
            }

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Seluruh notifikasi telah ditandai dibaca.";
            TempData["Success"] = "Seluruh notifikasi telah ditandai dibaca.";

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Notifikasi");
        }

        // GET: /Admin/Profile
        [HttpGet]
        public new ActionResult Profile()
        {
            ViewBag.Title = "Edit Profil Administrator";

            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            var user = _db.Users
                .Include(u => u.Profile)
                .FirstOrDefault(u => u.IdUser == currentUserId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Data pengguna tidak ditemukan.";
                return RedirectToAction("Dashboard");
            }

            var viewModel = new AdminProfileViewModel
            {
                UserId = user.IdUser,
                NamaLengkap = user.NamaLengkap,
                Email = user.Email,
                NoTelepon = user.Profile != null ? user.Profile.NoHp : "",
                Bio = user.Profile != null ? user.Profile.Bio : "",
                Alamat = user.Profile != null ? user.Profile.Alamat : "",
                FotoUrl = user.FotoProfile
            };

            return View("Profile", viewModel);
        }

        // POST: /Admin/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(AdminProfileViewModel model)
        {
            ViewBag.Title = "Edit Profil Administrator";

            int currentUserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
            var user = _db.Users
                .Include(u => u.Profile)
                .FirstOrDefault(u => u.IdUser == currentUserId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Data akun pengguna tidak ditemukan.";
                return RedirectToAction("Dashboard");
            }

            if (!ModelState.IsValid)
            {
                model.FotoUrl = user.FotoProfile;
                return View("Profile", model);
            }

            // Cek duplikasi email jika email diubah
            if (!user.Email.Equals(model.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                string cleanEmail = model.Email.Trim().ToLower();
                bool isDuplicate = _db.Users.Any(u => u.Email.ToLower() == cleanEmail && u.IdUser != currentUserId);
                if (isDuplicate)
                {
                    ModelState.AddModelError("Email", "Alamat email ini sudah terdaftar di sistem.");
                    model.FotoUrl = user.FotoProfile;
                    return View("Profile", model);
                }
                user.Email = cleanEmail;
                Session["Email"] = cleanEmail;
            }

            user.NamaLengkap = model.NamaLengkap.Trim();
            Session["NamaLengkap"] = user.NamaLengkap;

            // Handle foto profile upload
            if (model.FotoUpload != null && model.FotoUpload.ContentLength > 0)
            {
                try
                {
                    string ext = System.IO.Path.GetExtension(model.FotoUpload.FileName).ToLower();
                    string[] allowedExts = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                    if (allowedExts.Contains(ext))
                    {
                        string fileName = $"admin_{user.IdUser}_{Guid.NewGuid():N}{ext}";
                        string folderPath = Server.MapPath("~/Content/images/profiles/");

                        if (!System.IO.Directory.Exists(folderPath))
                        {
                            System.IO.Directory.CreateDirectory(folderPath);
                        }

                        string savePath = System.IO.Path.Combine(folderPath, fileName);
                        model.FotoUpload.SaveAs(savePath);

                        user.FotoProfile = $"/Content/images/profiles/{fileName}";
                        Session["FotoProfile"] = user.FotoProfile;
                    }
                    else
                    {
                        ModelState.AddModelError("FotoUpload", "Format file foto harus .jpg, .jpeg, .png, .gif, atau .webp.");
                        model.FotoUrl = user.FotoProfile;
                        return View("Profile", model);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Gagal mengunggah foto profil: " + ex.Message);
                    model.FotoUrl = user.FotoProfile;
                    return View("Profile", model);
                }
            }

            // Handle update profile details
            if (user.Profile == null)
            {
                user.Profile = new Profiles
                {
                    IdUser = user.IdUser,
                    UpdatedAt = DateTime.Now
                };
                _db.Profiles.Add(user.Profile);
            }

            user.Profile.NoHp = model.NoTelepon != null ? model.NoTelepon.Trim() : null;
            user.Profile.Bio = model.Bio != null ? model.Bio.Trim() : null;
            user.Profile.Alamat = model.Alamat != null ? model.Alamat.Trim() : null;
            user.Profile.UpdatedAt = DateTime.Now;

            // Handle Ganti Password jika diisi
            if (!string.IsNullOrWhiteSpace(model.KataSandiBaru))
            {
                if (string.IsNullOrWhiteSpace(model.KataSandiLama))
                {
                    ModelState.AddModelError("KataSandiLama", "Password lama wajib diisi untuk mengubah password.");
                    model.FotoUrl = user.FotoProfile;
                    return View("Profile", model);
                }

                bool isOldPasswordValid = false;
                try
                {
                    isOldPasswordValid = BCrypt.Net.BCrypt.Verify(model.KataSandiLama, user.PasswordHash);
                }
                catch
                {
                    isOldPasswordValid = false;
                }

                if (!isOldPasswordValid && string.Equals(model.KataSandiLama, user.PasswordHash, StringComparison.Ordinal))
                {
                    isOldPasswordValid = true;
                }

                if (!isOldPasswordValid)
                {
                    ModelState.AddModelError("KataSandiLama", "Password lama yang Anda masukkan salah.");
                    model.FotoUrl = user.FotoProfile;
                    return View("Profile", model);
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.KataSandiBaru);
            }

            user.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Profil Administrator berhasil diperbarui!";
            TempData["Success"] = "Profil Administrator berhasil diperbarui!";

            return RedirectToAction("Profile");
        }

        // GET: /Admin/LogAktivitas (Placeholder)
        [HttpGet]
        public ActionResult LogAktivitas()
        {
            ViewBag.Title = "Log Audit Aktivitas Sistem";
            return View("Dashboard", new AdminDashboardViewModel());
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
