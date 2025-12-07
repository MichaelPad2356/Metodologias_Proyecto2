using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace backend.Controllers;

/// <summary>
/// Controlador para gestionar las invitaciones del usuario actual
/// </summary>
[ApiController]
[Route("api/invitations")]
[Authorize]
public class InvitationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InvitationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las invitaciones pendientes del usuario actual
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<UserInvitationDto>>> GetPendingInvitations()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized();

        var invitations = await _context.ProjectMembers
            .Where(m => m.UserEmail.ToLower() == userEmail.ToLower() && m.Status == MemberStatus.Pending)
            .Join(_context.Projects,
                member => member.ProjectId,
                project => project.Id,
                (member, project) => new UserInvitationDto
                {
                    Id = member.Id,
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    ProjectCode = project.Code,
                    Role = member.Role,
                    InvitedBy = member.InvitedBy,
                    InvitedAt = member.InvitedAt
                })
            .OrderByDescending(i => i.InvitedAt)
            .ToListAsync();

        return Ok(invitations);
    }

    /// <summary>
    /// Obtiene el conteo de invitaciones pendientes
    /// </summary>
    [HttpGet("pending/count")]
    public async Task<ActionResult<int>> GetPendingInvitationsCount()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized();

        var count = await _context.ProjectMembers
            .CountAsync(m => m.UserEmail.ToLower() == userEmail.ToLower() && m.Status == MemberStatus.Pending);

        return Ok(count);
    }

    /// <summary>
    /// Acepta una invitación
    /// </summary>
    [HttpPost("{invitationId}/accept")]
    public async Task<IActionResult> AcceptInvitation(int invitationId)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized();

        var invitation = await _context.ProjectMembers.FindAsync(invitationId);
        
        if (invitation == null)
            return NotFound("Invitación no encontrada");

        // Verificar que la invitación sea del usuario actual
        if (invitation.UserEmail.ToLower() != userEmail.ToLower())
            return Forbid();

        if (invitation.Status != MemberStatus.Pending)
            return BadRequest("Esta invitación ya fue procesada");

        invitation.Status = MemberStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "¡Invitación aceptada! Ahora eres parte del proyecto." });
    }

    /// <summary>
    /// Rechaza una invitación
    /// </summary>
    [HttpPost("{invitationId}/decline")]
    public async Task<IActionResult> DeclineInvitation(int invitationId)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized();

        var invitation = await _context.ProjectMembers.FindAsync(invitationId);
        
        if (invitation == null)
            return NotFound("Invitación no encontrada");

        // Verificar que la invitación sea del usuario actual
        if (invitation.UserEmail.ToLower() != userEmail.ToLower())
            return Forbid();

        if (invitation.Status != MemberStatus.Pending)
            return BadRequest("Esta invitación ya fue procesada");

        invitation.Status = MemberStatus.Declined;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Invitación rechazada" });
    }

    /// <summary>
    /// Obtiene todas las invitaciones del usuario (historial)
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<UserInvitationDto>>> GetAllInvitations()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized();

        var invitations = await _context.ProjectMembers
            .Where(m => m.UserEmail.ToLower() == userEmail.ToLower())
            .Join(_context.Projects,
                member => member.ProjectId,
                project => project.Id,
                (member, project) => new UserInvitationDto
                {
                    Id = member.Id,
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    ProjectCode = project.Code,
                    Role = member.Role,
                    Status = member.Status.ToString(),
                    InvitedBy = member.InvitedBy,
                    InvitedAt = member.InvitedAt,
                    AcceptedAt = member.AcceptedAt
                })
            .OrderByDescending(i => i.InvitedAt)
            .ToListAsync();

        return Ok(invitations);
    }
}

/// <summary>
/// DTO para invitaciones del usuario
/// </summary>
public class UserInvitationDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? InvitedBy { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
}
