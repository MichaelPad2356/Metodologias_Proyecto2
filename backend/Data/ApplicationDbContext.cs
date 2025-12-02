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
    public DbSet<ProjectPlanVersion> ProjectPlanVersions { get; set; }
    public DbSet<Iteration> Iterations { get; set; }
    public DbSet<IterationTask> IterationTasks { get; set; }
    public DbSet<Iteracion> Iteraciones { get; set; }
    public DbSet<Defect> Defects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Defect configuration
        modelBuilder.Entity<Defect>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Severity).HasDefaultValue("Low");
            entity.Property(e => e.Status).HasDefaultValue("New");
        });

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
                .WithMany(p => p.Deliverables)
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

        // Artifact configuration
        modelBuilder.Entity<Artifact>()
            .HasMany(a => a.Versions)
            .WithOne(v => v.Artifact)
            .HasForeignKey(v => v.ArtifactId);

        // ProjectPlanVersion configuration
        modelBuilder.Entity<ProjectPlanVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.ProjectId, e.Version }).IsUnique();
        });

        // Iteration configuration
        modelBuilder.Entity<Iteration>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Iteration)
                .HasForeignKey(e => e.IterationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // IterationTask configuration
        modelBuilder.Entity<IterationTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.ProjectPhase)
                .WithMany()
                .HasForeignKey(e => e.ProjectPhaseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Iteracion configuration
        modelBuilder.Entity<Iteracion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TareasJson).HasColumnType("TEXT");
        });
    }
}