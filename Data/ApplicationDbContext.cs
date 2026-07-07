using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalonBook.Models;

namespace SalonBook.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Detinator> Detinatori { get; set; }
        public DbSet<Salon> Saloane { get; set; }
        public DbSet<Serviciu> Servicii { get; set; }
        public DbSet<Programare> Programari { get; set; }
        public DbSet<Notificare> Notificari { get; set; }
        public DbSet<ClientBlocat> ClientiBlocati { get; set; }
        public DbSet<Recenzie> Recenzii { get; set; }
        public DbSet<PerioadaBlocata> PerioadeBlockate { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Detinator>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Salon>()
                .HasOne(s => s.Detinator)
                .WithMany(d => d.Saloane)
                .HasForeignKey(s => s.DetinatorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Serviciu>()
                .HasOne(sv => sv.Salon)
                .WithMany(s => s.Servicii)
                .HasForeignKey(sv => sv.SalonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Programare>()
                .HasOne(p => p.Client)
                .WithMany(u => u.Programari)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programare>()
                .HasOne(p => p.Serviciu)
                .WithMany(s => s.Programari)
                .HasForeignKey(p => p.ServiciuId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notificare>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Recenzie>()
                .HasOne(r => r.Salon)
                .WithMany(s => s.Recenzii)
                .HasForeignKey(r => r.SalonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Recenzie>()
                .HasOne(r => r.Client)
                .WithMany()
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PerioadaBlocata>()
                .HasOne(p => p.Salon)
                .WithMany()
                .HasForeignKey(p => p.SalonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}