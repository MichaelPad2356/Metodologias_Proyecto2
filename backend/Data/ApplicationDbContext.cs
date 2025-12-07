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
    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }

    // HU-018: Configuración del sistema
    public DbSet<SystemRole> SystemRoles { get; set; }
    public DbSet<CustomArtifactType> CustomArtifactTypes { get; set; }
    public DbSet<CustomPhaseDefinition> CustomPhaseDefinitions { get; set; }
    public DbSet<ConfigurationHistory> ConfigurationHistory { get; set; }

    // HU-019: Plantillas OpenUP
    public DbSet<OpenUpTemplate> OpenUpTemplates { get; set; }

    // HU-020, HU-025, HU-026: Gestión de proyectos
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<DeliverableMovement> DeliverableMovements { get; set; }
    public DbSet<ProjectClosure> ProjectClosures { get; set; }

    // Autenticación
    public DbSet<User> Users { get; set; }

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
        });


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

        // Configure Workflow relationships
        modelBuilder.Entity<Artifact>()
            .HasOne(a => a.Workflow)
            .WithMany()
            .HasForeignKey(a => a.WorkflowId)
            .IsRequired(false);

        modelBuilder.Entity<Artifact>()
            .HasOne(a => a.CurrentStep)
            .WithMany()
            .HasForeignKey(a => a.CurrentStepId)
            .IsRequired(false);

        // Configure Iteracion
        modelBuilder.Entity<Iteracion>()
            .ToTable("Iteraciones");

        // HU-018: SystemRole configuration
        modelBuilder.Entity<SystemRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // HU-019: OpenUpTemplate configuration
        modelBuilder.Entity<OpenUpTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ParentTemplate)
                .WithMany()
                .HasForeignKey(e => e.ParentTemplateId)
                .IsRequired(false);
        });

        // HU-025: ProjectMember configuration
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId);
            entity.Property(e => e.Status).HasConversion<string>();
        });

        // HU-020: DeliverableMovement configuration
        modelBuilder.Entity<DeliverableMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Deliverable)
                .WithMany()
                .HasForeignKey(e => e.DeliverableId);
        });

        // HU-026: ProjectClosure configuration
        modelBuilder.Entity<ProjectClosure>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId);
        });
    }
}
