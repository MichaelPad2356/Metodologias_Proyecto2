using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectPhase> ProjectPhases { get; set; }
    public DbSet<Deliverable> Deliverables { get; set; }
    public DbSet<Microincrement> Microincrements { get; set; }
    public DbSet<Artifact> Artifacts { get; set; }
    public DbSet<ArtifactVersion> ArtifactVersions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasMany(e => e.Phases)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProjectPhase configuration
        modelBuilder.Entity<ProjectPhase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasMany(e => e.Deliverables)
                .WithOne(d => d.ProjectPhase)
                .HasForeignKey(d => d.ProjectPhaseId);
            entity.HasMany(e => e.Artifacts)
                .WithOne(a => a.ProjectPhase)
                .HasForeignKey(a => a.ProjectPhaseId);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Deliverable configuration
        modelBuilder.Entity<Deliverable>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ProjectPhase)
                .WithMany()
                .HasForeignKey(e => e.ProjectPhaseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Microincrement configuration
        modelBuilder.Entity<Microincrement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ProjectPhase)
                .WithMany()
                .HasForeignKey(e => e.ProjectPhaseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Deliverable)
                .WithMany(e => e.Microincrements)
                .HasForeignKey(e => e.DeliverableId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Artifact>()
            .HasMany(a => a.Versions)
            .WithOne(v => v.Artifact)
            .HasForeignKey(v => v.ArtifactId);
    }
}