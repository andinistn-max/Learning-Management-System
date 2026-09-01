# RENCANA TAHAPAN IMPLEMENTASI KODE (STEP-BY-STEP)

Panduan eksekusi prompt terstruktur per sub-step:

- **Fase 0: Data Entity & Seeder**
  - Step 0.3: Buat 17 class Entity di `Models/Entity/` dan daftarkan `DbSet` di `LmsDbContext.cs`.
  - Step 0.4: Buat script database seeder berisi minimal 20 data dummy untuk setiap entitas utama.

- **Fase 1: Autentikasi & Halaman Publik**
  - Step 1.1: Buat Landing Page publik & Halaman About.
  - Step 1.2: Buat Controller Autentikasi (Register, Login BCrypt, Google OAuth, Forgot/Reset Password).
  - Step 1.3: Buat Custom Action Filter `[AuthorizeRole]` dan Setup Session/Cookie.
  - Step 1.4: Buat Halaman Error Global (401, 403, 404, 500) dan Toastr Notification.

- **Fase 2: Modul Admin (RBAC Role 1)**
  - Step 2.1: Layout khusus Admin (Sidebar & Navbar).
  - Step 2.2: Dashboard Admin (Statistik Real-time & Chart).
  - Step 2.3: Menu Daftar Guru (CRUD, Toggle Status, Search, Filter, Sort, Pagination).
  - Step 2.4: Menu Daftar Siswa (CRUD, Toggle Status, Search, Filter, Pagination).
  - Step 2.5: Menu Daftar Kelas (Monitoring Semua Kelas Guru, Soft Delete & Restore).
  - Step 2.6: Menu Laporan Platform & Fitur Export Excel via EPPlus 4.1.1.

- **Fase 3: Modul Guru (RBAC Role 2)**
  - Step 3.1: Layout khusus Guru & Dashboard Guru (Kartu Kelas).
  - Step 3.2: Fitur Buat Kelas Baru (Upload Thumbnail & Kategori).
  - Step 3.3: Ruang Kelas Guru - Tab Stream & Tab Materi (Upload PDF/Video).
  - Step 3.4: Ruang Kelas Guru - Tab Quiz (Form Kuis & Form Soal Pilihan Ganda).
  - Step 3.5: Ruang Kelas Guru - Tab Tugas (Buat Tugas, Lihat Pengumpulan, Nilai & Feedback).
  - Step 3.6: Ruang Kelas Guru - Tab Absensi (Buka Absensi & Rekap Matriks).
  - Step 3.7: Ruang Kelas Guru - Tab Rekap Nilai Terkalkulasi (15-15-40-30).

- **Fase 4: Modul Siswa (RBAC Role 3)**
  - Step 4.1: Layout khusus Siswa & Dashboard Siswa (Kelas yang Diikuti).
  - Step 4.2: Katalog Tambah Kelas (Search, Filter, Detail Kelas, Tombol Daftar).
  - Step 4.3: Menu Global Siswa (Semua Tugas, Semua Materi, Semua Quiz, Nilai Rata-rata).
  - Step 4.4: Ruang Kelas Siswa - Belajar Materi & Tombol "Tandai Sudah Dipelajari".
  - Step 4.5: Ruang Kelas Siswa - Presensi Absensi (Tombol Present).
  - Step 4.6: Ruang Kelas Siswa - Pengerjaan Quiz (Timer & Auto-Scoring).
  - Step 4.7: Ruang Kelas Siswa - Upload Pengumpulan Tugas.
  - Step 4.8: Ruang Kelas Siswa - Tab Nilai Transparan Kelas.

- **Fase 5: Dokumentasi API & Finalisasi**
  - Step 5.1: Setup Swagger Web API Documentation.
  - Step 5.2: Finalisasi README.md dan Flowchart Sistem.