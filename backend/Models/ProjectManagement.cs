using System.ComponentModel.DataAnnotations;

namespace backend.Models;

/// <summary>
/// HU-025: Usuarios del proyecto con roles específicos
/// </summary>
public class ProjectMember
{
    public int Id { get; set; }

    [Required]
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required]
    [StringLength(200)]
    public string UserEmail { get; set; } = string.Empty;

    [StringLength(200)]
    public string? UserName { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Desarrollador"; // Autor, Revisor, PO, SM, Desarrollador, Tester, Admin

    public MemberStatus Status { get; set; } = MemberStatus.Pending;

    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }

    [StringLength(100)]
    public string? InvitedBy { get; set; }
}

public enum MemberStatus
{
    Pending,
    Accepted,
    Declined,
    Removed
}

/// <summary>
/// HU-020: Historial de movimientos de entregables
/// </summary>
public class DeliverableMovement
{
    public int Id { get; set; }

    public int DeliverableId { get; set; }
    public Deliverable? Deliverable { get; set; }

    public int FromPhaseId { get; set; }
    public int ToPhaseId { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    [StringLength(100)]
    public string? MovedBy { get; set; }

    public DateTime MovedAt { get; set; } = DateTime.UtcNow;

    public bool RequiredConfirmation { get; set; } = false;
    public string? WarningsJson { get; set; } // Advertencias que se mostraron
}

/// <summary>
/// HU-026: Documento de cierre generado automáticamente
/// </summary>
public class ProjectClosure
{
    public int Id { get; set; }

    [Required]
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public DateTime ClosedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? ClosedBy { get; set; }

    public bool IsForcedClose { get; set; } = false;

    [StringLength(1000)]
    public string? ForceCloseJustification { get; set; }

    // Resumen del cierre en JSON
    public string? ValidationResultJson { get; set; }
    public string? ArtifactsSummaryJson { get; set; }
    public string? TeamMembersJson { get; set; }

    // Documento generado
    public string? ClosureDocumentPath { get; set; }
}
