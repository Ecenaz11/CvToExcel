using Microsoft.EntityFrameworkCore;
using CvToExcel.Domain.Entities;

namespace CvToExcel.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<CvDocument> CvDocuments { get; set; }
    public DbSet<Education> Educations { get; set; }
    public DbSet<WorkExperience> WorkExperiences { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Language> Languages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CvDocument>()
        .Property(c => c.FullName)
        .IsRequired()
        .HasMaxLength(200);

        modelBuilder.Entity<CvDocument>()
        .Property(c => c.Email)
        .HasMaxLength(150);

        modelBuilder.Entity<CvDocument>()
        .Property(c => c.Phone)
        .HasMaxLength(30);

        modelBuilder.Entity<CvDocument>()
        .Property(c => c.Location)
        .HasMaxLength(150);

        modelBuilder.Entity<CvDocument>()
        .Property(c => c.OriginalFileName)
        .IsRequired()
        .HasMaxLength(255);

        modelBuilder.Entity<CvDocument>()
        .Property(c => c.StoredFileName)
        .IsRequired()
        .HasMaxLength(255);
       
        modelBuilder.Entity<CvDocument>()
        .HasIndex(c => c.StoredFileName)
        .IsUnique();

        modelBuilder.Entity<CvDocument>()
        .Property(c => c.FilePath)
        .IsRequired()
        .HasMaxLength(500);

        modelBuilder.Entity<CvDocument>()
        .Property(c => c.ContentType)
        .IsRequired()
        .HasMaxLength(100);

        modelBuilder.Entity<Education>()
        .HasOne(e => e.CvDocument)
        .WithMany(c => c.Educations)
        .HasForeignKey(e => e.CvDocumentId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Education>()
        .Property(e => e.Institution)
        .IsRequired()
        .HasMaxLength(200);

        modelBuilder.Entity<Education>()
        .Property(e => e.Department)
        .HasMaxLength(150);

        modelBuilder.Entity<Education>()
        .Property(e => e.Degree)
        .HasMaxLength(100);

        modelBuilder.Entity<WorkExperience>()
        .HasOne(w => w.CvDocument)
        .WithMany(c => c.WorkExperiences)
        .HasForeignKey(w => w.CvDocumentId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkExperience>()
        .Property(w => w.CompanyName)
        .IsRequired()
        .HasMaxLength(200);

        modelBuilder.Entity<WorkExperience>()
        .Property(w => w.JobTitle)
        .IsRequired()
        .HasMaxLength(150);

        modelBuilder.Entity<Skill>()
        .HasOne(s => s.CvDocument)
        .WithMany(c => c.Skills)
        .HasForeignKey(s => s.CvDocumentId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Skill>()
        .Property(s => s.Name)
        .IsRequired()
        .HasMaxLength(100);

        modelBuilder.Entity<Language>()
        .HasOne(l => l.CvDocument)
        .WithMany(c => c.Languages)
        .HasForeignKey(l => l.CvDocumentId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Language>()
        .Property(l => l.Name)
        .IsRequired()
        .HasMaxLength(100);

        modelBuilder.Entity<Language>()
        .Property(l => l.ProficiencyLevel)
        .IsRequired()
        .HasMaxLength(50);
    }
}