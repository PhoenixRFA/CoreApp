using Microsoft.EntityFrameworkCore;

namespace EFCoreApp.Model
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<UserData> UserData { get; set; }
        public DbSet<Language> Languages { get; set; }


        private readonly bool _configuredFromDbContextOptions = false;

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        public ApplicationContext(DbContextOptions options) : base(options)
        {
            _configuredFromDbContextOptions = true;

            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_configuredFromDbContextOptions) return;

            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=testappdb;Trusted_connection=True;")
                .LogTo(System.Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
