using Microsoft.EntityFrameworkCore;
using SpecimenCheckin.Api.Models;

namespace SpecimenCheckin.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Lab> Labs => Set<Lab>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Manifest> Manifests => Set<Manifest>();
    public DbSet<Specimen> Specimens => Set<Specimen>();
    public DbSet<Discrepancy> Discrepancies => Set<Discrepancy>();
    public DbSet<CheckInEvent> CheckInEvents => Set<CheckInEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lab>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasIndex(l => l.Name).IsUnique();
        });

        modelBuilder.Entity<Clinic>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Lab)
                .WithMany(l => l.Clinics)
                .HasForeignKey(c => c.LabId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Manifest>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.Code);
            e.HasOne(m => m.Lab)
                .WithMany(l => l.Manifests)
                .HasForeignKey(m => m.LabId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Clinic)
                .WithMany(c => c.Manifests)
                .HasForeignKey(m => m.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Specimen>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.Code);
            e.HasOne(s => s.Manifest)
                .WithMany(m => m.Specimens)
                .HasForeignKey(s => s.ManifestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Discrepancy>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Manifest)
                .WithMany(m => m.Discrepancies)
                .HasForeignKey(d => d.ManifestId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.Specimen)
                .WithMany(s => s.Discrepancies)
                .HasForeignKey(d => d.SpecimenId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CheckInEvent>(e =>
        {
            e.HasKey(ev => ev.Id);
            e.HasOne(ev => ev.Manifest)
                .WithMany(m => m.CheckInEvents)
                .HasForeignKey(ev => ev.ManifestId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
