using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Registrar un nuevo usuario
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        // Validar modelo
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Datos inválidos"
            });
        }

        // Verificar si el email ya existe
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (existingUser != null)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Ya existe un usuario con este email"
            });
        }

        // Crear usuario
        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = HashPassword(dto.Password),
            Role = "Usuario",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generar token
        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = "Usuario registrado exitosamente",
            Token = token,
            User = MapToDto(user)
        });
    }

    /// <summary>
    /// Iniciar sesión
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Datos inválidos"
            });
        }

        // Buscar usuario
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (user == null)
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Email o contraseña incorrectos"
            });
        }

        // Verificar contraseña
        if (!VerifyPassword(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Email o contraseña incorrectos"
            });
        }

        // Verificar si está activo
        if (!user.IsActive)
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Tu cuenta ha sido desactivada"
            });
        }

        // Actualizar último login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generar token
        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = "Inicio de sesión exitoso",
            Token = token,
            User = MapToDto(user)
        });
    }

    /// <summary>
    /// Obtener usuario actual desde el token
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponseDto>> GetCurrentUser()
    {
        // Obtener el token del header
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "No autenticado"
            });
        }

        var token = authHeader.Substring("Bearer ".Length);

        try
        {
            var principal = ValidateToken(token);
            if (principal == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Token inválido"
                });
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Token inválido"
                });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                });
            }

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "OK",
                User = MapToDto(user)
            });
        }
        catch
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Token inválido o expirado"
            });
        }
    }

    /// <summary>
    /// Obtener todos los usuarios (para asignaciones)
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await _context.Users
            .Where(u => u.IsActive)
            .Select(u => MapToDto(u))
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Cambiar contraseña
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new AuthResponseDto { Success = false, Message = "No autenticado" });
        }

        var token = authHeader.Substring("Bearer ".Length);
        var principal = ValidateToken(token);
        if (principal == null)
        {
            return Unauthorized(new AuthResponseDto { Success = false, Message = "Token inválido" });
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new AuthResponseDto { Success = false, Message = "Token inválido" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new AuthResponseDto { Success = false, Message = "Usuario no encontrado" });
        }

        if (!VerifyPassword(dto.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new AuthResponseDto { Success = false, Message = "Contraseña actual incorrecta" });
        }

        user.PasswordHash = HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponseDto { Success = true, Message = "Contraseña cambiada exitosamente" });
    }

    // ==================== HELPERS ====================

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "OpenUpSecretKey2024SuperSecureKeyForJWT!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("userId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "OpenUpApp",
            audience: _configuration["Jwt:Audience"] ?? "OpenUpUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "OpenUpSecretKey2024SuperSecureKeyForJWT!");

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "OpenUpApp",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "OpenUpUsers",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "OpenUpSalt2024"));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}

/// <summary>
/// DTO para cambiar contraseña
/// </summary>
public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
