using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectPhase> ProjectPhases => Set<ProjectPhase>();
    public DbSet<Iteration> Iterations => Set<Iteration>();
    public DbSet<IterationTask> IterationTasks => Set<IterationTask>();
    public DbSet<Microincrement> Microincrements => Set<Microincrement>();
    public DbSet<Artifact> Artifacts => Set<Artifact>();
    public DbSet<ArtifactVersion> ArtifactVersions => Set<ArtifactVersion>();
    public DbSet<ProjectPlanVersion> ProjectPlanVersions => Set<ProjectPlanVersion>();
    public DbSet<Deliverable> Deliverables => Set<Deliverable>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Defect> Defects => Set<Defect>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ProjectPhase relationship
        modelBuilder.Entity<ProjectPhase>()
            .HasOne(pp => pp.Project)
            .WithMany(p => p.Phases)
            .HasForeignKey(pp => pp.ProjectId);

        // Configure Iteration relationship
        modelBuilder.Entity<Iteration>()
            .HasOne(i => i.Project)
            .WithMany()
            .HasForeignKey(i => i.ProjectId);

        // Configure IterationTask relationship
        modelBuilder.Entity<IterationTask>()
            .HasOne(t => t.Iteration)
            .WithMany(i => i.Tasks)
            .HasForeignKey(t => t.IterationId);

        modelBuilder.Entity<IterationTask>()
            .HasOne(t => t.ProjectPhase)
            .WithMany()
            .HasForeignKey(t => t.ProjectPhaseId)
            .IsRequired(false);

        // Configure Microincrement relationship
        modelBuilder.Entity<Microincrement>()
            .HasOne(m => m.ProjectPhase)
            .WithMany()
            .HasForeignKey(m => m.ProjectPhaseId);

        modelBuilder.Entity<Microincrement>()
            .HasOne(m => m.Deliverable)
            .WithMany(d => d.Microincrements)
            .HasForeignKey(m => m.DeliverableId)
            .IsRequired(false);

        // Configure Artifact relationship
        modelBuilder.Entity<Artifact>()
            .HasOne(a => a.ProjectPhase)
            .WithMany(p => p.Artifacts)
            .HasForeignKey(a => a.ProjectPhaseId);

        // Configure ArtifactVersion relationship
        modelBuilder.Entity<ArtifactVersion>()
            .HasOne(av => av.Artifact)
            .WithMany(a => a.Versions)
            .HasForeignKey(av => av.ArtifactId);

        // Configure Deliverable relationship  
        modelBuilder.Entity<Deliverable>()
            .HasOne(d => d.ProjectPhase)
            .WithMany(p => p.Deliverables)
            .HasForeignKey(d => d.ProjectPhaseId);

        // Configure Defect relationships
        modelBuilder.Entity<Defect>()
            .HasOne(d => d.Project)
            .WithMany()
            .HasForeignKey(d => d.ProjectId);

        modelBuilder.Entity<Defect>()
            .HasOne(d => d.Artifact)
            .WithMany()
            .HasForeignKey(d => d.ArtifactId)
            .IsRequired(false);

        // Configure Workflow relationships
        modelBuilder.Entity<WorkflowStep>()
            .HasOne(ws => ws.Workflow)
            .WithMany(w => w.Steps)
            .HasForeignKey(ws => ws.WorkflowId);

        // Store enums as strings
        modelBuilder.Entity<Artifact>()
            .Property(a => a.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Artifact>()
            .Property(a => a.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Defect>()
            .Property(d => d.Severity)
            .HasConversion<string>();

        modelBuilder.Entity<Defect>()
            .Property(d => d.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ProjectPhase>()
            .Property(p => p.Status)
            .HasConversion<string>();
    }
}
