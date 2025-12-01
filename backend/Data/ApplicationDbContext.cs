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
    public DbSet<Artifact> Artifacts { get; set; }
    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<ArtifactHistory> ArtifactHistories { get; set; }
    public DbSet<Iteracion> Iteraciones { get; set; }
    public DbSet<ProjectPlanVersion> PlanVersions { get; set; }
    public DbSet<Microincrement> Microincrements { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Iteration> Iterations { get; set; }
    public DbSet<IterationTask> IterationTasks { get; set; }
<<<<<<< HEAD
    public DbSet<Iteracion> Iteraciones { get; set; }
    public DbSet<Defect> Defects { get; set; }
=======
    public DbSet<ProjectPlanVersion> ProjectPlanVersions { get; set; }
>>>>>>> origin/feature/-entregable

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Status)
                .HasConversion<string>();
        });

        modelBuilder.Entity<Artifact>(entity =>
        {
            entity.Property(a => a.Type)
                .HasConversion<string>();
            
            entity.Property(a => a.Status)
                .HasConversion<string>();

            entity.HasOne(a => a.Workflow)
                .WithMany()
                .HasForeignKey(a => a.WorkflowId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.CurrentStep)
                .WithMany()
                .HasForeignKey(a => a.CurrentStepId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.HasOne(ws => ws.Workflow)
                .WithMany(w => w.Steps)
                .HasForeignKey(ws => ws.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ArtifactHistory>(entity =>
        {
            entity.HasOne(ah => ah.Artifact)
                .WithMany()
                .HasForeignKey(ah => ah.ArtifactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Iteracion>(entity =>
        {
            entity.Property(i => i.TareasJson)
                .HasColumnType("TEXT");
        });

        modelBuilder.Entity<ProjectPlanVersion>(entity =>
        {
            entity.HasOne(pv => pv.Project)
                .WithMany()
                .HasForeignKey(pv => pv.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // COMENTAR ESTAS LÍNEAS DE MICROINCREMENT POR AHORA:
        // modelBuilder.Entity<Microincrement>(entity =>
        // {
        //     entity.HasOne(m => m.Iteration)
        //         .WithMany()
        //         .HasForeignKey(m => m.IterationId)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     entity.Property(m => m.Status)
        //         .HasConversion<string>();
        // });
    }
}