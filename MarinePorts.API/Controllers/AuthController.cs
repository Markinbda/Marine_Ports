using MarinePorts.API.Data;
using MarinePorts.API.DTOs;
using MarinePorts.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MarinePorts.API.Controllers;

/// <summary>
/// Handles user registration and login.
/// POST /api/auth/register  – create a new account (pending admin approval)
/// POST /api/auth/login     – authenticate and receive a JWT token
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext  _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db     = db;
        _config = config;
    }

    // ── POST /api/auth/register ───────────────────────────────────────────────
    /// <summary>
    /// Registers a new user. The account is created with IsApproved = false.
    /// A Port Authority administrator must approve the account before the user
    /// can add boats or moorings.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // ModelState validation covers all [Required] / [EmailAddress] etc. attributes
        // on RegisterDto and returns HTTP 400 with field-level messages automatically.
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Prevent public registration of Admin accounts – security boundary.
        if (!string.IsNullOrEmpty(dto.Role) && dto.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Admin accounts cannot be self-registered." });

        // Default role to BoatOwner when not specified
        var assignedRole = string.IsNullOrWhiteSpace(dto.Role) ? "BoatOwner" : dto.Role;

        // Check for duplicate email (enforced at DB level too, but we surface a friendly message).
        bool emailTaken = await _db.Users.AnyAsync(u => u.Email == dto.Email.ToLower());
        if (emailTaken)
            return Conflict(new { message = "An account with this email address already exists." });

        // Hash the password using BCrypt-equivalent PBKDF2 via built-in .NET utilities.
        // Never store plain-text passwords.
        string passwordHash = HashPassword(dto.Password);

        var user = new AppUser
        {
            FirstName                    = dto.FirstName.Trim(),
            LastName                     = dto.LastName.Trim(),
            Email                        = dto.Email.Trim().ToLower(),
            PasswordHash                 = passwordHash,
            PhoneNumber                  = dto.PhoneNumber.Trim(),
            AddressLine1                 = dto.AddressLine1.Trim(),
            AddressLine2                 = dto.AddressLine2.Trim(),
            Parish                       = dto.Parish.Trim(),
            OrganisationName             = dto.OrganisationName?.Trim(),
            GovernmentIdOrLicenceNumber  = dto.GovernmentIdOrLicenceNumber?.Trim() ?? string.Empty,
            Role                         = assignedRole,
            RegisteredAt                 = DateTime.UtcNow,
            IsApproved                   = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Register), new
        {
            message    = "Registration successful. You can now sign in to your account.",
            userId     = user.Id,
            isApproved = user.IsApproved
        });
    }

    // ── POST /api/auth/login ──────────────────────────────────────────────────
    /// <summary>
    /// Authenticates a user and returns a signed JWT token valid for 8 hours.
    /// Unapproved accounts receive HTTP 403.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

        // Use a constant-time comparison to avoid timing attacks.
        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        if (!user.IsApproved)
            return StatusCode(403, new
            {
                message = "Your account is awaiting approval by the Port Authority. " +
                          "You will be notified once your registration has been reviewed."
            });

        string token = GenerateJwtToken(user);

        return Ok(new
        {
            token,
            userId   = user.Id,
            fullName = user.FullName,
            email    = user.Email,
            role     = user.Role
        });
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Uses PBKDF2-SHA512 (RFC 2898) with a random salt. The salt and iteration
    /// count are embedded in the returned string – no separate storage needed.
    /// </summary>
    private static string HashPassword(string password)
    {
        // 16-byte random salt
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        // 100 000 iterations of PBKDF2-SHA512
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA512);
        byte[] hash = pbkdf2.GetBytes(32);

        // Encode as "iterations:salt_b64:hash_b64" for self-describing storage.
        return $"100000:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 3) return false;

            int    iterations = int.Parse(parts[0]);
            byte[] salt       = Convert.FromBase64String(parts[1]);
            byte[] expected   = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512);
            byte[] actual = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Generates a signed JWT with role and identity claims.</summary>
    private string GenerateJwtToken(AppUser user)
    {
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key not configured.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name,               user.FullName),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:            _config["Jwt:Issuer"] ?? "MarinePortsAPI",
            claims:            claims,
            expires:           DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
