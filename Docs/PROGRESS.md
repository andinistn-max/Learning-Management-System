# STATUS PROGRESS PENGERJAAN PER MENU & FITUR (EDUPULSE LMS)

## 0. Setup Basis & Database Seeding
- [x] Step 0.1: Setup Database SQL Server Express & Schema db_Learning_Management_System
- [x] Step 0.2: Konfigurasi Connection String Web.config, Packages NuGet, & Master Layout Tabler
- [x] Step 0.3: Pembuatan Seluruh Models/Entity (17 Tabel) & Setup LmsDbContext.cs
- [x] Step 0.4: Skrip Seeder Minimal 20 Data Awal per Entitas Utama


## 1. Modul Autentikasi & Halaman Publik
- [x] Step 1.1: Landing Page Publik & Halaman About
- [x] Step 1.2: Register Account (Validasi Form Realtime, BCrypt Hash, Auto Create Profile)
- [x] Step 1.3: Login Account (Email/Password BCrypt, Active Check, Session & JWT Token)
- [x] Step 1.4: Fitur Continue with Google (Google OAuth 2.0 Integration)
- [x] Step 1.5: Fitur Forgot Password & Reset Password (Token Expiry Verification)
- [x] Step 1.6: Modul Profil Pengguna & Keamanan (Edit Data Diri, Upload Avatar, Ganti Password)
- [x] Step 1.7: Toast Notification Component (Success, Error, Warning, Info)


## 2. Modul Admin (RBAC 1)
- [x] Step 2.1: Layout Khusus Admin (Sidebar, Navbar Notifikasi, Dropdown Profile)
- [x] Step 2.2: Dashboard Admin Real-Time (Card Summary, Statistik Total, Log Aktivitas)
- [x] Step 2.3: Menu Daftar Guru (CRUD, Toggle Aktif/Nonaktif, Search, Filter, Sort, Pagination)
- [x] Step 2.4: Fitur Tambah Akun Guru oleh Admin
- [x] Step 2.5: Menu Daftar Siswa (CRUD, Toggle Status, Search, Filter, Pagination)
- [x] Step 2.6: Menu Daftar Kelas (Monitoring Semua Kelas Guru, Soft Delete & Restore)
- [x] Step 2.7: Menu Laporan Platform & Export Excel (.xlsx via EPPlus 4.1.1)
- [x] Step 2.8: Halaman Notifikasi Admin & Edit Profil Admin


## 3. Modul Guru (RBAC 2)
- [x] Step 3.1: Layout Khusus Guru (Sidebar, Navbar Notifikasi Real-time, Edit Profil)
- [x] Step 3.2: Dashboard Guru (Card Summary & Grid Kelas Milik Guru)
- [x] Step 3.3: Fitur Buat Kelas Baru (Upload Thumbnail, Kategori, Deskripsi)
- [x] Step 3.4: Ruang Kelas Guru - Tab Stream (Postingan & Pengumuman)
- [x] Step 3.5: Ruang Kelas Guru - Tab Materi (CRUD Materi, Upload PDF/Video, Soft Delete)
- [x] Step 3.6: Ruang Kelas Guru - Tab Quiz (CRUD Kuis, Form Soal Pilihan Ganda & Kunci Jawaban)
- [x] Step 3.7: Ruang Kelas Guru - Tab Tugas (CRUD Tugas, Lampiran File, Soft Delete)
- [x] Step 3.8: Ruang Kelas Guru - Tab Koreksi Tugas (Lihat File Siswa, Input Nilai & Feedback)
- [x] Step 3.9: Ruang Kelas Guru - Tab Absensi (Buka Sesi Absen, Rekap Matriks Kehadiran Siswa)
- [x] Step 3.10: Ruang Kelas Guru - Tab Nilai (Rekap Terkalkulasi: 15% Absen, 15% Materi, 40% Quiz, 30% Tugas)


## 4. Modul Siswa (RBAC 3)
- [x] Step 4.1: Layout Khusus Siswa (Sidebar, Navbar Notifikasi Siswa, Edit Profil)
- [x] Step 4.2: Dashboard Siswa (Grid Kelas yang Diikuti & Progress Belajar)
- [x] Step 4.3: Menu Katalog Tambah Kelas (Search, Filter Kategori, Sorting, Detail & Tombol Daftar)
- [x] Step 4.4: Menu Global Siswa - Semua Tugas, Semua Materi, Semua Quiz (Validasi Status Deadline)
- [x] Step 4.5: Menu Global Siswa - Rekap Nilai Rata-rata Semua Kelas
- [x] Step 4.6: Ruang Kelas Siswa - Tab Materi (Viewer Materi, Download File, Tombol Tandai Sudah Dipelajari)
- [x] Step 4.7: Ruang Kelas Siswa - Tab Absensi (Tombol "Present" saat Jam Buka Sesi, Status "Hadir")
- [x] Step 4.8: Ruang Kelas Siswa - Tab Quiz (Timer Ujian Real-time, Pilihan Ganda, Auto-Submit, Auto-Scoring)
- [x] Step 4.9: Ruang Kelas Siswa - Tab Tugas (Upload Dokumen Tugas, Konfirmasi Modal, Status Pengumpulan)
- [x] Step 4.10: Ruang Kelas Siswa - Tab Nilai Khusus Kelas


## 5. Dokumentasi API & Finalisasi Pengumpulan
- [ ] Step 5.1: Swagger / OpenAPI REST Web API Documentation
- [ ] Step 5.2: Finalisasi README.md Lengkap (Panduan Instalasi, Akun Demo, Flowchart)