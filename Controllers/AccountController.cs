using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

        #region Helper Pengiriman Email OTP via SmtpClient
        /// <summary>
        /// Mengirimkan 6-digit kode OTP ke email pendaftar menggunakan System.Net.Mail.SmtpClient
        /// </summary>
        private bool SendEmailOtp(string toEmail, string otpCode, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                string host = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                int port = 587;
                int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out port);
                if (port <= 0) port = 587;

                string user = ConfigurationManager.AppSettings["SmtpUser"] ?? "";
                string pass = (ConfigurationManager.AppSettings["SmtpPass"] ?? "").Replace(" ", "").Trim();
                string from = ConfigurationManager.AppSettings["SmtpFrom"];
                if (string.IsNullOrWhiteSpace(from))
                {
                    from = !string.IsNullOrWhiteSpace(user) ? user : "no-reply@pub-learninghub.com";
                }
                string fromName = ConfigurationManager.AppSettings["SmtpFromName"] ?? "PUB Learning Hub";

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(from, fromName);
                    mail.To.Add(toEmail);
                    mail.Subject = $"{otpCode} adalah Kode Verifikasi Pendaftaran Anda - PUB Learning Hub";
                    mail.IsBodyHtml = true;
                    mail.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
</head>
<body style='font-family: ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #F0F3FA; margin: 0; padding: 24px;'>
    <div style='max-width: 520px; margin: 0 auto; background: #ffffff; border-radius: 18px; overflow: hidden; box-shadow: 0 10px 25px rgba(57,88,134,0.1); border: 1px solid #E2EAF8;'>
        <div style='background: linear-gradient(135deg, #395886 0%, #638ECB 100%); padding: 32px 24px; text-align: center; color: #ffffff;'>
            <h1 style='margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;'>PUB Learning Hub</h1>
            <p style='margin: 6px 0 0 0; font-size: 13px; color: rgba(255,255,255,0.85);'>Pemberdayaan Umat Berkelanjutan</p>
        </div>
        <div style='padding: 32px 28px; color: #334155;'>
            <h3 style='margin-top: 0; color: #0f172a; font-size: 18px; font-weight: 700;'>Verifikasi Pendaftaran Akun Baru</h3>
            <p style='line-height: 1.6; font-size: 14px; color: #475569;'>
                Halo Calon Pengguna,<br/>
                Terima kasih telah mendaftar di <strong>PUB Learning Hub</strong>. Masukkan 6 digit kode One-Time Password (OTP) berikut untuk menyelesaikan verifikasi akun Anda:
            </p>
            <div style='background: #F0F3FA; border: 2px dashed #638ECB; border-radius: 14px; padding: 18px; text-align: center; margin: 24px 0;'>
                <span style='font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #395886; font-family: monospace;'>{otpCode}</span>
            </div>
            <p style='font-size: 13px; color: #64748b; line-height: 1.5; margin-bottom: 0;'>
                <span style='color: #ef4444; font-weight: 600;'>&#9888; Perhatian:</span> Kode ini aktif selama <strong>5 menit</strong>. Jangan pernah memberikan kode ini kepada orang lain demi menjaga keamanan akun Anda.
            </p>
        </div>
        <div style='background: #f8fafc; border-top: 1px solid #e2e8f0; padding: 16px 24px; text-align: center; font-size: 12px; color: #94a3b8;'>
            &copy; {DateTime.Now.Year} PUB Learning Hub &bull; Email otomatis, mohon tidak membalas email ini.
        </div>
    </div>
</body>
</html>";

                    using (var smtp = new SmtpClient(host, port))
                    {
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;
                        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
                        {
                            smtp.Credentials = new NetworkCredential(user, pass);
                        }
                        smtp.Timeout = 15000;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[SmtpClient OTP Error]: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Mengirimkan email tautan atur ulang kata sandi pengguna via SmtpClient
        /// </summary>
        private bool SendEmailResetPassword(string toEmail, string resetLink, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                string host = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                int port = 587;
                int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out port);
                if (port <= 0) port = 587;

                string user = ConfigurationManager.AppSettings["SmtpUser"] ?? "";
                string pass = (ConfigurationManager.AppSettings["SmtpPass"] ?? "").Replace(" ", "").Trim();
                string from = ConfigurationManager.AppSettings["SmtpFrom"];
                if (string.IsNullOrWhiteSpace(from))
                {
                    from = !string.IsNullOrWhiteSpace(user) ? user : "no-reply@pub-learninghub.com";
                }
                string fromName = ConfigurationManager.AppSettings["SmtpFromName"] ?? "PUB Learning Hub";

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(from, fromName);
                    mail.To.Add(toEmail);
                    mail.Subject = "Tautan Atur Ulang Kata Sandi - PUB Learning Hub";
                    mail.IsBodyHtml = true;
                    mail.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
</head>
<body style='font-family: ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #F0F3FA; margin: 0; padding: 24px;'>
    <div style='max-width: 520px; margin: 0 auto; background: #ffffff; border-radius: 18px; overflow: hidden; box-shadow: 0 10px 25px rgba(57,88,134,0.1); border: 1px solid #E2EAF8;'>
        <div style='background: linear-gradient(135deg, #395886 0%, #638ECB 100%); padding: 32px 24px; text-align: center; color: #ffffff;'>
            <h1 style='margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;'>PUB Learning Hub</h1>
            <p style='margin: 6px 0 0 0; font-size: 13px; color: rgba(255,255,255,0.85);'>Pemberdayaan Umat Berkelanjutan</p>
        </div>
        <div style='padding: 32px 28px; color: #334155;'>
            <h3 style='margin-top: 0; color: #0f172a; font-size: 18px; font-weight: 700;'>Permintaan Atur Ulang Password</h3>
            <p style='line-height: 1.6; font-size: 14px; color: #475569;'>
                Halo,<br/>
                Kami menerima permintaan untuk mereset kata sandi akun <strong>PUB Learning Hub</strong> Anda. Silakan klik tombol di bawah ini untuk membuat kata sandi baru:
            </p>
            <div style='text-align: center; margin: 28px 0;'>
                <a href='{resetLink}' style='background: #395886; color: #ffffff; padding: 14px 32px; border-radius: 10px; text-decoration: none; font-weight: 700; font-size: 15px; display: inline-block; box-shadow: 0 4px 12px rgba(57, 88, 134, 0.3);'>Atur Ulang Kata Sandi</a>
            </div>
            <p style='font-size: 13px; color: #64748b; line-height: 1.5;'>
                Jika tombol di atas tidak dapat diklik, salin dan tempel tautan berikut di peramban Anda:<br/>
                <a href='{resetLink}' style='color: #395886; word-break: break-all; font-size: 12px;'>{resetLink}</a>
            </p>
            <p style='font-size: 13px; color: #64748b; line-height: 1.5; margin-bottom: 0;'>
                <span style='color: #ef4444; font-weight: 600;'>&#9888; Perhatian:</span> Tautan ini hanya berlaku selama <strong>1 jam</strong>. Abaikan email ini jika Anda tidak merasa melakukan permintaan ini.
            </p>
        </div>
        <div style='background: #f8fafc; border-top: 1px solid #e2e8f0; padding: 16px 24px; text-align: center; font-size: 12px; color: #94a3b8;'>
            &copy; {DateTime.Now.Year} PUB Learning Hub &bull; Email otomatis, mohon tidak membalas email ini.
        </div>
    </div>
</body>
</html>";

                    using (var smtp = new SmtpClient(host, port))
                    {
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;
                        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
                        {
                            smtp.Credentials = new NetworkCredential(user, pass);
                        }
                        smtp.Timeout = 15000;
                        smtp.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[SmtpClient ResetPassword Error]: {ex}");
                return false;
            }
        }
        #endregion

        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            ViewBag.Title = "Daftar Akun Baru - PUB Learning Hub";
            ViewBag.IsPublicPage = true;
            return View(new RegisterViewModel());
        }

        // POST: /Account/SendOtp
        [HttpPost]
        public ActionResult SendOtp(RegisterViewModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Data pendaftaran tidak valid." });
            }

            if (string.IsNullOrWhiteSpace(model.NamaLengkap))
            {
                return Json(new { success = false, message = "Nama lengkap wajib diisi." });
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return Json(new { success = false, message = "Alamat email wajib diisi." });
            }

            string cleanEmail = model.Email.Trim().ToLower();
            try
            {
                var addr = new MailAddress(cleanEmail);
                if (addr.Address != cleanEmail)
                {
                    return Json(new { success = false, message = "Format email tidak valid." });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Format email tidak valid." });
            }

            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
            {
                return Json(new { success = false, message = "Password minimal harus 6 karakter." });
            }

            if (model.Password != model.ConfirmPassword)
            {
                return Json(new { success = false, message = "Konfirmasi password tidak cocok dengan password." });
            }

            // 1. Cek duplikasi email pada tabel Users
            if (_db.Users.Any(u => u.Email.ToLower() == cleanEmail))
            {
                return Json(new { success = false, message = "Alamat email ini sudah terdaftar. Silakan gunakan email lain atau login ke akun Anda." });
            }

            // 2. Generate 6 digit angka acak OTP
            var random = new Random();
            string otpCode = random.Next(100000, 999999).ToString();

            // 3. Simpan data form sementara, kode OTP, dan batas kedaluwarsa 5 menit ke Session
            Session["RegisterOtp"] = otpCode;
            Session["RegisterData"] = model;
            Session["OtpExpiry"] = DateTime.Now.AddMinutes(5);

            // 4. Kirim email berisi kode OTP ke email pendaftar via SmtpClient
            string pass = ConfigurationManager.AppSettings["SmtpPass"];
            bool emailSent = false;
            string smtpError = null;

            if (!string.IsNullOrWhiteSpace(pass))
            {
                emailSent = SendEmailOtp(cleanEmail, otpCode, out smtpError);
            }

            if (emailSent)
            {
                return Json(new { 
                    success = true, 
                    email = cleanEmail, 
                    message = $"Kode OTP 6-digit berhasil dikirimkan ke {cleanEmail}. Silakan periksa kotak masuk atau spam email Anda." 
                });
            }
            else if (string.IsNullOrWhiteSpace(pass))
            {
                // Coba kirim jika server SMTP lokal / default tersedia
                bool attempted = SendEmailOtp(cleanEmail, otpCode, out smtpError);
                if (attempted)
                {
                    return Json(new { 
                        success = true, 
                        email = cleanEmail, 
                        message = $"Kode OTP telah dikirim ke {cleanEmail}." 
                    });
                }
                else
                {
                    // Fallback informatif untuk lingkungan testing lokal saat SmtpPass belum diatur
                    return Json(new { 
                        success = true, 
                        email = cleanEmail, 
                        message = $"Kode OTP verifikasi 6 digit telah digenerate. (Mode Pengujian: Masukkan SmtpPass di Web.config untuk email nyata). Kode OTP Anda: {otpCode}",
                        debugOtp = otpCode
                    });
                }
            }
            else
            {
                return Json(new { 
                    success = false, 
                    message = $"Gagal mengirimkan email verifikasi: {smtpError}. Pastikan konfigurasi SMTP di Web.config sudah tepat." 
                });
            }
        }

        // POST: /Account/VerifyOtpAndRegister
        [HttpPost]
        public ActionResult VerifyOtpAndRegister(string otpInput)
        {
            if (string.IsNullOrWhiteSpace(otpInput))
            {
                return Json(new { success = false, message = "Silakan masukkan 6 digit kode OTP verifikasi." });
            }

            string sessionOtp = Session["RegisterOtp"] as string;
            DateTime? otpExpiry = Session["OtpExpiry"] as DateTime?;
            var registerData = Session["RegisterData"] as RegisterViewModel;

            if (string.IsNullOrEmpty(sessionOtp) || otpExpiry == null || registerData == null)
            {
                return Json(new { success = false, message = "Sesi pendaftaran tidak ditemukan atau telah berakhir. Silakan isi kembali formulir pendaftaran." });
            }

            // Cek batas waktu kedaluwarsa (5 menit)
            if (DateTime.Now > otpExpiry.Value)
            {
                return Json(new { success = false, message = "Kode OTP telah kedaluwarsa. Silakan klik 'Kirim Ulang Kode OTP'." });
            }

            // Cocokkan kode OTP
            if (!string.Equals(otpInput.Trim(), sessionOtp.Trim(), StringComparison.Ordinal))
            {
                return Json(new { success = false, message = "Kode OTP yang Anda masukkan salah. Silakan periksa kembali." });
            }

            string cleanEmail = registerData.Email.Trim().ToLower();

            // Pastikan email belum terdaftar (jika ada pendaftaran paralel)
            if (_db.Users.Any(u => u.Email.ToLower() == cleanEmail))
            {
                return Json(new { success = false, message = "Email sudah terdaftar. Silakan login ke akun Anda." });
            }

            // Cari Role yang dipilih (Guru / Siswa)
            string selectedRoleName = (registerData.Role == "Guru") ? "Guru" : "Siswa";
            var role = _db.Roles.FirstOrDefault(r => r.NamaRole == selectedRoleName);
            if (role == null)
            {
                role = _db.Roles.FirstOrDefault(r => r.NamaRole == "Siswa");
            }
            int idRole = role != null ? role.IdRole : 3;

            // Hashing password menggunakan BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerData.Password);

            // Simpan akun baru ke tabel Users dengan IsActive = true dan IsEmailVerified = true
            var newUser = new Users
            {
                NamaLengkap = registerData.NamaLengkap.Trim(),
                Email = cleanEmail,
                PasswordHash = passwordHash,
                IdRole = idRole,
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            // Otomatis buat data awal di tabel Profiles berelasi dengan IdUser baru
            var newProfile = new Profiles
            {
                IdUser = newUser.IdUser,
                NoHp = null,
                Bio = $"Pengguna baru {selectedRoleName} PUB Learning Hub.",
                Alamat = null,
                JenisKelamin = null,
                UpdatedAt = DateTime.Now
            };

            _db.Profiles.Add(newProfile);
            _db.SaveChanges();

            // Hapus data sesi pendaftaran terkait
            Session.Remove("RegisterOtp");
            Session.Remove("RegisterData");
            Session.Remove("OtpExpiry");

            // Berikan pesan sukses via TempData dan redirect ke /Account/Login
            TempData["SuccessMessage"] = "Pendaftaran dan verifikasi akun berhasil! Silakan masuk menggunakan email dan password Anda.";

            return Json(new { 
                success = true, 
                message = "Verifikasi berhasil! Mengalihkan ke halaman login...", 
                redirectUrl = Url.Action("Login", "Account") 
            });
        }

        // POST: /Account/ResendOtp
        [HttpPost]
        public ActionResult ResendOtp()
        {
            var registerData = Session["RegisterData"] as RegisterViewModel;
            if (registerData == null || string.IsNullOrWhiteSpace(registerData.Email))
            {
                return Json(new { success = false, message = "Sesi pendaftaran tidak ditemukan. Silakan lengkapi formulir pendaftaran kembali." });
            }

            string cleanEmail = registerData.Email.Trim().ToLower();

            // Generate kode OTP 6-digit baru
            var random = new Random();
            string newOtp = random.Next(100000, 999999).ToString();

            // Perbarui sesi dengan kode baru dan perpanjang masa aktif 5 menit
            Session["RegisterOtp"] = newOtp;
            Session["OtpExpiry"] = DateTime.Now.AddMinutes(5);

            string pass = ConfigurationManager.AppSettings["SmtpPass"];
            bool emailSent = false;
            string smtpError = null;

            if (!string.IsNullOrWhiteSpace(pass))
            {
                emailSent = SendEmailOtp(cleanEmail, newOtp, out smtpError);
            }

            if (emailSent)
            {
                return Json(new { 
                    success = true, 
                    email = cleanEmail, 
                    message = $"Kode OTP baru telah berhasil dikirimkan ke {cleanEmail}." 
                });
            }
            else if (string.IsNullOrWhiteSpace(pass))
            {
                bool attempted = SendEmailOtp(cleanEmail, newOtp, out smtpError);
                if (attempted)
                {
                    return Json(new { 
                        success = true, 
                        email = cleanEmail, 
                        message = $"Kode OTP baru telah dikirimkan ke {cleanEmail}." 
                    });
                }
                else
                {
                    return Json(new { 
                        success = true, 
                        email = cleanEmail, 
                        message = $"Kode OTP baru telah dibuat. (Mode Pengujian: Masukkan SmtpPass di Web.config untuk email nyata). Kode baru: {newOtp}",
                        debugOtp = newOtp
                    });
                }
            }
            else
            {
                return Json(new { 
                    success = false, 
                    message = $"Gagal mengirim ulang kode OTP: {smtpError}." 
                });
            }
        }

        // POST: /Account/Register (Fallback standard form submit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            ViewBag.Title = "Daftar Akun Baru - PUB Learning Hub";
            ViewBag.IsPublicPage = true;

            if (Request.IsAjaxRequest())
            {
                return SendOtp(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string cleanEmail = model.Email.Trim().ToLower();

            if (_db.Users.Any(u => u.Email.ToLower() == cleanEmail))
            {
                ModelState.AddModelError("Email", "Email ini sudah terdaftar. Silakan gunakan email lain atau masuk ke akun Anda.");
                return View(model);
            }

            // Simpan ke sesi dan arahkan pengguna untuk verifikasi OTP
            var random = new Random();
            string otpCode = random.Next(100000, 999999).ToString();

            Session["RegisterOtp"] = otpCode;
            Session["RegisterData"] = model;
            Session["OtpExpiry"] = DateTime.Now.AddMinutes(5);

            SendEmailOtp(cleanEmail, otpCode, out _);

            ViewBag.ShowOtpModal = true;
            ViewBag.OtpEmail = cleanEmail;
            return View(model);
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
                return RedirectToAction("Dashboard", "Siswa");
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
            string redirectUrl = Url.Action("Dashboard", "Siswa");
            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                redirectUrl = Url.Action("Dashboard", "Admin");
            }
            else if (roleName.Equals("Guru", StringComparison.OrdinalIgnoreCase))
            {
                redirectUrl = Url.Action("Dashboard", "Guru");
            }
            else
            {
                redirectUrl = Url.Action("Dashboard", "Siswa");
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

            // 3. Kirim link reset password via email
            string resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken, email = user.Email }, Request.Url.Scheme);
            string smtpError = null;
            bool emailSent = SendEmailResetPassword(user.Email, resetLink, out smtpError);

            if (emailSent)
            {
                TempData["SuccessMessage"] = $"Instruksi dan tautan atur ulang kata sandi telah berhasil dikirimkan ke {user.Email}. Silakan periksa kotak masuk atau spam email Anda.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Instruksi reset password telah diproses untuk {user.Email}.";
                TempData["ResetLinkPreview"] = resetLink;
            }

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
