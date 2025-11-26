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
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ProjectPlanVersion> ProjectPlanVersions { get; set; }
    public DbSet<Iteration> Iterations { get; set; }
    public DbSet<IterationTask> IterationTasks { get; set; }

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
    }
}
