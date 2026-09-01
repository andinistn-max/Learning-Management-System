using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly LmsDbContext _db = new LmsDbContext();

        // GET: / or /Home/Index
        public ActionResult Index()
        {
            ViewBag.Title = "EduPulse LMS - Platform Pembelajaran Digital Interaktif";
            ViewBag.IsPublicPage = true;

            var viewModel = new LandingPageViewModel
            {
                TotalKelas = _db.Kelas.Count(k => k.IsPublish && !k.IsDeleted),
                TotalGuru = _db.Users.Count(u => u.Role.NamaRole == "Guru" && u.IsActive),
                TotalSiswa = _db.Users.Count(u => u.Role.NamaRole == "Siswa" && u.IsActive),
                TotalKategori = _db.Kategori.Count(),

                KategoriList = _db.Kategori
                    .Select(k => new KategoriDto
                    {
                        IdKategori = k.IdKategori,
                        NamaKategori = k.NamaKategori,
                        Deskripsi = k.Deskripsi,
                        TotalKelas = _db.Kelas.Count(c => c.IdKategori == k.IdKategori && c.IsPublish && !c.IsDeleted)
                    })
                    .OrderByDescending(k => k.TotalKelas)
                    .Take(6)
                    .ToList(),

                KelasPopuler = _db.Kelas
                    .Include(k => k.Kategori)
                    .Include(k => k.Guru)
                    .Include(k => k.Guru.Profile)
                    .Where(k => k.IsPublish && !k.IsDeleted)
                    .OrderByDescending(k => _db.MemberKelas.Count(m => m.IdKelas == k.IdKelas))
                    .Take(6)
                    .Select(k => new KelasPreviewDto
                    {
                        IdKelas = k.IdKelas,
                        NamaKelas = k.NamaKelas,
                        Deskripsi = k.Deskripsi,
                        Thumbnail = k.Thumbnail,
                        Level = k.Level,
                        Durasi = k.Durasi,
                        NamaKategori = k.Kategori != null ? k.Kategori.NamaKategori : "-",
                        NamaGuru = k.Guru != null ? k.Guru.NamaLengkap : "-",
                        FotoGuru = k.Guru != null ? k.Guru.FotoProfile : null,
                        TotalSiswa = _db.MemberKelas.Count(m => m.IdKelas == k.IdKelas)
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: /Home/About
        public ActionResult About()
        {
            ViewBag.Title = "Tentang EduPulse LMS";
            ViewBag.IsPublicPage = true;
            ViewBag.Message = "EduPulse LMS adalah platform Learning Management System interaktif berteknologi tinggi.";

            ViewBag.TotalKelas = _db.Kelas.Count(k => k.IsPublish && !k.IsDeleted);
            ViewBag.TotalGuru = _db.Users.Count(u => u.Role.NamaRole == "Guru" && u.IsActive);
            ViewBag.TotalSiswa = _db.Users.Count(u => u.Role.NamaRole == "Siswa" && u.IsActive);

            return View();
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
