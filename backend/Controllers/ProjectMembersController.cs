using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace backend.Controllers;

/// <summary>
/// HU-025: Controlador para gestión de miembros del proyecto
/// </summary>
[ApiController]
[Route("api/projects/{projectId}/members")]
[Authorize]
public class ProjectMembersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProjectMembersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los miembros de un proyecto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetProjectMembers(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound("Proyecto no encontrado");

        var members = await _context.ProjectMembers
            .Where(m => m.ProjectId == projectId && m.Status != MemberStatus.Removed)
            .Select(m => new ProjectMemberDto
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                UserEmail = m.UserEmail,
                UserName = m.UserName,
                Role = m.Role,
                Status = m.Status.ToString(),
                InvitedAt = m.InvitedAt,
                AcceptedAt = m.AcceptedAt,
                InvitedBy = m.InvitedBy
            })
            .OrderBy(m => m.Role)
            .ThenBy(m => m.UserName)
            .ToListAsync();

        return Ok(members);
    }

    /// <summary>
    /// Obtiene un miembro específico
    /// </summary>
    [HttpGet("{memberId}")]
    public async Task<ActionResult<ProjectMemberDto>> GetMember(int projectId, int memberId)
    {
        var member = await _context.ProjectMembers.FindAsync(memberId);
        if (member == null || member.ProjectId != projectId)
            return NotFound();

        return new ProjectMemberDto
        {
            Id = member.Id,
            ProjectId = member.ProjectId,
            UserEmail = member.UserEmail,
            UserName = member.UserName,
            Role = member.Role,
            Status = member.Status.ToString(),
            InvitedAt = member.InvitedAt,
            AcceptedAt = member.AcceptedAt,
            InvitedBy = member.InvitedBy
        };
    }

    /// <summary>
    /// Invita a un usuario al proyecto
    /// </summary>
    [HttpPost("invite")]
    public async Task<ActionResult<ProjectMemberDto>> InviteMember(int projectId, InviteMemberDto dto)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound("Proyecto no encontrado");

        // Verificar si ya existe
        var existingMember = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserEmail == dto.UserEmail && m.Status != MemberStatus.Removed);

        if (existingMember != null)
            return BadRequest("Este usuario ya es miembro del proyecto o tiene una invitación pendiente");

        // Validar rol
        var validRoles = new[] { "Autor", "Revisor", "Product Owner", "Scrum Master", "Desarrollador", "Tester", "Administrador" };
        if (!validRoles.Contains(dto.Role))
            return BadRequest($"Rol inválido. Los roles válidos son: {string.Join(", ", validRoles)}");

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserEmail = dto.UserEmail,
            UserName = dto.UserName,
            Role = dto.Role,
            Status = MemberStatus.Pending,
            InvitedBy = User.Identity?.Name ?? "Sistema"
        };

        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync();

        // En un sistema real, aquí se enviaría una notificación por email
        // await _notificationService.SendInvitationEmail(member);

        return CreatedAtAction(nameof(GetMember), new { projectId, memberId = member.Id }, new ProjectMemberDto
        {
            Id = member.Id,
            ProjectId = member.ProjectId,
            UserEmail = member.UserEmail,
            UserName = member.UserName,
            Role = member.Role,
            Status = member.Status.ToString(),
            InvitedAt = member.InvitedAt,
            InvitedBy = member.InvitedBy
        });
    }

    /// <summary>
    /// Acepta una invitación al proyecto
    /// </summary>
    [HttpPost("{memberId}/accept")]
    public async Task<IActionResult> AcceptInvitation(int projectId, int memberId)
    {
        var member = await _context.ProjectMembers.FindAsync(memberId);
        if (member == null || member.ProjectId != projectId)
            return NotFound();

        if (member.Status != MemberStatus.Pending)
            return BadRequest("Esta invitación ya fue procesada");

        member.Status = MemberStatus.Accepted;
        member.AcceptedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Invitación aceptada exitosamente" });
    }

    /// <summary>
    /// Rechaza una invitación al proyecto
    /// </summary>
    [HttpPost("{memberId}/decline")]
    public async Task<IActionResult> DeclineInvitation(int projectId, int memberId)
    {
        var member = await _context.ProjectMembers.FindAsync(memberId);
        if (member == null || member.ProjectId != projectId)
            return NotFound();

        if (member.Status != MemberStatus.Pending)
            return BadRequest("Esta invitación ya fue procesada");

        member.Status = MemberStatus.Declined;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Invitación rechazada" });
    }

    /// <summary>
    /// Actualiza el rol de un miembro
    /// </summary>
    [HttpPut("{memberId}/role")]
    public async Task<IActionResult> UpdateMemberRole(int projectId, int memberId, [FromBody] UpdateRoleDto dto)
    {
        var member = await _context.ProjectMembers.FindAsync(memberId);
        if (member == null || member.ProjectId != projectId)
            return NotFound();

        var validRoles = new[] { "Autor", "Revisor", "Product Owner", "Scrum Master", "Desarrollador", "Tester", "Administrador" };
        if (!validRoles.Contains(dto.Role))
            return BadRequest($"Rol inválido. Los roles válidos son: {string.Join(", ", validRoles)}");

        member.Role = dto.Role;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Rol actualizado exitosamente", role = member.Role });
    }

    /// <summary>
    /// Elimina un miembro del proyecto
    /// </summary>
    [HttpDelete("{memberId}")]
    public async Task<IActionResult> RemoveMember(int projectId, int memberId)
    {
        var member = await _context.ProjectMembers.FindAsync(memberId);
        if (member == null || member.ProjectId != projectId)
            return NotFound();

        member.Status = MemberStatus.Removed;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Miembro removido del proyecto" });
    }

    /// <summary>
    /// Obtiene la matriz de permisos por rol
    /// </summary>
    [HttpGet("/api/permissions/matrix")]
    public ActionResult<Dictionary<string, List<string>>> GetPermissionMatrix()
    {
        var matrix = new Dictionary<string, List<string>>
        {
            ["Administrador"] = new List<string> 
            { 
                "create_artifact", "edit_artifact", "delete_artifact", "approve_artifact",
                "create_project", "edit_project", "close_project", "delete_project",
                "manage_members", "manage_roles", "view_audit", "force_close"
            },
            ["Product Owner"] = new List<string> 
            { 
                "create_artifact", "edit_artifact", "approve_artifact",
                "create_project", "edit_project", "close_project",
                "manage_members", "view_audit"
            },
            ["Scrum Master"] = new List<string> 
            { 
                "create_artifact", "edit_artifact", 
                "edit_project", "manage_members", "view_audit"
            },
            ["Desarrollador"] = new List<string> 
            { 
                "create_artifact", "edit_artifact"
            },
            ["Tester"] = new List<string> 
            { 
                "create_artifact", "edit_artifact", "review_artifact"
            },
            ["Revisor"] = new List<string> 
            { 
                "review_artifact", "approve_artifact"
            },
            ["Autor"] = new List<string> 
            { 
                "create_artifact", "edit_artifact"
            }
        };

        return Ok(matrix);
    }
}

#region DTOs

public class ProjectMemberDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime InvitedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? InvitedBy { get; set; }
}

public class InviteMemberDto
{
    public string UserEmail { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Role { get; set; } = "Desarrollador";
}

public class UpdateRoleDto
{
    public string Role { get; set; } = string.Empty;
}

#endregion
