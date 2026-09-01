USE db_Learning_Management_System;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================================
-- 0. INDEXING UNTUK GOOGLEID NULLABLE UNIQUE IN SQL SERVER
-- ============================================================================
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'ALTER TABLE Users DROP CONSTRAINT [' + name + ']; '
FROM sys.objects
WHERE type = 'UQ' AND parent_object_id = OBJECT_ID('Users') AND name LIKE '%GoogleId%';

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Users_GoogleId' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Users_GoogleId ON Users(GoogleId) WHERE GoogleId IS NOT NULL;
END;
GO

-- ============================================================================
-- 1. SEEDER TABEL ROLES (RBAC)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM Roles WHERE NamaRole = 'Admin')
    INSERT INTO Roles (NamaRole, Deskripsi) VALUES ('Admin', 'Administrator Sistem');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE NamaRole = 'Guru')
    INSERT INTO Roles (NamaRole, Deskripsi) VALUES ('Guru', 'Pengajar Kelas');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE NamaRole = 'Siswa')
    INSERT INTO Roles (NamaRole, Deskripsi) VALUES ('Siswa', 'Peserta Kelas');
GO

-- ============================================================================
-- 2. SEEDER TABEL USERS & PROFILES (21 AKUN)
-- Default Password: Password123! (BCrypt Hash: $2a$11$lGeGjhXeJcTn1ACmQ/rVlu7CmUxqM5njphSImAn22EyAAYRgMvaKC)
-- ============================================================================
DECLARE @IdRoleAdmin INT = (SELECT IdRole FROM Roles WHERE NamaRole = 'Admin');
DECLARE @IdRoleGuru INT = (SELECT IdRole FROM Roles WHERE NamaRole = 'Guru');
DECLARE @IdRoleSiswa INT = (SELECT IdRole FROM Roles WHERE NamaRole = 'Siswa');
DECLARE @PasswordHash NVARCHAR(500) = '$2a$11$lGeGjhXeJcTn1ACmQ/rVlu7CmUxqM5njphSImAn22EyAAYRgMvaKC';

-------------------------------------------------------------------------------
-- A. SUPER ADMIN (1 AKUN)
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleAdmin, 'Super Administrator', 'admin@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'admin@edupulse.com'), '081234567890', 'Main Administrator EduPulse LMS', 'Jakarta Pusat, DKI Jakarta', '1990-01-01', 'Laki-Laki');
END;

-------------------------------------------------------------------------------
-- B. GURU (5 AKUN)
-------------------------------------------------------------------------------
-- Guru 1
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'guru1@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleGuru, 'Budi Santoso, S.Kom., M.T.', 'guru1@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'guru1@edupulse.com'), '0812987654321', 'Pengajar Senior Pemrograman Web & Software Engineering', 'Bandung, Jawa Barat', '1985-05-15', 'Laki-Laki');
END;

-- Guru 2
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'guru2@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleGuru, 'Siti Rahmawati, M.Pd.', 'guru2@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'guru2@edupulse.com'), '0813876543210', 'Instruktur Literasi Digital & Bahasa Inggris Industri', 'Surakarta, Jawa Tengah', '1988-08-20', 'Perempuan');
END;

-- Guru 3
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'guru3@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleGuru, 'Dr. Ahmad Hidayat, M.Sc.', 'guru3@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'guru3@edupulse.com'), '0814765432109', 'Dosen & Researcher bidang Machine Learning & Data Science', 'Yogyakarta, DI Yogyakarta', '1982-11-10', 'Laki-Laki');
END;

-- Guru 4
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'guru4@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleGuru, 'Dewi Lestari, S.T., M.Kom.', 'guru4@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'guru4@edupulse.com'), '0815654321098', 'Product Designer & Lead UI/UX Mentor', 'Surabaya, Jawa Timur', '1991-03-25', 'Perempuan');
END;

-- Guru 5
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'guru5@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleGuru, 'Rizky Pratama, S.Kom.', 'guru5@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'guru5@edupulse.com'), '0816543210987', 'Network & Cyber Security Specialist', 'Malang, Jawa Timur', '1989-07-12', 'Laki-Laki');
END;

-------------------------------------------------------------------------------
-- C. SISWA (15 AKUN)
-------------------------------------------------------------------------------
-- Siswa 1
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa1@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Andi Wijaya', 'siswa1@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa1@edupulse.com'), '082100000001', 'Siswa antusias belajar Web Development', 'Jakarta Selatan', '2004-01-10', 'Laki-Laki');
END;

-- Siswa 2
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa2@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Anisa Putri', 'siswa2@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa2@edupulse.com'), '082100000002', 'Peserta kelas UI/UX Design', 'Depok, Jawa Barat', '2004-02-14', 'Perempuan');
END;

-- Siswa 3
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa3@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Bagas Kara', 'siswa3@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa3@edupulse.com'), '082100000003', 'Tertarik pada Backend & Cloud Computing', 'Bekasi, Jawa Barat', '2003-06-20', 'Laki-Laki');
END;

-- Siswa 4
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa4@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Citra Kirana', 'siswa4@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa4@edupulse.com'), '082100000004', 'Belajar Data Science & Python', 'Bogor, Jawa Barat', '2004-08-05', 'Perempuan');
END;

-- Siswa 5
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa5@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Daffa Rizky', 'siswa5@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa5@edupulse.com'), '082100000005', 'Siswa peminat Mobile App Development', 'Tangerang, Banten', '2003-12-01', 'Laki-Laki');
END;

-- Siswa 6
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa6@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Eka Novita', 'siswa6@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa6@edupulse.com'), '082100000006', 'Fokus pada Digital Marketing & Content', 'Bandung, Jawa Barat', '2004-03-18', 'Perempuan');
END;

-- Siswa 7
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa7@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Fajar Nugraha', 'siswa7@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa7@edupulse.com'), '082100000007', 'Pengembang game indie muda', 'Semarang, Jawa Tengah', '2003-09-30', 'Laki-Laki');
END;

-- Siswa 8
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa8@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Gita Gutawa', 'siswa8@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa8@edupulse.com'), '082100000008', 'Tertarik dengan Frontend Framework React & Vue', 'Surakarta, Jawa Tengah', '2004-05-22', 'Perempuan');
END;

-- Siswa 9
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa9@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Hadi Kurniawan', 'siswa9@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa9@edupulse.com'), '082100000009', 'Peminat Cyber Security & Networking', 'Yogyakarta', '2003-11-11', 'Laki-Laki');
END;

-- Siswa 10
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa10@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Indah Permata', 'siswa10@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa10@edupulse.com'), '082100000010', 'Belajar Database Management & SQL', 'Surabaya, Jawa Timur', '2004-07-07', 'Perempuan');
END;

-- Siswa 11
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa11@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Joko Susilo', 'siswa11@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa11@edupulse.com'), '082100000011', 'Siswa Rekayasa Perangkat Lunak', 'Malang, Jawa Timur', '2003-04-15', 'Laki-Laki');
END;

-- Siswa 12
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa12@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Kartika Sari', 'siswa12@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa12@edupulse.com'), '082100000012', 'Peserta kelas Quality Assurance & Testing', 'Denpasar, Bali', '2004-10-25', 'Perempuan');
END;

-- Siswa 13
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa13@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Lukman Hakim', 'siswa13@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa13@edupulse.com'), '082100000013', 'Tertarik dengan DevOps & CI/CD Pipeline', 'Medan, Sumatera Utara', '2003-01-08', 'Laki-Laki');
END;

-- Siswa 14
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa14@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Maya Anggraini', 'siswa14@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa14@edupulse.com'), '082100000014', 'Peserta Aktif kelas Data Analytics', 'Palembang, Sumatera Selatan', '2004-09-12', 'Perempuan');
END;

-- Siswa 15
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'siswa15@edupulse.com')
BEGIN
    INSERT INTO Users (IdRole, NamaLengkap, Email, PasswordHash, IsEmailVerified, IsActive, CreatedAt)
    VALUES (@IdRoleSiswa, 'Naufal Azhar', 'siswa15@edupulse.com', @PasswordHash, 1, 1, GETDATE());

    INSERT INTO Profiles (IdUser, NoHp, Bio, Alamat, TglLahir, JenisKelamin)
    VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa15@edupulse.com'), '082100000015', 'Belajar C# & ASP.NET Core Framework', 'Makassar, Sulawesi Selatan', '2003-05-04', 'Laki-Laki');
END;
GO

-- ============================================================================
-- 3. SEEDER TABEL KATEGORI (5 KATEGORI BIDANG STUDI)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM Kategori WHERE NamaKategori = 'Pemrograman & Software Engineering')
    INSERT INTO Kategori (NamaKategori, Deskripsi, CreatedAt)
    VALUES ('Pemrograman & Software Engineering', 'Mempelajari bahasa pemrograman modern, framework web, dan arsitektur rekayasa perangkat lunak.', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Kategori WHERE NamaKategori = 'Desain Grafis & UI/UX')
    INSERT INTO Kategori (NamaKategori, Deskripsi, CreatedAt)
    VALUES ('Desain Grafis & UI/UX', 'Panduan mendesain antarmuka pengguna (UI/UX) dan karya visual grafis interaktif.', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Kategori WHERE NamaKategori = 'Data Science & Artificial Intelligence')
    INSERT INTO Kategori (NamaKategori, Deskripsi, CreatedAt)
    VALUES ('Data Science & Artificial Intelligence', 'Pengolahan data, visualisasi data, statistik, dan algoritma Machine Learning.', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Kategori WHERE NamaKategori = 'Jaringan Komputer & Cyber Security')
    INSERT INTO Kategori (NamaKategori, Deskripsi, CreatedAt)
    VALUES ('Jaringan Komputer & Cyber Security', 'Konfigurasi jaringan, administrasi server, dan dasar keamanan cyber.', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Kategori WHERE NamaKategori = 'Bisnis Digital & Marketing')
    INSERT INTO Kategori (NamaKategori, Deskripsi, CreatedAt)
    VALUES ('Bisnis Digital & Marketing', 'Strategi pemasaran digital, kewirausahaan berbasis teknologi, dan pertumbuhan bisnis.', GETDATE());
GO

-- ============================================================================
-- 4. SEEDER TABEL KELAS (6 KELAS AKTIF DIAJAR OLEH 5 GURU)
-- ============================================================================
-- Kelas 1 (Guru 1 - Budi Santoso)
IF NOT EXISTS (SELECT 1 FROM Kelas WHERE NamaKelas = 'Full-Stack Web Development dengan ASP.NET Core & Tabler UI')
BEGIN
    INSERT INTO Kelas (IdKategori, IdGuru, NamaKelas, Deskripsi, Thumbnail, Level, Durasi, IsPublish, IsDeleted, CreatedAt)
    VALUES (
        (SELECT IdKategori FROM Kategori WHERE NamaKategori = 'Pemrograman & Software Engineering'),
        (SELECT IdUser FROM Users WHERE Email = 'guru1@edupulse.com'),
        'Full-Stack Web Development dengan ASP.NET Core & Tabler UI',
        'Kuasai pengembangan aplikasi web modern dari frontend hingga backend menggunakan C# ASP.NET dan Tabler UI Dashboard.',
        '/Content/images/kelas/web-dev.jpg',
        'Intermediate', 40, 1, 0, GETDATE()
    );
END;

-- Kelas 2 (Guru 2 - Siti Rahmawati)
IF NOT EXISTS (SELECT 1 FROM Kelas WHERE NamaKelas = 'Literasi Digital & Komunikasi Bahasa Inggris Bisnis')
BEGIN
    INSERT INTO Kelas (IdKategori, IdGuru, NamaKelas, Deskripsi, Thumbnail, Level, Durasi, IsPublish, IsDeleted, CreatedAt)
    VALUES (
        (SELECT IdKategori FROM Kategori WHERE NamaKategori = 'Bisnis Digital & Marketing'),
        (SELECT IdUser FROM Users WHERE Email = 'guru2@edupulse.com'),
        'Literasi Digital & Komunikasi Bahasa Inggris Bisnis',
        'Tingkatkan kemampuan komunikasi bahasa Inggris profesional dan literasi teknologi untuk lingkungan kerja modern.',
        '/Content/images/kelas/digital-english.jpg',
        'Beginner', 24, 1, 0, GETDATE()
    );
END;

-- Kelas 3 (Guru 3 - Dr. Ahmad Hidayat)
IF NOT EXISTS (SELECT 1 FROM Kelas WHERE NamaKelas = 'Data Science & Machine Learning dengan Python')
BEGIN
    INSERT INTO Kelas (IdKategori, IdGuru, NamaKelas, Deskripsi, Thumbnail, Level, Durasi, IsPublish, IsDeleted, CreatedAt)
    VALUES (
        (SELECT IdKategori FROM Kategori WHERE NamaKategori = 'Data Science & Artificial Intelligence'),
        (SELECT IdUser FROM Users WHERE Email = 'guru3@edupulse.com'),
        'Data Science & Machine Learning dengan Python',
        'Pembelajaran mendalam analisis data, komputasi numerik, dan algoritma Machine Learning menggunakan Python.',
        '/Content/images/kelas/data-science.jpg',
        'Advanced', 50, 1, 0, GETDATE()
    );
END;

-- Kelas 4 (Guru 4 - Dewi Lestari)
IF NOT EXISTS (SELECT 1 FROM Kelas WHERE NamaKelas = 'Mastering UI/UX Design: Figma dari Dasar hingga Prototyping')
BEGIN
    INSERT INTO Kelas (IdKategori, IdGuru, NamaKelas, Deskripsi, Thumbnail, Level, Durasi, IsPublish, IsDeleted, CreatedAt)
    VALUES (
        (SELECT IdKategori FROM Kategori WHERE NamaKategori = 'Desain Grafis & UI/UX'),
        (SELECT IdUser FROM Users WHERE Email = 'guru4@edupulse.com'),
        'Mastering UI/UX Design: Figma dari Dasar hingga Prototyping',
        'Panduan praktis perancangan wireframe, design system, dan prototype interaktif aplikasi menggunakan Figma.',
        '/Content/images/kelas/uiux-figma.jpg',
        'Beginner', 30, 1, 0, GETDATE()
    );
END;

-- Kelas 5 (Guru 5 - Rizky Pratama)
IF NOT EXISTS (SELECT 1 FROM Kelas WHERE NamaKelas = 'Network Security & Ethical Hacking Essentials')
BEGIN
    INSERT INTO Kelas (IdKategori, IdGuru, NamaKelas, Deskripsi, Thumbnail, Level, Durasi, IsPublish, IsDeleted, CreatedAt)
    VALUES (
        (SELECT IdKategori FROM Kategori WHERE NamaKategori = 'Jaringan Komputer & Cyber Security'),
        (SELECT IdUser FROM Users WHERE Email = 'guru5@edupulse.com'),
        'Network Security & Ethical Hacking Essentials',
        'Konsep keamanan jaringan, analisis kerentanan sistem, pencegahan serangan cyber, dan prinsip-prinsip Ethical Hacking.',
        '/Content/images/kelas/cyber-security.jpg',
        'Intermediate', 45, 1, 0, GETDATE()
    );
END;

-- Kelas 6 (Guru 1 - Budi Santoso)
IF NOT EXISTS (SELECT 1 FROM Kelas WHERE NamaKelas = 'Arsitektur Microservices & RESTful API dengan C#')
BEGIN
    INSERT INTO Kelas (IdKategori, IdGuru, NamaKelas, Deskripsi, Thumbnail, Level, Durasi, IsPublish, IsDeleted, CreatedAt)
    VALUES (
        (SELECT IdKategori FROM Kategori WHERE NamaKategori = 'Pemrograman & Software Engineering'),
        (SELECT IdUser FROM Users WHERE Email = 'guru1@edupulse.com'),
        'Arsitektur Microservices & RESTful API dengan C#',
        'Mendesain Web API berkinerja tinggi, arsitektur microservices, otentikasi JWT, dan integrasi database.',
        '/Content/images/kelas/microservices.jpg',
        'Advanced', 36, 1, 0, GETDATE()
    );
END;
GO

-- ============================================================================
-- 5. SEEDER TABEL MEMBERKELAS (PENDAFTARAN 15 SISWA KE 2-3 KELAS)
-- ============================================================================
DECLARE @K1 INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Full-Stack Web Development%');
DECLARE @K2 INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Literasi Digital%');
DECLARE @K3 INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Data Science%');
DECLARE @K4 INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Mastering UI/UX%');
DECLARE @K5 INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Network Security%');
DECLARE @K6 INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Arsitektur Microservices%');

-- Siswa 1
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa1@edupulse.com') AND IdKelas = @K1)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa1@edupulse.com'), @K1, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa1@edupulse.com') AND IdKelas = @K4)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa1@edupulse.com'), @K4, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa1@edupulse.com') AND IdKelas = @K6)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa1@edupulse.com'), @K6, GETDATE(), 'Active');

-- Siswa 2
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa2@edupulse.com') AND IdKelas = @K2)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa2@edupulse.com'), @K2, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa2@edupulse.com') AND IdKelas = @K4)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa2@edupulse.com'), @K4, GETDATE(), 'Active');

-- Siswa 3
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa3@edupulse.com') AND IdKelas = @K1)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa3@edupulse.com'), @K1, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa3@edupulse.com') AND IdKelas = @K5)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa3@edupulse.com'), @K5, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa3@edupulse.com') AND IdKelas = @K6)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa3@edupulse.com'), @K6, GETDATE(), 'Active');

-- Siswa 4
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa4@edupulse.com') AND IdKelas = @K3)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa4@edupulse.com'), @K3, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa4@edupulse.com') AND IdKelas = @K2)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa4@edupulse.com'), @K2, GETDATE(), 'Active');

-- Siswa 5
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa5@edupulse.com') AND IdKelas = @K1)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa5@edupulse.com'), @K1, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa5@edupulse.com') AND IdKelas = @K4)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa5@edupulse.com'), @K4, GETDATE(), 'Active');

-- Siswa 6
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa6@edupulse.com') AND IdKelas = @K2)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa6@edupulse.com'), @K2, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa6@edupulse.com') AND IdKelas = @K4)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa6@edupulse.com'), @K4, GETDATE(), 'Active');

-- Siswa 7
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa7@edupulse.com') AND IdKelas = @K1)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa7@edupulse.com'), @K1, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa7@edupulse.com') AND IdKelas = @K5)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa7@edupulse.com'), @K5, GETDATE(), 'Active');

-- Siswa 8
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa8@edupulse.com') AND IdKelas = @K4)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa8@edupulse.com'), @K4, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa8@edupulse.com') AND IdKelas = @K1)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa8@edupulse.com'), @K1, GETDATE(), 'Active');

-- Siswa 9
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa9@edupulse.com') AND IdKelas = @K5)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa9@edupulse.com'), @K5, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa9@edupulse.com') AND IdKelas = @K6)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa9@edupulse.com'), @K6, GETDATE(), 'Active');

-- Siswa 10
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa10@edupulse.com') AND IdKelas = @K3)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa10@edupulse.com'), @K3, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa10@edupulse.com') AND IdKelas = @K1)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa10@edupulse.com'), @K1, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa10@edupulse.com') AND IdKelas = @K6)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa10@edupulse.com'), @K6, GETDATE(), 'Active');

-- Siswa 11
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa11@edupulse.com') AND IdKelas = @K1)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa11@edupulse.com'), @K1, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa11@edupulse.com') AND IdKelas = @K6)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa11@edupulse.com'), @K6, GETDATE(), 'Active');

-- Siswa 12
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa12@edupulse.com') AND IdKelas = @K2)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa12@edupulse.com'), @K2, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa12@edupulse.com') AND IdKelas = @K4)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa12@edupulse.com'), @K4, GETDATE(), 'Active');

-- Siswa 13
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa13@edupulse.com') AND IdKelas = @K5)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa13@edupulse.com'), @K5, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa13@edupulse.com') AND IdKelas = @K6)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa13@edupulse.com'), @K6, GETDATE(), 'Active');

-- Siswa 14
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa14@edupulse.com') AND IdKelas = @K3)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa14@edupulse.com'), @K3, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa14@edupulse.com') AND IdKelas = @K2)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa14@edupulse.com'), @K2, GETDATE(), 'Active');

-- Siswa 15
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa15@edupulse.com') AND IdKelas = @K1)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa15@edupulse.com'), @K1, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa15@edupulse.com') AND IdKelas = @K3)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa15@edupulse.com'), @K3, GETDATE(), 'Active');
IF NOT EXISTS (SELECT 1 FROM MemberKelas WHERE IdSiswa = (SELECT IdUser FROM Users WHERE Email = 'siswa15@edupulse.com') AND IdKelas = @K6)
    INSERT INTO MemberKelas (IdSiswa, IdKelas, TglJoin, Status) VALUES ((SELECT IdUser FROM Users WHERE Email = 'siswa15@edupulse.com'), @K6, GETDATE(), 'Active');
GO

-- ============================================================================
-- 6. SEEDER TABEL MATERI & PROGRESMATERI
-- ============================================================================
DECLARE @K1_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Full-Stack Web Development%');
DECLARE @K2_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Literasi Digital%');
DECLARE @K3_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Data Science%');
DECLARE @K4_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Mastering UI/UX%');
DECLARE @K5_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Network Security%');
DECLARE @K6_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Arsitektur Microservices%');

-- Kelas 1 Materi
IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K1_Id AND PertemuanKe = 1)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K1_Id, 1, 'Pengenalan ASP.NET Core MVC & Arsitektur Project', 'Modul dasar pengenalan struktur folder dan alur MVC di ASP.NET.', 'modul1-aspnet.pdf', '/Uploads/materi/modul1-aspnet.pdf', 'pdf', 2450000, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K1_Id AND PertemuanKe = 2)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K1_Id, 2, 'Entity Framework 6 Code First & Data Annotations', 'Penjelasan detail pemetaan entity, DbContext, dan migrasi database.', 'modul2-ef6.docx', '/Uploads/materi/modul2-ef6.docx', 'docx', 1850000, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K1_Id AND PertemuanKe = 3)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K1_Id, 3, 'Integrasi Tabler UI Dashboard & Dynamic Views', 'Tutorial pembuatan dashboard interaktif dengan Tabler UI.', 'modul3-tabler.mp4', '/Uploads/materi/modul3-tabler.mp4', 'mp4', 15400000, GETDATE());

-- Kelas 2 Materi
IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K2_Id AND PertemuanKe = 1)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K2_Id, 1, 'Etika Pembelajaran Digital & Komunikasi Profesional', 'Panduan berkomunikasi efektif di lingkungan kerja digital.', 'modul1-etika.pdf', '/Uploads/materi/modul1-etika.pdf', 'pdf', 1200000, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K2_Id AND PertemuanKe = 2)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K2_Id, 2, 'Bahasa Inggris Industri & Penulisan Email Bisnis', 'Format dan kosakata penting untuk email bisnis internasional.', 'modul2-english.pdf', '/Uploads/materi/modul2-english.pdf', 'pdf', 1450000, GETDATE());

-- Kelas 3 Materi
IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K3_Id AND PertemuanKe = 1)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K3_Id, 1, 'Pengenalan NumPy & Pandas untuk Data Wrangler', 'Teknik eksplorasi dan manipulasi data array & dataframe.', 'modul1-pandas.pdf', '/Uploads/materi/modul1-pandas.pdf', 'pdf', 3100000, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K3_Id AND PertemuanKe = 2)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K3_Id, 2, 'Visualisasi Data Interaktif dengan Matplotlib & Seaborn', 'Panduan membuat grafik visualisasi statistik data yang efektif.', 'modul2-visualization.pdf', '/Uploads/materi/modul2-visualization.pdf', 'pdf', 2800000, GETDATE());

-- Kelas 4 Materi
IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K4_Id AND PertemuanKe = 1)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K4_Id, 1, 'Prinsip Desain Antarmuka & Design System', 'Konsep hierarki visual, warna, dan tipografi dalam UI.', 'modul1-ui-principles.pdf', '/Uploads/materi/modul1-ui-principles.pdf', 'pdf', 4200000, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K4_Id AND PertemuanKe = 2)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K4_Id, 2, 'Wireframing & Low-Fidelity Layout di Figma', 'Langkah-langkah pembuatan wireframe halaman web dan aplikasi mobile.', 'modul2-figma-wireframe.pdf', '/Uploads/materi/modul2-figma-wireframe.pdf', 'pdf', 3600000, GETDATE());

-- Kelas 5 Materi
IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K5_Id AND PertemuanKe = 1)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K5_Id, 1, 'Dasar Protokol Jaringan & Model OSI Layer', 'Pemahaman 7 layer OSI dan protokol TCP/IP.', 'modul1-osi-layer.pdf', '/Uploads/materi/modul1-osi-layer.pdf', 'pdf', 1900000, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K5_Id AND PertemuanKe = 2)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K5_Id, 2, 'Metodologi Reconnaissance & Scanning Port dengan Nmap', 'Teknik pemetaan port aktif dan identifikasi layanan server.', 'modul2-nmap-recon.pdf', '/Uploads/materi/modul2-nmap-recon.pdf', 'pdf', 2700000, GETDATE());

-- Kelas 6 Materi
IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K6_Id AND PertemuanKe = 1)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K6_Id, 1, 'Prinsip RESTful API & Desain Controller', 'Merancang endpoint API yang bersih dan konsisten.', 'modul1-rest-api.pdf', '/Uploads/materi/modul1-rest-api.pdf', 'pdf', 2100000, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Materi WHERE IdKelas = @K6_Id AND PertemuanKe = 2)
    INSERT INTO Materi (IdKelas, PertemuanKe, JudulMateri, Deskripsi, NamaFile, FilePath, TipeMateri, FileSize, CreatedAt)
    VALUES (@K6_Id, 2, 'Otentikasi JWT & Middleware Authorization', 'Pengamanan Web API menggunakan JSON Web Tokens (JWT).', 'modul2-jwt-auth.pdf', '/Uploads/materi/modul2-jwt-auth.pdf', 'pdf', 2900000, GETDATE());
GO

-- Seeder ProgresMateri
INSERT INTO ProgresMateri (IdMateri, IdSiswa, IsSelesai, DiselesaikanPada)
SELECT m.IdMateri, mk.IdSiswa, 1, DATEADD(DAY, -1, GETDATE())
FROM Materi m
JOIN MemberKelas mk ON m.IdKelas = mk.IdKelas
WHERE NOT EXISTS (SELECT 1 FROM ProgresMateri pm WHERE pm.IdMateri = m.IdMateri AND pm.IdSiswa = mk.IdSiswa);
GO

-- ============================================================================
-- 7. SEEDER TABEL JADWALABSEN & ABSENSI
-- ============================================================================
DECLARE @K1_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Full-Stack Web Development%');
DECLARE @K2_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Literasi Digital%');
DECLARE @K3_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Data Science%');
DECLARE @K4_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Mastering UI/UX%');
DECLARE @K5_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Network Security%');
DECLARE @K6_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Arsitektur Microservices%');

-- Jadwal Sesi Pertemuan 1 (Lampau)
IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K1_Id AND PertemuanKe = 1)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K1_Id, 1, CAST(DATEADD(DAY, -7, GETDATE()) AS DATE), '08:00:00', '10:00:00', 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K2_Id AND PertemuanKe = 1)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K2_Id, 1, CAST(DATEADD(DAY, -7, GETDATE()) AS DATE), '10:30:00', '12:30:00', 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K3_Id AND PertemuanKe = 1)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K3_Id, 1, CAST(DATEADD(DAY, -7, GETDATE()) AS DATE), '13:30:00', '15:30:00', 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K4_Id AND PertemuanKe = 1)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K4_Id, 1, CAST(DATEADD(DAY, -7, GETDATE()) AS DATE), '08:00:00', '10:00:00', 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K5_Id AND PertemuanKe = 1)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K5_Id, 1, CAST(DATEADD(DAY, -7, GETDATE()) AS DATE), '10:30:00', '12:30:00', 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K6_Id AND PertemuanKe = 1)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K6_Id, 1, CAST(DATEADD(DAY, -7, GETDATE()) AS DATE), '13:30:00', '15:30:00', 0, GETDATE());

-- Jadwal Sesi Pertemuan 2 (Hari Ini - IsOpen = 1)
IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K1_Id AND PertemuanKe = 2)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K1_Id, 2, CAST(GETDATE() AS DATE), '08:00:00', '16:00:00', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K2_Id AND PertemuanKe = 2)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K2_Id, 2, CAST(GETDATE() AS DATE), '08:00:00', '16:00:00', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K3_Id AND PertemuanKe = 2)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K3_Id, 2, CAST(GETDATE() AS DATE), '08:00:00', '16:00:00', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K4_Id AND PertemuanKe = 2)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K4_Id, 2, CAST(GETDATE() AS DATE), '08:00:00', '16:00:00', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K5_Id AND PertemuanKe = 2)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K5_Id, 2, CAST(GETDATE() AS DATE), '08:00:00', '16:00:00', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM JadwalAbsen WHERE IdKelas = @K6_Id AND PertemuanKe = 2)
    INSERT INTO JadwalAbsen (IdKelas, PertemuanKe, Tanggal, JamMulai, JamSelesai, IsOpen, CreatedAt)
    VALUES (@K6_Id, 2, CAST(GETDATE() AS DATE), '08:00:00', '16:00:00', 1, GETDATE());
GO

-- Seeder Absensi untuk seluruh siswa yang terdaftar di JadwalAbsen
INSERT INTO Absensi (IdJadwal, IdSiswa, WaktuAbsen, Status, Catatan)
SELECT j.IdJadwal, mk.IdSiswa, DATEADD(MINUTE, (mk.IdSiswa * 3), j.CreatedAt), 'Hadir', 'Hadir tepat waktu'
FROM JadwalAbsen j
JOIN MemberKelas mk ON j.IdKelas = mk.IdKelas
WHERE NOT EXISTS (SELECT 1 FROM Absensi a WHERE a.IdJadwal = j.IdJadwal AND a.IdSiswa = mk.IdSiswa);
GO

-- ============================================================================
-- 8. SEEDER TABEL QUIZ, SOALQUIZ, OPSIJAWABAN & NILAIQUIZ
-- ============================================================================
DECLARE @K1_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Full-Stack Web Development%');

-- Quiz 1 (Kelas 1)
IF NOT EXISTS (SELECT 1 FROM Quiz WHERE IdKelas = @K1_Id AND PertemuanKe = 2)
BEGIN
    INSERT INTO Quiz (IdKelas, PertemuanKe, JudulQuiz, Deskripsi, Durasi, PassingScore, WaktuMulai, WaktuSelesai, IsActive, CreatedAt)
    VALUES (@K1_Id, 2, 'Kuis Evaluasi Modul ASP.NET & Entity Framework', 'Uji pemahaman arsitektur MVC dan query LINQ to Entities.', 30, 75.00, DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, 7, GETDATE()), 1, GETDATE());

    DECLARE @Q1_Id INT = (SELECT IdQuiz FROM Quiz WHERE IdKelas = @K1_Id AND PertemuanKe = 2);

    -- Soal 1
    INSERT INTO SoalQuiz (IdQuiz, Pertanyaan, TipeSoal) VALUES (@Q1_Id, 'Manakah atribut Entity Framework yang digunakan untuk menentukan Primary Key?', 'PilihanGanda');
    DECLARE @S1 INT = SCOPE_IDENTITY();
    INSERT INTO OpsiJawaban (IdSoal, TeksOpsi, IsCorrect) VALUES 
    (@S1, '[Key]', 1), (@S1, '[Table]', 0), (@S1, '[ForeignKey]', 0), (@S1, '[Required]', 0);

    -- Soal 2
    INSERT INTO SoalQuiz (IdQuiz, Pertanyaan, TipeSoal) VALUES (@Q1_Id, 'Komponen ASP.NET MVC yang bertindak sebagai jembatan antara Model dan View adalah?', 'PilihanGanda');
    DECLARE @S2 INT = SCOPE_IDENTITY();
    INSERT INTO OpsiJawaban (IdSoal, TeksOpsi, IsCorrect) VALUES 
    (@S2, 'Controller', 1), (@S2, 'DbContext', 0), (@S2, 'BundleConfig', 0), (@S2, 'RouteTable', 0);

    -- Soal 3
    INSERT INTO SoalQuiz (IdQuiz, Pertanyaan, TipeSoal) VALUES (@Q1_Id, 'Manakah perintah LINQ yang digunakan untuk mengambil data pertama atau null jika kosong?', 'PilihanGanda');
    DECLARE @S3 INT = SCOPE_IDENTITY();
    INSERT INTO OpsiJawaban (IdSoal, TeksOpsi, IsCorrect) VALUES 
    (@S3, 'FirstOrDefault()', 1), (@S3, 'Select()', 0), (@S3, 'Where()', 0), (@S3, 'ToList()', 0);

    -- Soal 4
    INSERT INTO SoalQuiz (IdQuiz, Pertanyaan, TipeSoal) VALUES (@Q1_Id, 'Properti DbContext mana yang digunakan untuk menonaktifkan pemuatan otomatis relasi?', 'PilihanGanda');
    DECLARE @S4 INT = SCOPE_IDENTITY();
    INSERT INTO OpsiJawaban (IdSoal, TeksOpsi, IsCorrect) VALUES 
    (@S4, 'Configuration.LazyLoadingEnabled = false', 1), (@S4, 'Configuration.AutoSave = false', 0), (@S4, 'Configuration.CascadeDelete = false', 0), (@S4, 'Configuration.ProxyDisabled = true', 0);

    -- Soal 5
    INSERT INTO SoalQuiz (IdQuiz, Pertanyaan, TipeSoal) VALUES (@Q1_Id, 'Metode HTTP manakah yang paling tepat digunakan untuk memperbarui data yang sudah ada?', 'PilihanGanda');
    DECLARE @S5 INT = SCOPE_IDENTITY();
    INSERT INTO OpsiJawaban (IdSoal, TeksOpsi, IsCorrect) VALUES 
    (@S5, 'PUT / POST', 1), (@S5, 'GET', 0), (@S5, 'DELETE', 0), (@S5, 'OPTIONS', 0);
END;
GO

-- Seeder NilaiQuiz untuk siswa yang mengerjakan kuis
INSERT INTO NilaiQuiz (IdQuiz, IdSiswa, Score, WaktuMulai, WaktuSubmit, Status)
SELECT q.IdQuiz, mk.IdSiswa, 90.00, DATEADD(MINUTE, -25, GETDATE()), GETDATE(), 'Submitted'
FROM Quiz q
JOIN MemberKelas mk ON q.IdKelas = mk.IdKelas
WHERE NOT EXISTS (SELECT 1 FROM NilaiQuiz nq WHERE nq.IdQuiz = q.IdQuiz AND nq.IdSiswa = mk.IdSiswa);
GO

-- ============================================================================
-- 9. SEEDER TABEL TUGAS & PENGUMPULANTUGAS
-- ============================================================================
DECLARE @K1_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Full-Stack Web Development%');
DECLARE @K2_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Literasi Digital%');
DECLARE @K3_Id INT = (SELECT IdKelas FROM Kelas WHERE NamaKelas LIKE 'Data Science%');

IF NOT EXISTS (SELECT 1 FROM Tugas WHERE IdKelas = @K1_Id AND PertemuanKe = 2)
    INSERT INTO Tugas (IdKelas, PertemuanKe, JudulTugas, Deskripsi, Deadline, FileAttachment, IsDeleted, CreatedAt)
    VALUES (@K1_Id, 2, 'Tugas 1: Pembuatan Class Entity & DbContext', 'Buatlah seluruh class entity beserta DbContext sesuai spesifikasi schema database LMS.', DATEADD(DAY, 7, GETDATE()), '/Uploads/tugas/soal-tugas1-aspnet.pdf', 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Tugas WHERE IdKelas = @K2_Id AND PertemuanKe = 2)
    INSERT INTO Tugas (IdKelas, PertemuanKe, JudulTugas, Deskripsi, Deadline, FileAttachment, IsDeleted, CreatedAt)
    VALUES (@K2_Id, 2, 'Tugas 1: Draft Email Bisnis Profesional', 'Tuliskan draft email pengajuan kerjasama bisnis dalam Bahasa Inggris.', DATEADD(DAY, 7, GETDATE()), '/Uploads/tugas/soal-tugas1-english.pdf', 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Tugas WHERE IdKelas = @K3_Id AND PertemuanKe = 2)
    INSERT INTO Tugas (IdKelas, PertemuanKe, JudulTugas, Deskripsi, Deadline, FileAttachment, IsDeleted, CreatedAt)
    VALUES (@K3_Id, 2, 'Tugas 1: Analisis Dataset Eksploratif dengan Pandas', 'Lakukan analisis statistik dan pembersihan data pada dataset yang disediakan.', DATEADD(DAY, 7, GETDATE()), '/Uploads/tugas/soal-tugas1-pandas.pdf', 0, GETDATE());
GO

-- Seeder PengumpulanTugas
INSERT INTO PengumpulanTugas (IdTugas, IdSiswa, FilePath, WaktuKumpul, Nilai, Feedback, Status, CreatedAt)
SELECT t.IdTugas, mk.IdSiswa, '/Uploads/pengumpulan/jawaban_siswa_' + CAST(mk.IdSiswa AS VARCHAR) + '_tugas_' + CAST(t.IdTugas AS VARCHAR) + '.zip', DATEADD(DAY, -1, GETDATE()), 92.50, 'Sangat baik, struktur kode dan penjelasan lengkap.', 'Graded', GETDATE()
FROM Tugas t
JOIN MemberKelas mk ON t.IdKelas = mk.IdKelas
WHERE NOT EXISTS (SELECT 1 FROM PengumpulanTugas pt WHERE pt.IdTugas = t.IdTugas AND pt.IdSiswa = mk.IdSiswa);
GO

-- ============================================================================
-- 10. SEEDER TABEL STREAMPOSTINGAN & NOTIFIKASI
-- ============================================================================
INSERT INTO StreamPostingan (IdKelas, IdUser, Pesan, AttachmentUrl, CreatedAt)
SELECT k.IdKelas, k.IdGuru, 'Selamat datang di kelas ' + k.NamaKelas + '. Silakan pelajari materi pertemuan 1 dan periksa jadwal absensi.', '/Uploads/stream/welcome_announcement.png', GETDATE()
FROM Kelas k
WHERE NOT EXISTS (SELECT 1 FROM StreamPostingan sp WHERE sp.IdKelas = k.IdKelas AND sp.IdUser = k.IdGuru);

INSERT INTO Notifikasi (IdUser, Judul, Pesan, TipeNotif, UrlTujuan, IsRead, CreatedAt)
SELECT u.IdUser, 'Selamat Datang di EduPulse LMS', 'Akun Anda telah aktif. Selamat belajar!', 'Info', '/Dashboard', 0, GETDATE()
FROM Users u
WHERE NOT EXISTS (SELECT 1 FROM Notifikasi n WHERE n.IdUser = u.IdUser AND n.Judul = 'Selamat Datang di EduPulse LMS');
GO

