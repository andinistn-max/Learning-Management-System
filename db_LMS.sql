CREATE DATABASE db_Learning_Management_System;
USE db_Learning_Management_System;


CREATE TABLE Roles (
    IdRole INT IDENTITY(1,1) PRIMARY KEY,
    NamaRole NVARCHAR(50) NOT NULL UNIQUE,
    Deskripsi NVARCHAR(255) NULL
);

INSERT INTO Roles (NamaRole, Deskripsi) VALUES
('Admin', 'Administrator Sistem'),
('Guru', 'Pengajar Kelas'),
('Siswa', 'Peserta Kelas');


CREATE TABLE Users (
    IdUser INT IDENTITY(1,1) PRIMARY KEY,
    IdRole INT NOT NULL,
    NamaLengkap NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NULL,
    GoogleId NVARCHAR(200) NULL UNIQUE,
    FotoProfile NVARCHAR(500) NULL,
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    ResetPasswordToken NVARCHAR(255) NULL,
    ResetPasswordExpiry DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (IdRole) REFERENCES Roles(IdRole)
);

CREATE TABLE Profiles (
    IdUser INT PRIMARY KEY,
    NoHp NVARCHAR(30) NULL,
    Bio NVARCHAR(500) NULL,
    Alamat NVARCHAR(500) NULL,
    TglLahir DATE NULL,
    JenisKelamin NVARCHAR(20) NULL,
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Profiles_Users FOREIGN KEY (IdUser) REFERENCES Users(IdUser) ON DELETE CASCADE
);

CREATE TABLE Kategori (
    IdKategori INT IDENTITY(1,1) PRIMARY KEY,
    NamaKategori NVARCHAR(100) NOT NULL UNIQUE,
    Deskripsi NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

CREATE TABLE Kelas (
    IdKelas INT IDENTITY(1,1) PRIMARY KEY,
    IdKategori INT NOT NULL,
    IdGuru INT NOT NULL,
    NamaKelas NVARCHAR(200) NOT NULL,
    Deskripsi NVARCHAR(MAX) NULL,
    Thumbnail NVARCHAR(500) NULL,
    Level NVARCHAR(30) NULL,
    Durasi INT NULL,
    IsPublish BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Kelas_Kategori FOREIGN KEY (IdKategori) REFERENCES Kategori(IdKategori),
    CONSTRAINT FK_Kelas_Guru FOREIGN KEY (IdGuru) REFERENCES Users(IdUser)
);

CREATE TABLE MemberKelas (
    IdMember INT IDENTITY(1,1) PRIMARY KEY,
    IdSiswa INT NOT NULL,
    IdKelas INT NOT NULL,
    TglJoin DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(30) NOT NULL DEFAULT 'Active',
    CONSTRAINT FK_MemberKelas_Siswa FOREIGN KEY (IdSiswa) REFERENCES Users(IdUser),
    CONSTRAINT FK_MemberKelas_Kelas FOREIGN KEY (IdKelas) REFERENCES Kelas(IdKelas),
    CONSTRAINT UQ_MemberKelas UNIQUE(IdSiswa, IdKelas)
);

CREATE TABLE Materi (
    IdMateri INT IDENTITY(1,1) PRIMARY KEY,
    IdKelas INT NOT NULL,
    PertemuanKe INT NOT NULL,
    JudulMateri NVARCHAR(200) NOT NULL,
    Deskripsi NVARCHAR(MAX) NULL,
    NamaFile NVARCHAR(255) NULL,
    FilePath NVARCHAR(500) NULL,
    TipeMateri NVARCHAR(50) NULL,
    FileSize BIGINT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Materi_Kelas FOREIGN KEY (IdKelas) REFERENCES Kelas(IdKelas) ON DELETE CASCADE
);

CREATE TABLE ProgresMateri (
    IdProgres INT IDENTITY(1,1) PRIMARY KEY,
    IdMateri INT NOT NULL,
    IdSiswa INT NOT NULL,
    IsSelesai BIT NOT NULL DEFAULT 0,
    DiselesaikanPada DATETIME NULL,
    CONSTRAINT FK_ProgresMateri_Materi FOREIGN KEY (IdMateri) REFERENCES Materi(IdMateri) ON DELETE CASCADE,
    CONSTRAINT FK_ProgresMateri_Siswa FOREIGN KEY (IdSiswa) REFERENCES Users(IdUser),
    CONSTRAINT UQ_ProgresMateri UNIQUE(IdMateri, IdSiswa)
);

CREATE TABLE JadwalAbsen (
    IdJadwal INT IDENTITY(1,1) PRIMARY KEY,
    IdKelas INT NOT NULL,
    PertemuanKe INT NOT NULL,
    Tanggal DATE NOT NULL,
    JamMulai TIME NOT NULL,
    JamSelesai TIME NOT NULL,
    TokenPresensi NVARCHAR(20) NULL,
    IsOpen BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_JadwalAbsen_Kelas FOREIGN KEY (IdKelas) REFERENCES Kelas(IdKelas) ON DELETE CASCADE
);

CREATE TABLE Absensi (
    IdAbsen INT IDENTITY(1,1) PRIMARY KEY,
    IdJadwal INT NOT NULL,
    IdSiswa INT NOT NULL,
    WaktuAbsen DATETIME NULL,
    Status NVARCHAR(30) NOT NULL,
    Catatan NVARCHAR(500) NULL,
    CONSTRAINT FK_Absensi_Jadwal FOREIGN KEY (IdJadwal) REFERENCES JadwalAbsen(IdJadwal) ON DELETE CASCADE,
    CONSTRAINT FK_Absensi_Siswa FOREIGN KEY (IdSiswa) REFERENCES Users(IdUser),
    CONSTRAINT UQ_Absensi UNIQUE(IdJadwal, IdSiswa)
);

CREATE TABLE Quiz (
    IdQuiz INT IDENTITY(1,1) PRIMARY KEY,
    IdKelas INT NOT NULL,
    PertemuanKe INT NOT NULL,
    JudulQuiz NVARCHAR(200) NOT NULL,
    Deskripsi NVARCHAR(MAX) NULL,
    Durasi INT NULL,
    PassingScore DECIMAL(5,2) NOT NULL DEFAULT 75,
    WaktuMulai DATETIME NULL,
    WaktuSelesai DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Quiz_Kelas FOREIGN KEY (IdKelas) REFERENCES Kelas(IdKelas) ON DELETE CASCADE
);

CREATE TABLE SoalQuiz (
    IdSoal INT IDENTITY(1,1) PRIMARY KEY,
    IdQuiz INT NOT NULL,
    Pertanyaan NVARCHAR(MAX) NOT NULL,
    TipeSoal NVARCHAR(30) NOT NULL DEFAULT 'PilihanGanda',
    CONSTRAINT FK_SoalQuiz_Quiz FOREIGN KEY (IdQuiz) REFERENCES Quiz(IdQuiz) ON DELETE CASCADE
);

CREATE TABLE OpsiJawaban (
    IdOpsi INT IDENTITY(1,1) PRIMARY KEY,
    IdSoal INT NOT NULL,
    TeksOpsi NVARCHAR(500) NOT NULL,
    IsCorrect BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_OpsiJawaban_Soal FOREIGN KEY (IdSoal) REFERENCES SoalQuiz(IdSoal) ON DELETE CASCADE
);

CREATE TABLE NilaiQuiz (
    IdAttempt INT IDENTITY(1,1) PRIMARY KEY,
    IdQuiz INT NOT NULL,
    IdSiswa INT NOT NULL,
    Score DECIMAL(5,2) NULL,
    WaktuMulai DATETIME NULL,
    WaktuSubmit DATETIME NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'InProgress',
    CONSTRAINT FK_NilaiQuiz_Quiz FOREIGN KEY (IdQuiz) REFERENCES Quiz(IdQuiz),
    CONSTRAINT FK_NilaiQuiz_Siswa FOREIGN KEY (IdSiswa) REFERENCES Users(IdUser)
);

CREATE TABLE Tugas (
    IdTugas INT IDENTITY(1,1) PRIMARY KEY,
    IdKelas INT NOT NULL,
    PertemuanKe INT NOT NULL,
    JudulTugas NVARCHAR(200) NOT NULL,
    Deskripsi NVARCHAR(MAX) NULL,
    Deadline DATETIME NOT NULL,
    FileAttachment NVARCHAR(500) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Tugas_Kelas FOREIGN KEY (IdKelas) REFERENCES Kelas(IdKelas) ON DELETE CASCADE
);

CREATE TABLE PengumpulanTugas (
    IdKumpul INT IDENTITY(1,1) PRIMARY KEY,
    IdTugas INT NOT NULL,
    IdSiswa INT NOT NULL,
    FilePath NVARCHAR(500) NULL,
    WaktuKumpul DATETIME NULL,
    Nilai DECIMAL(5,2) NULL,
    Feedback NVARCHAR(MAX) NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Submitted',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_PengumpulanTugas_Tugas FOREIGN KEY (IdTugas) REFERENCES Tugas(IdTugas) ON DELETE CASCADE,
    CONSTRAINT FK_PengumpulanTugas_Siswa FOREIGN KEY (IdSiswa) REFERENCES Users(IdUser),
    CONSTRAINT UQ_PengumpulanTugas UNIQUE(IdTugas, IdSiswa)
);

CREATE TABLE StreamPostingan (
    IdStream INT IDENTITY(1,1) PRIMARY KEY,
    IdKelas INT NOT NULL,
    IdUser INT NOT NULL,
    Pesan NVARCHAR(MAX) NOT NULL,
    AttachmentUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Stream_Kelas FOREIGN KEY (IdKelas) REFERENCES Kelas(IdKelas) ON DELETE CASCADE,
    CONSTRAINT FK_Stream_Users FOREIGN KEY (IdUser) REFERENCES Users(IdUser)
);

CREATE TABLE KomentarStream (
    IdKomentar INT IDENTITY(1,1) PRIMARY KEY,
    IdStream INT NOT NULL,
    IdUser INT NOT NULL,
    Komentar NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_KomentarStream_Stream FOREIGN KEY (IdStream) REFERENCES StreamPostingan(IdStream) ON DELETE CASCADE,
    CONSTRAINT FK_KomentarStream_Users FOREIGN KEY (IdUser) REFERENCES Users(IdUser)
);

CREATE TABLE Notifikasi (
    IdNotif INT IDENTITY(1,1) PRIMARY KEY,
    IdUser INT NOT NULL,
    Judul NVARCHAR(200) NOT NULL,
    Pesan NVARCHAR(500) NULL,
    TipeNotif NVARCHAR(50) NULL,
    UrlTujuan NVARCHAR(500) NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Notifikasi_Users FOREIGN KEY (IdUser) REFERENCES Users(IdUser)
);