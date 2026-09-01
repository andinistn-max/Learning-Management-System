using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using BCrypt.Net;
using Learning_Management_System.Models.Entity;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Controllers
{
    public class ProfileController : Controller
    {
        private readonly LmsDbContext _db = new LmsDbContext();

        // Helper untuk mendapatkan UserId dari Session
        private int? GetCurrentUserId()
        {
            if (Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out int userId))
            {
                return userId;
            }
            return null;
        }

        // GET: /Profile or /Profile/Index
        [HttpGet]
        public ActionResult Index()
        {
            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                TempData["ErrorMessage"] = "Silakan login terlebih dahulu untuk mengakses halaman profil.";
                return RedirectToAction("Login", "Account");
            }

            var user = _db.Users
                .Include(u => u.Role)
                .Include(u => u.Profile)
                .FirstOrDefault(u => u.IdUser == currentUserId.Value);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Pengguna tidak ditemukan.";
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new ProfileViewModel
            {
                UserId = user.IdUser,
                NamaLengkap = user.NamaLengkap,
                Email = user.Email,
                FotoUrl = user.FotoProfile,
                NoTelepon = user.Profile != null ? user.Profile.NoHp : null,
                Bio = user.Profile != null ? user.Profile.Bio : null,
                Alamat = user.Profile != null ? user.Profile.Alamat : null,
                JenisKelamin = user.Profile != null ? user.Profile.JenisKelamin : null
            };

            ViewBag.Title = "Pengaturan Profil & Keamanan - EduPulse LMS";
            ViewBag.UserRole = user.Role != null ? user.Role.NamaRole : "Siswa";

            return View(viewModel);
        }

        // POST: /Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileViewModel model)
        {
            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                TempData["ErrorMessage"] = "Sesi Anda telah berakhir. Silakan login kembali.";
                return RedirectToAction("Login", "Account");
            }

            var user = _db.Users
                .Include(u => u.Profile)
                .FirstOrDefault(u => u.IdUser == currentUserId.Value);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Pengguna tidak ditemukan.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Mohon periksa kembali inputan data profil Anda.";
                return View("Index", model);
            }

            // 1. Update nama lengkap pada tabel Users
            user.NamaLengkap = model.NamaLengkap.Trim();

            // 2. Handling Upload Foto Profil baru jika ada
            if (model.FotoUpload != null && model.FotoUpload.ContentLength > 0)
            {
                try
                {
                    string fileExtension = Path.GetExtension(model.FotoUpload.FileName).ToLower();
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

                    if (allowedExtensions.Contains(fileExtension))
                    {
                        string fileName = $"avatar_{user.IdUser}_{DateTime.Now.Ticks}{fileExtension}";
                        string targetFolder = Server.MapPath("~/Content/uploads/avatars/");

                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        string path = Path.Combine(targetFolder, fileName);
                        model.FotoUpload.SaveAs(path);

                        string relativePath = $"/Content/uploads/avatars/{fileName}";
                        user.FotoProfile = relativePath;
                        Session["FotoProfile"] = relativePath;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Format file foto tidak didukung (Gunakan .jpg, .jpeg, .png, atau .webp).";
                        return View("Index", model);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Gagal mengunggah foto profil: " + ex.Message;
                    return View("Index", model);
                }
            }

            // 3. Update nomor telepon, bio, alamat, dan jenis kelamin pada tabel Profiles
            if (user.Profile == null)
            {
                user.Profile = new Profiles
                {
                    IdUser = user.IdUser
                };
                _db.Profiles.Add(user.Profile);
            }

            user.Profile.NoHp = !string.IsNullOrWhiteSpace(model.NoTelepon) ? model.NoTelepon.Trim() : null;
            user.Profile.Bio = !string.IsNullOrWhiteSpace(model.Bio) ? model.Bio.Trim() : null;
            user.Profile.Alamat = !string.IsNullOrWhiteSpace(model.Alamat) ? model.Alamat.Trim() : null;
            user.Profile.JenisKelamin = !string.IsNullOrWhiteSpace(model.JenisKelamin) ? model.JenisKelamin.Trim() : null;
            user.Profile.UpdatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;

            _db.SaveChanges();

            // Update Session Name
            Session["NamaLengkap"] = user.NamaLengkap;

            TempData["SuccessMessage"] = "Profil Anda berhasil diperbarui!";
            return RedirectToAction("Index");
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            int? currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                TempData["ErrorMessage"] = "Sesi Anda telah berakhir. Silakan login kembali.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Mohon lengkapi seluruh kolom kata sandi dengan benar.";
                return RedirectToAction("Index");
            }

            var user = _db.Users.FirstOrDefault(u => u.IdUser == currentUserId.Value);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Pengguna tidak ditemukan.";
                return RedirectToAction("Login", "Account");
            }

            // 1. Verifikasi kecocokan kata sandi lama via BCrypt
            bool isOldPasswordValid = false;
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                try
                {
                    isOldPasswordValid = BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash);
                }
                catch
                {
                    isOldPasswordValid = false;
                }
            }

            if (!isOldPasswordValid)
            {
                TempData["ErrorMessage"] = "Kata sandi lama yang Anda masukkan tidak cocok.";
                return RedirectToAction("Index");
            }

            // 2. Hash kata sandi baru dan simpan ke database Users
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.Now;

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Kata sandi Anda berhasil diperbarui!";
            return RedirectToAction("Index");
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
