using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace MVCApp
{
    public partial class TestappdbContext : DbContext
    {
        public TestappdbContext() { }

        public TestappdbContext(DbContextOptions<TestappdbContext> options) : base(options) { }

        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<LanguageUser> LanguageUsers { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserDatum> UserData { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<LanguageUser>(entity =>
            {
                entity.HasKey(e => new { e.LanguagesId, e.UsersId });

                entity.ToTable("LanguageUser");

                entity.HasIndex(e => e.UsersId, "IX_LanguageUser_UsersId");

                entity.HasOne(d => d.Languages)
                    .WithMany(p => p.LanguageUsers)
                    .HasForeignKey(d => d.LanguagesId);

                entity.HasOne(d => d.Users)
                    .WithMany(p => p.LanguageUsers)
                    .HasForeignKey(d => d.UsersId);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.CompanyId, "IX_Users_CompanyId");

                entity.HasIndex(e => e.UserDataId, "IX_Users_UserDataId");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.CompanyId);

                entity.HasOne(d => d.UserData)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.UserDataId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
