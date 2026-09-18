using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Google.Apis.Auth;
using Learning_Management_System.Helpers;
using Learning_Management_System.Models.DTO;
using Learning_Management_System.Models.Entity;
using Learning_Management_System.Models.ViewModel;
using Learning_Management_System.Services.Context;

namespace Learning_Management_System.Controllers.API
{
    /// <summary>
    /// Layanan Autentikasi REST API (JWT &amp; Akun Pengguna)
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly LmsDbContext db = new LmsDbContext();

        /// <summary>
        /// Registrasi akun baru (Siswa / Guru)
        /// </summary>
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public IHttpActionResult Register([FromBody] RegisterRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            if (db.Users.Any(u => u.Email.ToLower() == dto.Email.ToLower()))
                return BadRequest("Email sudah terdaftar dalam sistem.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newUser = new Users
            {
                NamaLengkap = dto.NamaLengkap,
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = passwordHash,
                IdRole = dto.IdRole > 0 ? dto.IdRole : 3, // Default Siswa
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            // Buat profil default
            var newProfile = new Profiles
            {
                IdUser = newUser.IdUser,
                Bio = "Peserta aktif platform PUB Learning Hub.",
                UpdatedAt = DateTime.Now
            };
            db.Profiles.Add(newProfile);
            db.SaveChanges();

            string roleName = newUser.IdRole == 1 ? "Admin" : (newUser.IdRole == 2 ? "Guru" : "Siswa");
            string token = JwtHelper.GenerateToken(newUser.IdUser, newUser.Email, newUser.NamaLengkap, roleName);

            return Ok(ApiResponse<object>.Ok(new
            {
                userId = newUser.IdUser,
                namaLengkap = newUser.NamaLengkap,
                email = newUser.Email,
                role = roleName,
                token = token
            }, "Registrasi akun berhasil."));
        }

        /// <summary>
        /// Login akun pengguna dan dapatkan JWT Bearer Token
        /// </summary>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public IHttpActionResult Login([FromBody] LoginRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var user = db.Users.Include("Role").FirstOrDefault(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null || !user.IsActive)
                return Unauthorized();

            if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized();

            string roleName = user.Role != null ? user.Role.NamaRole : "Siswa";
            string token = JwtHelper.GenerateToken(user.IdUser, user.Email, user.NamaLengkap, roleName);

            return Ok(ApiResponse<object>.Ok(new
            {
                userId = user.IdUser,
                namaLengkap = user.NamaLengkap,
                email = user.Email,
                role = roleName,
                token = token
            }, "Autentikasi login berhasil."));
        }

        /// <summary>
        /// Login integrasi Google OAuth 2.0 menggunakan Google ID Token
        /// </summary>
        [HttpPost]
        [Route("google")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.GoogleIdToken))
                return BadRequest("Google ID Token tidak boleh kosong.");

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.GoogleIdToken);
                if (payload == null)
                    return BadRequest("Token Google tidak valid.");

                var user = db.Users.Include("Role").FirstOrDefault(u => u.Email.ToLower() == payload.Email.ToLower());
                if (user == null)
                {
                    user = new Users
                    {
                        NamaLengkap = payload.Name ?? "Pengguna Google",
                        Email = payload.Email.ToLower(),
                        GoogleId = payload.Subject,
                        FotoProfile = payload.Picture,
                        IdRole = 3, // Siswa
                        IsActive = true,
                        IsEmailVerified = true,
                        CreatedAt = DateTime.Now
                    };
                    db.Users.Add(user);
                    db.SaveChanges();

                    var profile = new Profiles
                    {
                        IdUser = user.IdUser,
                        Bio = "Pengguna terdaftar via Google OAuth.",
                        UpdatedAt = DateTime.Now
                    };
                    db.Profiles.Add(profile);
                    db.SaveChanges();
                }

                string roleName = user.Role != null ? user.Role.NamaRole : "Siswa";
                string token = JwtHelper.GenerateToken(user.IdUser, user.Email, user.NamaLengkap, roleName);

                return Ok(ApiResponse<object>.Ok(new
                {
                    userId = user.IdUser,
                    namaLengkap = user.NamaLengkap,
                    email = user.Email,
                    role = roleName,
                    token = token
                }, "Login Google berhasil."));
            }
            catch (Exception ex)
            {
                return BadRequest("Validasi token Google gagal: " + ex.Message);
            }
        }

        /// <summary>
        /// Permintaan pengiriman kode/token reset kata sandi ke email
        /// </summary>
        [HttpPost]
        [Route("forgot-password")]
        [AllowAnonymous]
        public IHttpActionResult ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var user = db.Users.FirstOrDefault(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null)
                return Ok(ApiResponse.Ok("Jika email terdaftar, instruksi reset kata sandi telah dikirimkan."));

            string token = Guid.NewGuid().ToString("N");
            user.ResetPasswordToken = token;
            user.ResetPasswordExpiry = DateTime.Now.AddHours(2);
            db.SaveChanges();

            return Ok(ApiResponse<object>.Ok(new { resetToken = token }, "Token reset kata sandi berhasil digenerate."));
        }

        /// <summary>
        /// Reset kata sandi baru menggunakan token valid
        /// </summary>
        [HttpPost]
        [Route("reset-password")]
        [AllowAnonymous]
        public IHttpActionResult ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var user = db.Users.FirstOrDefault(u => u.ResetPasswordToken == dto.Token && u.ResetPasswordExpiry > DateTime.Now);
            if (user == null)
                return BadRequest("Token reset kata sandi tidak valid atau telah kedaluwarsa.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;
            user.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            return Ok(ApiResponse.Ok("Kata sandi berhasil diperbarui. Silakan login kembali."));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
