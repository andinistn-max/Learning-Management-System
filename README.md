# EduPulse - Learning Management System (LMS)

## Deskripsi Singkat
EduPulse adalah platform Learning Management System (LMS) berbasis web fullstack yang dibangun menggunakan ASP.NET MVC 5, Web API 2, dan Microsoft SQL Server Express dengan tema antarmuka modern Tabler UI (Bootstrap 5).

## Fitur Utama
- **Role-Based Access Control (RBAC)**: Hak akses terpisah untuk Admin, Guru, dan Siswa.
- **Autentikasi Lengkap**: Login BCrypt, Google OAuth 2.0, Register, Forgot Password, dan Reset Password.
- **Manajemen Kelas & Pembelajaran**: Upload materi pembelajaran (PDF/Video), jadwal presensi interaktif, kuis pilihan ganda auto-grading dengan countdown timer, dan pengumpulan tugas terintegrasi.
- **Sistem Penilaian Otomatis**: Kalkulasi bobot nilai akhir (`15% Absensi + 15% Progres Materi + 40% Quiz + 30% Tugas`).
- **Laporan & Export**: Export rekapitulasi platform ke format Microsoft Excel (.xlsx) menggunakan EPPlus 4.1.1.
- **CRUD Lengkap dengan Search, Filter, Sorting & Pagination** pada setiap entitas utama.

## Teknologi yang Digunakan
- **Backend**: ASP.NET MVC 5 & Web API 2 (.NET Framework 4.7.2 / 4.8), Entity Framework 6.4.4
- **Database**: Microsoft SQL Server Express Edition (`localhost\SQLEXPRESS`)
- **Frontend**: ASP.NET Razor Views (.cshtml), Tabler UI (Bootstrap 5 via cdnjs), Toastr Notification
- **Security & Libraries**: BCrypt.Net-Next, System.IdentityModel.Tokens.Jwt, Google.Apis.Auth, EPPlus 4.1.1

## Akun Demo Pengujian
- **Admin**: `admin@edupulse.com` / `Admin123!`
- **Guru**: `guru@edupulse.com` / `Guru123!`
- **Siswa**: `siswa@edupulse.com` / `Siswa123!`

## Cara Instalasi & Menjalankan Proyek
1. Pastikan SQL Server Express dan Visual Studio 2022 sudah terpasang.
2. Buka SSMS, hubungkan ke `localhost\SQLEXPRESS`, lalu jalankan query pada file `Docs/SCHEMA.md`.
3. Buka solution file `EduPulse.sln` di Visual Studio.
4. Klik kanan Solution -> **Restore NuGet Packages**.
5. Tekan tombol **F5 / Ctrl + F5** (IIS Express) untuk menjalankan aplikasi di browser.