# SPESIFIKASI BISNIS & ALUR APLIKASI LENGKAP

## A. Alur Publik & Autentikasi
1. **Landing Page (`/`)**:
   - Navbar: Logo EduPulse, Menu About, Tombol Login, Tombol Register.
   - Hero Section & Katalog Singkat Kelas Populer.
2. **Login (`/Account/Login`)**:
   - Form Login Email & Password (BCrypt check).
   - Tombol "Continue with Google": Jika email belum terdaftar, otomatis buat user baru role Siswa dan ambil nama + foto profile dari payload Google.
   - Pengecekan Akun: Jika `IsActive == 0`, tolak login dengan pesan "Akun Anda dinonaktifkan oleh Admin."
   - Redirect sesuai role: Admin -> `/Admin/Dashboard`, Guru -> `/Guru/Dashboard`, Siswa -> `/Siswa/Dashboard`.
3. **Register (`/Account/Register`)**:
   - Input: Nama Lengkap, Email, Password, Pilihan Role (Guru / Siswa). Profil tabel otomatis dibuat.
4. **Forgot & Reset Password**:
   - Input Email -> Kirim token reset (disimpan di `ResetPasswordToken` dengan masa berlaku 1 jam di `ResetPasswordExpiry`).
   - Halaman Reset Password -> Verifikasi token dan simpan password hash baru.


## B. Alur & Fitur Role Admin (`/Admin/...`)
1. **Navbar Admin**: Logo, Icon Notifikasi (Pemberitahuan guru/siswa baru daftar & perubahan status akun), Foto Profile & Nama (Dropdown Edit Profile).
2. **Dashboard Admin**: Card Summary (Total Guru, Siswa, Kelas, Siswa Aktif), Chart Statistik, Log aktivitas terbaru.
3. **Daftar Guru**:
   - Tabel: Foto, Nama, Email, No HP, Bio, Status (Aktif/Nonaktif), Aksi.
   - Tombol "Tambah Akun Guru" -> Modal/Form data guru langsung aktif.
   - Fitur toggle status akun (Mengaktifkan / Menonaktifkan guru).
4. **Daftar Siswa**:
   - Tabel data seluruh siswa, fitur edit data dasar, dan toggle status akun aktif/nonaktif.
5. **Daftar Kelas**:
   - Menampilkan seluruh kelas milik semua guru: Nama Kelas, Guru Pembuat, Jumlah Siswa, Status Publish, Action (Edit, Nonaktifkan, Soft Delete).
6. **Laporan & Export**:
   - Tabel ringkasan komprehensif platform.
   - Tombol "Export Excel" -> Mengunduh rekap `.xlsx` via EPPlus.


## C. Alur & Fitur Role Guru (`/Guru/...`)
1. **Navbar Guru**: Notifikasi Real-time (Siswa kumpul tugas, submit kuis, absensi masuk), Foto & Edit Profil.
2. **Dashboard Guru**: Card summary & grid daftar kelas yang diajar oleh guru bersangkutan + Tombol `+ Buat Kelas`.
3. **Fitur Buat Kelas**: Form Nama Kelas, Kategori, Deskripsi, Upload Thumbnail -> Tersimpan dan muncul di katalog siswa.
4. **Manajemen Ruang Kelas (`/Guru/KelasDetail/{id}`)**:
   - **Stream**: Postingan pengumuman interaktif di kelas.
   - **Materi**: List materi per pertemuan, tombol "Tambah Materi" (Upload PDF, PPT, DOC, Video).
   - **Quiz**: 
     - Tombol "Tambah Quiz" (Pertemuan, Judul, Deskripsi, Durasi, Waktu Mulai/Selesai, Passing Score).
     - Halaman "Tambah Soal" (Pertanyaan, Opsi A/B/C/D, Radio kunci benar).
     - Tombol "Aktifkan Quiz".
   - **Tugas**: 
     - Tombol "Tambah Tugas" (Pertemuan, Judul, Deskripsi, Deadline, Upload Lampiran Soal).
     - Tombol "Lihat Pengumpulan" -> Tabel file siswa yang sudah submit, input Nilai (0-100) & Feedback koreksi.
   - **Absensi**: 
     - Tombol "Buat Absensi" (Pertemuan, Tanggal, Jam Mulai, Jam Selesai) -> Klik "Buka Absensi".
     - Tombol "Rekap" -> Menampilkan matriks kehadiran siswa per pertemuan.
   - **Rekap Nilai**: 
     - Tabel rekap nilai seluruh siswa dengan rumus kalkulasi otomatis:
       `Nilai Akhir = (Absensi * 15%) + (Progres Materi * 15%) + (Quiz * 40%) + (Tugas * 30%)`.


## D. Alur & Fitur Role Siswa (`/Siswa/...`)
1. **Dashboard Siswa**: Grid kelas yang sedang diikuti oleh siswa + Tombol "Masuk Kelas".
2. **Tambah Kelas Baru (Katalog)**:
   - Menampilkan kelas publish yang BELUM diikuti siswa.
   - Fitur Search keyword & Filter kategori.
   - Detail Kelas -> Tombol "Daftar Kelas" -> Otomatis join ke kelas (`MemberKelas`).
3. **Menu Global Siswa**:
   - **Semua Tugas**: List seluruh tugas dari semua kelas yang diikuti beserta status deadline.
   - **Semua Materi**: List materi dari semua kelas yang diikuti.
   - **Semua Quiz**: List kuis dari semua kelas beserta status pengerjaan.
   - **Nilai Global**: Nilai rata-rata dari seluruh kelas yang diikuti.
4. **Manajemen di Dalam Ruang Kelas (`/Siswa/KelasDetail/{id}`)**:
   - **Materi**: Buka file pembelajaran -> Tombol "Tandai Sudah Dipelajari" (Progress 100%).
   - **Absensi**: Muncul tombol `Present` jika jam absensi sedang dibuka -> Klik -> Label berubah menjadi `Hadir` (disable).
   - **Quiz**: Klik "Mulai Quiz" -> Timer ujian berjalan -> Pilihan Ganda A/B/C/D -> Submit -> Auto-grading skor instan.
   - **Tugas**: Upload file tugas -> Konfirmasi Modal -> Submit -> Status berubah "Sudah Dikumpulkan".
   - **Nilai**: Menampilkan nilai khusus kelas tersebut secara transparan.

## E. Instruksi Output & Akses URL Pengujian
- **Mode Pengujian**: User akan menjalankan dan menguji aplikasi secara langsung di browser lokal via IIS Express (tanpa remote testing / browser automation).
- **Format Respons AI**: Setiap kali menyelesaikan pembuatan modul, controller, atau halaman view, AI **wajib menyertakan link URL lokal** yang dapat langsung diklik atau diakses oleh user untuk pengujian di browser.
- **Daftar Endpoint URL Standar**:
  - Landing Page: `/` atau `/Home/Index`
  - About: `/Home/About`
  - Login: `/Account/Login`
  - Register: `/Account/Register`
  - Forgot Password: `/Account/ForgotPassword`
  - Reset Password: `/Account/ResetPassword`
  - Dashboard Admin: `/Admin/Dashboard`
  - Daftar Guru: `/Admin/DaftarGuru`
  - Daftar Siswa: `/Admin/DaftarSiswa`
  - Daftar Kelas (Admin): `/Admin/DaftarKelas`
  - Laporan Admin: `/Admin/Laporan`
  - Dashboard Guru: `/Guru/Dashboard`
  - Buat Kelas: `/Guru/BuatKelas`
  - Ruang Kelas Guru: `/Guru/KelasDetail/{id}`
  - Dashboard Siswa: `/Siswa/Dashboard`
  - Katalog Tambah Kelas: `/Siswa/Katalog`
  - Semua Tugas Siswa: `/Siswa/SemuaTugas`
  - Semua Materi Siswa: `/Siswa/SemuaMateri`
  - Semua Quiz Siswa: `/Siswa/SemuaQuiz`
  - Nilai Global Siswa: `/Siswa/NilaiGlobal`
  - Ruang Kelas Siswa: `/Siswa/KelasDetail/{id}`
  - Swagger UI: `/swagger`