using MarinePorts.API.Data;
using MarinePorts.API.DTOs;
using MarinePorts.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarinePorts.API.Controllers;

/// <summary>
/// Administration endpoints for the Marine &amp; Ports system.
/// All routes require Role = Admin EXCEPT /bootstrap (first-run setup).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    // ── POST /api/admin/bootstrap ─────────────────────────────────────────────
    /// <summary>
    /// One-time endpoint: promotes an existing user to Admin and approves them.
    /// Only works while ZERO Admin accounts exist in the database.
    /// Disabled automatically once the first admin is created.
    /// </summary>
    [HttpPost("bootstrap")]
    [AllowAnonymous]
    public async Task<IActionResult> Bootstrap([FromBody] BootstrapDto dto)
    {
        // Security: refuse if any Admin already exists (prevents privilege escalation).
        bool adminExists = await _db.Users.AnyAsync(u => u.Role == "Admin");
        if (adminExists)
            return Conflict(new { message = "Bootstrap is disabled — an Admin account already exists." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user is null)
            return NotFound(new { message = $"No registered user found with email '{dto.Email}'." });

        user.Role       = "Admin";
        user.IsApproved = true;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message  = $"{user.FullName} has been promoted to Admin and approved.",
            userId   = user.Id,
            email    = user.Email,
            role     = user.Role,
            approved = user.IsApproved
        });
    }

    // ── GET /api/admin/users ──────────────────────────────────────────────────
    /// <summary>Returns all registered users with summary info.</summary>
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Select(u => new
            {
                u.Id,
                FullName = u.FirstName + " " + u.LastName,
                u.Email,
                u.PhoneNumber,
                u.Parish,
                u.OrganisationName,
                u.GovernmentIdOrLicenceNumber,
                u.Role,
                u.IsApproved,
                u.RegisteredAt,
                BoatCount    = u.Boats.Count,
                MooringCount = u.Moorings.Count
            })
            .OrderByDescending(u => u.RegisteredAt)
            .ToListAsync();

        return Ok(users);
    }

    // ── GET /api/admin/stats ──────────────────────────────────────────────────
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStats()
    {
        var totalMoorings = await _db.Moorings.CountAsync();
        var totalBoats    = await _db.Boats.CountAsync();
        var totalUsers    = await _db.Users.CountAsync();
        var pendingUsers  = await _db.Users.CountAsync(u => !u.IsApproved);
        return Ok(new { totalMoorings, totalBoats, totalUsers, pendingUsers });
    }

    // ── PUT /api/admin/users/{id}/approve ─────────────────────────────────────
    /// <summary>Approves a pending user account.</summary>
    [HttpPut("users/{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.IsApproved = true;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"{user.FullName} approved.", userId = user.Id });
    }

    // ── PUT /api/admin/users/{id}/reject ──────────────────────────────────────
    /// <summary>Revokes / un-approves a user account.</summary>
    [HttpPut("users/{id:int}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.IsApproved = false;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"{user.FullName} access revoked.", userId = user.Id });
    }

    // ── PUT /api/admin/users/{id}/role ────────────────────────────────────────
    /// <summary>Changes a user's role (BoatOwner | MooringOwner | PortAuthority | Admin).</summary>
    [HttpPut("users/{id:int}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetRole(int id, [FromBody] SetRoleDto dto)
    {
        var allowed = new[] { "BoatOwner", "MooringOwner", "PortAuthority", "Admin" };
        if (!allowed.Contains(dto.Role))
            return BadRequest(new { message = "Invalid role. Must be one of: " + string.Join(", ", allowed) });

        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.Role = dto.Role;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"{user.FullName} is now {user.Role}.", userId = user.Id });
    }

    // ── DELETE /api/admin/users/{id} ──────────────────────────────────────────
    /// <summary>Permanently deletes a user and all their boats/moorings.</summary>
    [HttpDelete("users/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ── PUT /api/admin/users/{id} ─────────────────────────────────────────────
    [HttpPut("users/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        var allowed = new[] { "BoatOwner", "MooringOwner", "PortAuthority", "Admin" };
        if (!allowed.Contains(dto.Role))
            return BadRequest(new { message = "Invalid role." });

        user.FirstName        = dto.FirstName.Trim();
        user.LastName         = dto.LastName.Trim();
        user.Email            = dto.Email.Trim().ToLower();
        user.PhoneNumber      = dto.PhoneNumber?.Trim() ?? string.Empty;
        user.Parish           = dto.Parish?.Trim() ?? string.Empty;
        user.OrganisationName = dto.OrganisationName?.Trim();
        user.Role             = dto.Role;
        user.IsApproved       = dto.IsApproved;

        await _db.SaveChangesAsync();
        return Ok(new { message = $"{user.FullName} updated." });
    }

    // ══════════════════════════════════════════════════════════════════════════
    // BOATS – admin management
    // ══════════════════════════════════════════════════════════════════════════

    // GET /api/admin/boats
    [HttpGet("boats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllBoats()
    {
        var boats = await _db.Boats
            .Include(b => b.AppUser)
            .OrderByDescending(b => b.RegisteredAt)
            .Select(b => new
            {
                b.Id, b.RegistrationNumber, b.BoatName, b.BoatType,
                b.OwnerName, b.LengthFeet, b.Latitude, b.Longitude,
                b.PhotoUrl, b.RegisteredAt, b.ColorCode,
                b.AppUserId,
                UserEmail = b.AppUser != null ? b.AppUser.Email : null
            })
            .ToListAsync();
        return Ok(boats);
    }

    // POST /api/admin/boats  – add a boat; optionally assign to a specific user
    [HttpPost("boats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddBoat([FromBody] AdminBoatCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int targetUserId = dto.UserId ?? 0;
        if (targetUserId > 0)
        {
            bool userExists = await _db.Users.AnyAsync(u => u.Id == targetUserId);
            if (!userExists)
                return BadRequest(new { message = $"No user found with ID {targetUserId}." });
        }
        else
        {
            // Admin adds under their own account if no userId specified
            targetUserId = (await _db.Users.FirstAsync(u => u.Role == "Admin")).Id;
        }

        var boat = new Boat
        {
            RegistrationNumber = dto.RegistrationNumber.Trim(),
            OwnerName          = dto.OwnerName.Trim(),
            BoatName           = dto.BoatName.Trim(),
            BoatType           = dto.BoatType.Trim(),
            LengthFeet         = dto.LengthFeet,
            Latitude           = dto.Latitude,
            Longitude          = dto.Longitude,
            PhotoUrl           = dto.PhotoUrl,
            AppUserId          = targetUserId,
            RegisteredAt       = DateTime.UtcNow
        };

        _db.Boats.Add(boat);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Boat added.", boatId = boat.Id });
    }

    // DELETE /api/admin/boats/{id}
    [HttpDelete("boats/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBoat(int id)
    {
        var boat = await _db.Boats.FindAsync(id);
        if (boat is null) return NotFound();
        _db.Boats.Remove(boat);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── PUT /api/admin/boats/{id} ─────────────────────────────────────────────
    [HttpPut("boats/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBoat(int id, [FromBody] AdminUpdateBoatDto dto)
    {
        var boat = await _db.Boats.FindAsync(id);
        if (boat is null) return NotFound();

        boat.RegistrationNumber = dto.RegistrationNumber.Trim();
        boat.BoatName           = dto.BoatName.Trim();
        boat.BoatType           = dto.BoatType.Trim();
        boat.OwnerName          = dto.OwnerName.Trim();
        boat.LengthFeet         = dto.LengthFeet;
        boat.Latitude           = dto.Latitude;
        boat.Longitude          = dto.Longitude;

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Boat {boat.RegistrationNumber} updated." });
    }

    // ══════════════════════════════════════════════════════════════════════════
    // MOORINGS – admin management
    // ══════════════════════════════════════════════════════════════════════════

    // GET /api/admin/moorings
    [HttpGet("moorings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllMoorings()
    {
        var moorings = await _db.Moorings
            .Include(m => m.AppUser)
            .OrderByDescending(m => m.RegisteredAt)
            .Select(m => new
            {
                m.Id, m.MooringNumber, m.OwnerName,
                m.Latitude, m.Longitude, m.BoatSize, m.PhotoUrl, m.RegisteredAt,
                m.AppUserId,
                UserEmail = m.AppUser != null ? m.AppUser.Email : null
            })
            .ToListAsync();
        return Ok(moorings);
    }

    // POST /api/admin/moorings
    [HttpPost("moorings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddMooring([FromBody] AdminMooringCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int targetUserId = dto.UserId ?? 0;
        if (targetUserId > 0)
        {
            bool userExists = await _db.Users.AnyAsync(u => u.Id == targetUserId);
            if (!userExists)
                return BadRequest(new { message = $"No user found with ID {targetUserId}." });
        }
        else
        {
            targetUserId = (await _db.Users.FirstAsync(u => u.Role == "Admin")).Id;
        }

        var mooring = new Mooring
        {
            MooringNumber = dto.MooringNumber.Trim(),
            OwnerName     = dto.OwnerName.Trim(),
            Latitude      = dto.Latitude,
            Longitude     = dto.Longitude,
            BoatSize      = dto.BoatSize?.Trim(),
            PhotoUrl      = dto.PhotoUrl,
            AppUserId     = targetUserId,
            RegisteredAt  = DateTime.UtcNow
        };

        _db.Moorings.Add(mooring);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Mooring added.", mooringId = mooring.Id });
    }

    // DELETE /api/admin/moorings/{id}
    [HttpDelete("moorings/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMooring(int id)
    {
        var mooring = await _db.Moorings.FindAsync(id);
        if (mooring is null) return NotFound();
        _db.Moorings.Remove(mooring);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── PUT /api/admin/moorings/{id} ──────────────────────────────────────────
    [HttpPut("moorings/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMooring(int id, [FromBody] AdminUpdateMooringDto dto)
    {
        var mooring = await _db.Moorings.FindAsync(id);
        if (mooring is null) return NotFound();

        mooring.MooringNumber = dto.MooringNumber.Trim();
        mooring.OwnerName     = dto.OwnerName.Trim();
        mooring.Latitude      = dto.Latitude;
        mooring.Longitude     = dto.Longitude;
        mooring.BoatSize      = dto.BoatSize?.Trim();

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Mooring {mooring.MooringNumber} updated." });
    }

    // ── GET /api/admin/users/{id} ─────────────────────────────────────────────
    /// <summary>Returns full profile + associated boats and moorings for one user.</summary>
    [HttpGet("users/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _db.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                FullName = u.FirstName + " " + u.LastName,
                u.Email,
                u.PhoneNumber,
                u.Parish,
                u.OrganisationName,
                u.GovernmentIdOrLicenceNumber,
                u.Role,
                u.IsApproved,
                u.RegisteredAt,
                Boats = u.Boats.Select(b => new
                {
                    b.Id,
                    b.RegistrationNumber,
                    b.BoatName,
                    b.BoatType,
                    b.LengthFeet,
                    b.Latitude,
                    b.Longitude,
                    b.RegisteredAt
                }).ToList(),
                Moorings = u.Moorings.Select(m => new
                {
                    m.Id,
                    m.MooringNumber,
                    m.BoatSize,
                    m.Latitude,
                    m.Longitude,
                    m.RegisteredAt
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (user is null) return NotFound();
        return Ok(user);
    }

    // GET /api/admin/users/list  – simple id+name list for dropdowns
    [HttpGet("users/list")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserList() =>
        Ok(await _db.Users
            .OrderBy(u => u.FirstName)
            .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName, u.Email })
            .ToListAsync());

    // ── POST /api/admin/users/{id}/reset-password ─────────────────────────────
    /// <summary>Admin-only: force-resets a user's password without knowing the old one.</summary>
    [HttpPost("users/{id:int}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters." });

        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.PasswordHash = HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Password reset for {user.FullName}." });
    }

    // ── POST /api/admin/bootstrap-reset ───────────────────────────────────────
    /// <summary>
    /// Emergency reset: resets the password of any account by email.
    /// Works ONLY when called from localhost (127.0.0.1 / ::1).
    /// This prevents remote abuse while still allowing local recovery.
    /// </summary>
    [HttpPost("bootstrap-reset")]
    [AllowAnonymous]
    public async Task<IActionResult> BootstrapReset([FromBody] BootstrapResetDto dto)
    {
        // Only allow from localhost
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        bool isLocal = remoteIp != null &&
                       (remoteIp.ToString() == "127.0.0.1" ||
                        remoteIp.ToString() == "::1" ||
                        System.Net.IPAddress.IsLoopback(remoteIp));
        if (!isLocal)
            return StatusCode(403, new { message = "This endpoint is only accessible from localhost." });

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user is null)
            return NotFound(new { message = $"No account found for '{dto.Email}'." });

        user.PasswordHash = HashPassword(dto.NewPassword);
        user.IsApproved   = true;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Password reset and account approved for {user.FullName}." });
    }

    // ── Password hashing helper (mirrors AuthController) ─────────────────────
    private static string HashPassword(string password)
    {
        byte[] salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
            password, salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA512);
        byte[] hash = pbkdf2.GetBytes(32);
        return $"100000:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────
public record BootstrapDto(string Email);
public record SetRoleDto(string Role);
public record ResetPasswordDto(string NewPassword);
public record BootstrapResetDto(string Email, string NewPassword);
public record AdminUpdateUserDto(
    string FirstName, string LastName, string Email,
    string? PhoneNumber, string? Parish, string? OrganisationName,
    string Role, bool IsApproved
);
public record AdminUpdateBoatDto(
    string RegistrationNumber, string BoatName, string BoatType,
    string OwnerName, double LengthFeet, double Latitude, double Longitude
);
public record AdminUpdateMooringDto(
    string MooringNumber, string OwnerName,
    double Latitude, double Longitude, string? BoatSize
);

/// <summary>Admin version of BoatCreateDto – includes optional UserId to assign ownership.</summary>
public class AdminBoatCreateDto : BoatCreateDto
{
    /// <summary>
    /// The user ID to assign the boat to. If omitted, defaults to the calling admin's own ID.
    /// </summary>
    public int? UserId { get; set; }
}

/// <summary>Admin version of MooringCreateDto – includes optional UserId to assign ownership.</summary>
public class AdminMooringCreateDto : MooringCreateDto
{
    public int? UserId { get; set; }
}
