using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Learning_Management_System.Models.Entity;

namespace Learning_Management_System.Services.Context
{
    public class LmsDbContext : DbContext
    {
        public LmsDbContext() : base("name=DbLmsContext")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Roles> Roles { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Profiles> Profiles { get; set; }
        public DbSet<Kategori> Kategori { get; set; }
        public DbSet<Kelas> Kelas { get; set; }
        public DbSet<MemberKelas> MemberKelas { get; set; }
        public DbSet<Materi> Materi { get; set; }
        public DbSet<ProgresMateri> ProgresMateri { get; set; }
        public DbSet<JadwalAbsen> JadwalAbsen { get; set; }
        public DbSet<Absensi> Absensi { get; set; }
        public DbSet<Quiz> Quiz { get; set; }
        public DbSet<SoalQuiz> SoalQuiz { get; set; }
        public DbSet<OpsiJawaban> OpsiJawaban { get; set; }
        public DbSet<NilaiQuiz> NilaiQuiz { get; set; }
        public DbSet<Tugas> Tugas { get; set; }
        public DbSet<PengumpulanTugas> PengumpulanTugas { get; set; }
        public DbSet<StreamPostingan> StreamPostingan { get; set; }
        public DbSet<KomentarStream> KomentarStream { get; set; }
        public DbSet<Notifikasi> Notifikasi { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
