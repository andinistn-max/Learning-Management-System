# SPESIFIKASI ENDPOINT REST WEB API

Format response seluruh endpoint dibungkus menggunakan generic model `ApiResponse<T>`.

## 1. Authentication (`/api/auth`)
- `POST /api/auth/register` : Registrasi akun baru (Body: NamaLengkap, Email, Password, IdRole)
- `POST /api/auth/login`    : Login akun (Body: Email, Password) -> Return JWT Token & Data User
- `POST /api/auth/google`   : Login Google OAuth (Body: GoogleIdToken)
- `POST /api/auth/forgot-password` : Request token reset password ke email
- `POST /api/auth/reset-password`  : Reset password dengan token valid

## 2. Modul Kelas (`/api/kelas`)
- `GET    /api/kelas`             : Ambil list kelas publish (Query: search, kategori, sort, page, limit)
- `GET    /api/kelas/{id}`        : Ambil detail kelas lengkap
- `POST   /api/kelas`             : Buat kelas baru (Role: Guru/Admin)
- `PUT    /api/kelas/{id}`        : Update data kelas
- `DELETE /api/kelas/{id}`        : Soft delete kelas (`IsDeleted = 1`)
- `POST   /api/kelas/{id}/join`   : Siswa mendaftar ke kelas

## 3. Modul Materi (`/api/materi`)
- `GET    /api/materi/kelas/{idKelas}` : List materi per kelas
- `POST   /api/materi`                 : Upload materi baru (Multipart/Form-Data)
- `DELETE /api/materi/{id}`            : Soft delete materi
- `POST   /api/materi/{id}/selesai`    : Siswa menandai materi selesai (Update ProgresMateri)

## 4. Modul Absensi (`/api/absensi`)
- `POST /api/absensi/jadwal`   : Guru membuka jadwal sesi absensi baru
- `POST /api/absensi/present`  : Siswa melakukan presensi kehadiran
- `GET  /api/absensi/rekap/{idKelas}` : Rekap matriks kehadiran siswa

## 5. Modul Tugas (`/api/tugas`)
- `POST /api/tugas`            : Guru membuat tugas baru (Multipart Form)
- `POST /api/tugas/submit`     : Siswa mengumpulkan dokumen tugas
- `POST /api/tugas/nilai`      : Guru memberi nilai & feedback tugas

## 6. Modul Quiz (`/api/quiz`)
- `POST /api/quiz`             : Guru membuat kuis baru
- `POST /api/quiz/{id}/soal`   : Guru menambahkan butir soal & opsi jawaban
- `POST /api/quiz/{id}/start`  : Siswa memulai kuis (Mencatat WaktuMulai)
- `POST /api/quiz/{id}/submit` : Siswa submit jawaban (Auto-scoring)

## 7. Modul Nilai & Laporan (`/api/nilai`)
- `GET /api/nilai/kelas/{idKelas}` : Rekap nilai siswa per kelas (Bobot: Absen 15%, Materi 15%, Quiz 40%, Tugas 30%)
- `GET /api/laporan/export-excel`  : Unduh file Excel rekapitulasi platform