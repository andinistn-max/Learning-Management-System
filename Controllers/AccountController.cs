using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using BCrypt.Net;
using Google.Apis.Auth;
using Learning_Management_System.Models.Entity;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly LmsDbContext _db = new LmsDbContext();

        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            ViewBag.Title = "Daftar Akun Baru - EduPulse LMS";
            ViewBag.IsPublicPage = true;
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            ViewBag.Title = "Daftar Akun Baru - EduPulse LMS";
            ViewBag.IsPublicPage = true;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string cleanEmail = model.Email.Trim().ToLower();

            // 1. Cek duplikasi email pada tabel Users
            if (_db.Users.Any(u => u.Email.ToLower() == cleanEmail))
            {
                ModelState.AddModelError("Email", "Email ini sudah terdaftar. Silakan gunakan email lain atau masuk ke akun Anda.");
                return View(model);
            }

            // 2. Cari Role yang dipilih (Guru / Siswa)
            string selectedRoleName = (model.Role == "Guru") ? "Guru" : "Siswa";
            var role = _db.Roles.FirstOrDefault(r => r.NamaRole == selectedRoleName);

            if (role == null)
            {
                role = _db.Roles.FirstOrDefault(r => r.NamaRole == "Siswa");
                if (role == null)
                {
                    ModelState.AddModelError("", "Peran sistem (Role) belum terkonfigurasi di database.");
                    return View(model);
                }
            }

            // 3. Hash password menggunakan BCrypt.Net
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // 4. Simpan akun baru ke tabel Users dengan IsActive = true
            var newUser = new Users
            {
                NamaLengkap = model.NamaLengkap.Trim(),
                Email = cleanEmail,
                PasswordHash = passwordHash,
                IdRole = role.IdRole,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            // 5. Otomatis buat data awal di tabel Profiles berelasi dengan UserId baru
            var newProfile = new Profiles
            {
                IdUser = newUser.IdUser,
                NoHp = null,
                Bio = $"Pengguna baru {selectedRoleName} EduPulse LMS.",
                Alamat = null,
                JenisKelamin = null,
                UpdatedAt = DateTime.Now
            };

            _db.Profiles.Add(newProfile);
            _db.SaveChanges();

            // 6. Berikan pesan sukses via TempData dan redirect ke /Account/Login
            TempData["SuccessMessage"] = "Registrasi akun berhasil! Silakan masuk menggunakan email dan password Anda.";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.Title = "Masuk Ke Akun - EduPulse LMS";
            ViewBag.IsPublicPage = true;
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.Title = "Masuk Ke Akun - EduPulse LMS";
            ViewBag.IsPublicPage = true;
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string cleanEmail = model.Email.Trim().ToLower();

            // 1. Cek apakah email terdaftar di database Users
            var user = _db.Users
                .Include(u => u.Role)
                .Include(u => u.Profile)
                .FirstOrDefault(u => u.Email.ToLower() == cleanEmail);

            if (user == null)
            {
                ModelState.AddModelError("", "Email atau password yang Anda masukkan salah.");
                return View(model);
            }

            // 2. Cek apakah status IsActive == true (jika nonaktif, kembalikan pesan error)
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Akun Anda sedang dinonaktifkan oleh Administrator. Silakan hubungi pusat bantuan.");
                return View(model);
            }

            // 3. Verifikasi kecocokan password menggunakan BCrypt.Net.BCrypt.Verify()
            bool isPasswordValid = false;
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                }
                catch
                {
                    isPasswordValid = false;
                }

                // Fallback check jika password di DB berupa plaintext
                if (!isPasswordValid && string.Equals(model.Password, user.PasswordHash, StringComparison.Ordinal))
                {
                    isPasswordValid = true;
                }
            }

            if (!isPasswordValid)
            {
                ModelState.AddModelError("", "Email atau password yang Anda masukkan salah.");
                return View(model);
            }

            // 4. Jika valid: Set Session (UserId, NamaLengkap, Role, FotoProfile), perbarui LastLogin/UpdatedAt
            string roleName = user.Role != null ? user.Role.NamaRole : "Siswa";

            Session["UserId"] = user.IdUser;
            Session["NamaLengkap"] = user.NamaLengkap;
            Session["Email"] = user.Email;
            Session["Role"] = roleName;
            Session["FotoProfile"] = user.FotoProfile;

            user.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            // 5. Buat autentikasi cookie/FormsAuthentication
            FormsAuthentication.SetAuthCookie(user.Email, model.RememberMe);

            // 6. Redirect sesuai returnUrl atau Role pengguna
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (roleName.Equals("Guru", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Guru");
            }
            else
            {
                return RedirectToAction("Index", "Profile");
            }
        }

        // POST: /Account/GoogleLogin
        [HttpPost]
        public async Task<ActionResult> GoogleLogin(string credential)
        {
            if (string.IsNullOrWhiteSpace(credential))
            {
                return Json(new { success = false, message = "Token credential Google tidak valid." });
            }

            GoogleJsonWebSignature.Payload payload = null;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(credential);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gagal memverifikasi akun Google: " + ex.Message });
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
            {
                return Json(new { success = false, message = "Informasi email tidak ditemukan pada akun Google Anda." });
            }

            string cleanEmail = payload.Email.Trim().ToLower();
            string googleId = payload.Subject;
            string fullName = !string.IsNullOrWhiteSpace(payload.Name) ? payload.Name.Trim() : cleanEmail;
            string photoUrl = payload.Picture;

            // 1. Cek apakah email sudah ada di tabel Users
            var user = _db.Users
                .Include(u => u.Role)
                .Include(u => u.Profile)
                .FirstOrDefault(u => u.Email.ToLower() == cleanEmail);

            if (user != null)
            {
                // Cek status IsActive
                if (!user.IsActive)
                {
                    return Json(new { success = false, message = "Akun Anda sedang dinonaktifkan oleh Administrator. Silakan hubungi pusat bantuan." });
                }

                // Update GoogleId dan FotoProfile jika belum ada
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleId;
                }
                if (!string.IsNullOrEmpty(photoUrl))
                {
                    user.FotoProfile = photoUrl;
                }
                user.IsEmailVerified = true;
                user.UpdatedAt = DateTime.Now;
                _db.SaveChanges();
            }
            else
            {
                // 2. Jika email belum terdaftar (Pengguna Baru): Otomatis buat record baru dengan Role = "Siswa" (default)
                var siswaRole = _db.Roles.FirstOrDefault(r => r.NamaRole == "Siswa");
                int idRole = siswaRole != null ? siswaRole.IdRole : 3;

                user = new Users
                {
                    NamaLengkap = fullName,
                    Email = cleanEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                    GoogleId = googleId,
                    FotoProfile = photoUrl,
                    IdRole = idRole,
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _db.Users.Add(user);
                _db.SaveChanges();

                // Otomatis buat Profile baru
                var newProfile = new Profiles
                {
                    IdUser = user.IdUser,
                    Bio = "Pengguna Siswa EduPulse LMS (Google Account).",
                    UpdatedAt = DateTime.Now
                };
                _db.Profiles.Add(newProfile);
                _db.SaveChanges();

                // Reload user relation
                user = _db.Users.Include(u => u.Role).Include(u => u.Profile).FirstOrDefault(u => u.IdUser == user.IdUser);
            }

            // 3. Set Session login
            string roleName = user.Role != null ? user.Role.NamaRole : "Siswa";

            Session["UserId"] = user.IdUser;
            Session["NamaLengkap"] = user.NamaLengkap;
            Session["Email"] = user.Email;
            Session["Role"] = roleName;
            Session["FotoProfile"] = user.FotoProfile;

            // 4. Buat cookie autentikasi
            FormsAuthentication.SetAuthCookie(user.Email, true);

            // 5. Tentukan target redirect URL sesuai Role
            string redirectUrl = Url.Action("Index", "Profile");
            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                redirectUrl = Url.Action("Dashboard", "Admin");
            }

            return Json(new { success = true, redirectUrl = redirectUrl, message = "Login Google berhasil!" });
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            ViewBag.Title = "Lupa Password - EduPulse LMS";
            ViewBag.IsPublicPage = true;
            return View(new ForgotPasswordViewModel());
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            ViewBag.Title = "Lupa Password - EduPulse LMS";
            ViewBag.IsPublicPage = true;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string cleanEmail = model.Email.Trim().ToLower();

            // 1. Cek apakah email terdaftar di database Users
            var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == cleanEmail);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Alamat email ini tidak terdaftar di sistem kami.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Akun Anda sedang dinonaktifkan. Silakan hubungi Administrator.");
                return View(model);
            }

            // 2. Generate token reset unik & simpan masa berlaku 1 jam
            string resetToken = Guid.NewGuid().ToString("N") + DateTime.Now.Ticks.ToString("x");
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordExpiry = DateTime.Now.AddHours(1);
            user.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            // 3. Simulasikan link reset password
            string resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken, email = user.Email }, Request.Url.Scheme);

            TempData["SuccessMessage"] = $"Instruksi reset password telah dikirim ke {user.Email}.";
            TempData["ResetLinkPreview"] = resetLink;

            return View(model);
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public ActionResult ResetPassword(string token, string email)
        {
            ViewBag.Title = "Atur Ulang Password - EduPulse LMS";
            ViewBag.IsPublicPage = true;

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Token reset password tidak valid atau tidak lengkap.";
                return RedirectToAction("Login", "Account");
            }

            string cleanEmail = email.Trim().ToLower();

            // Validasi kecocokan token dan masa berlaku token di database
            var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == cleanEmail && u.ResetPasswordToken == token);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Token reset password tidak valid atau tidak ditemukan.";
                return View("ResetPasswordInvalid");
            }

            if (user.ResetPasswordExpiry == null || user.ResetPasswordExpiry < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Masa berlaku token reset password Anda telah kadaluarsa (lebih dari 1 jam). Silakan ajukan ulang permintaan lupa password.";
                return View("ResetPasswordInvalid");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            ViewBag.Title = "Atur Ulang Password - EduPulse LMS";
            ViewBag.IsPublicPage = true;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string cleanEmail = model.Email.Trim().ToLower();

            // Validasi kembali token dan masa berlaku
            var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == cleanEmail && u.ResetPasswordToken == model.Token);

            if (user == null || user.ResetPasswordExpiry == null || user.ResetPasswordExpiry < DateTime.Now)
            {
                ModelState.AddModelError("", "Token reset password tidak valid atau telah kadaluarsa. Silakan ajukan lupa password kembali.");
                return View(model);
            }

            // Hash password baru menggunakan BCrypt.Net
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // Perbarui password di tabel Users & kosongkan reset token
            user.PasswordHash = newPasswordHash;
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;
            user.UpdatedAt = DateTime.Now;

            _db.SaveChanges();

            TempData["SuccessMessage"] = "Password Anda berhasil diperbarui! Silakan masuk dengan password baru Anda.";
            return RedirectToAction("Login", "Account");
        }

        // GET/POST: /Account/Logout
        public ActionResult Logout()
        {
            // Hapus seluruh Session
            Session.Clear();
            Session.Abandon();

            // Bersihkan Authentication Cookie
            FormsAuthentication.SignOut();

            TempData["SuccessMessage"] = "Anda telah berhasil keluar dari sistem.";
            return RedirectToAction("Index", "Home");
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
