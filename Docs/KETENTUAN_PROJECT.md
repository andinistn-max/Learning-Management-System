# STANDAR & KETENTUAN TEKNIS PROYEK S1 EDUPULSE LMS

Dokumen ini memuat seluruh standar wajib yang harus dipatuhi AI dalam meng-generate kode backend maupun frontend:

## 1. Standar Frontend (Client Side)
- **Responsive Layout**: Layout wajib responsif dan adaptif menggunakan grid Tabler UI (Bootstrap 5) pada Mobile (<768px), Tablet (769-1024px), dan Desktop (>1024px) tanpa overflow/layout rusak.
- **Authentication Flow**:
  - Alur lengkap: Login, Register, Logout, Forgot Password, Reset Password, dan Google OAuth 2.0.
  - Sesi login disimpan di Cookie dan Local Storage (JWT Token). Sesi harus tetap aktif saat browser direfresh.
  - Saat logout, bersihkan session server dan storage client, lalu redirect ke Landing Page.
- **Routing & Guarding**: Menggunakan role checking (`Session["Role"]` / ActionFilter `[AuthorizeRole("Admin", "Guru", "Siswa")]`). Redirect ke Login jika unauthorized dan Forbidden 403 jika role tidak sesuai.
- **Dashboard Interaktif**: Menampilkan data real-time, Card Summary ringkasan metrik, dan log aktivitas terbaru.
- **CRUD & Manipulasi Tabel**:
  - Setiap entitas utama wajib memiliki tampilan: List Data, Detail, Tambah, Edit, dan Hapus (Soft Delete).
  - List data wajib mendukung fitur:
    - **Search**: Pencarian keyword realtime/server-side.
    - **Filter**: Kategori, Status (Aktif/Nonaktif/Selesai), dan Tanggal.
    - **Sorting**: Terbaru, Terlama, A-Z, Z-A.
    - **Pagination**: Nomor halaman, tombol Next/Prev, info jumlah data, dan opsi item per halaman (10, 25, 50).
- **Form Validation & Notification**:
  - Validasi form realtime di client-side (Required, Email format, min/max length, password match).
  - Feedback aksi CRUD menggunakan Toast Notification (Toastr / Tabler Toast) untuk Success, Error, Warning, dan Info.
- **Error Handling Views**: Memiliki halaman error khusus: `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, dan `500 Internal Server Error`.

## 2. Standar Backend (Server Side)
- **REST API Standard**: Menggunakan endpoint RESTful (GET, POST, PUT, PATCH, DELETE) dengan HTTP Status Code tepat (200, 201, 400, 401, 403, 404, 422, 500) yang dibungkus dalam generic `ApiResponse<T>`.
- **Keamanan (Security)**:
  - Hash password wajib menggunakan `BCrypt.Net.BCrypt.HashPassword`.
  - Token JWT Authentication untuk Web API.
  - CORS diaktifkan pada Web API.
  - Pencegahan SQL Injection menggunakan parameterized LINQ queries pada Entity Framework 6.
  - Validasi server-side menyeluruh pada semua DTO request model.
- **Upload File**:
  - Validasi tipe file (Gambar: .jpg, .jpeg, .png; Dokumen: .pdf, .ppt, .pptx, .doc, .docx; Video: .mp4) dan batas ukuran file maksimal (misal 50MB).
  - Simpan secara modular di `/Uploads/Materi/`, `/Uploads/Tugas/`, `/Uploads/Thumbnail/`, `/Uploads/Profile/`.
- **Soft Delete**: Diterapkan wajib pada minimal 2 tabel: `Kelas`, `Materi`, dan `Tugas` dengan mengeset `IsDeleted = 1` dan `DeletedAt = DateTime.Now`.
- **Export Laporan**: Menggunakan library **EPPlus 4.1.1** untuk mengekspor data ke format `.xlsx`.