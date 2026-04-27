using MarinePorts.API.Data;
using MarinePorts.API.DTOs;
using MarinePorts.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarinePorts.API.Controllers;

/// <summary>
/// CRUD operations for registered boats.
/// GET endpoints are public. Write operations require an authenticated approved user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoatsController : ControllerBase
{
    private readonly AppDbContext _db;
    public BoatsController(AppDbContext db) => _db = db;

    // GET /api/boats
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Boats.Where(b => b.IsApproved)
            .Include(b => b.AppUser)
            .OrderByDescending(b => b.RegisteredAt).ToListAsync());

    // GET /api/boats/mine  – returns only the caller's boats
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        if (!TryGetCurrentUserId(out int userId))
            return Unauthorized(new { message = "Invalid token: missing user identifier claim." });

        return Ok(await _db.Boats.Where(b => b.AppUserId == userId)
            .OrderByDescending(b => b.RegisteredAt).ToListAsync());
    }

    // GET /api/boats/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var boat = await _db.Boats.Include(b => b.AppUser).FirstOrDefaultAsync(b => b.Id == id);
        if (boat is not null && !boat.IsApproved)
            return NotFound();
        return boat is null ? NotFound() : Ok(boat);
    }

    // POST /api/boats
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BoatCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!TryGetCurrentUserId(out int userId))
            return Unauthorized(new { message = "Invalid token: missing user identifier claim." });

        var user   = await _db.Users.FindAsync(userId);
        if (user is null || !user.IsApproved)
            return StatusCode(403, new { message = "Your account must be approved before adding registrations." });

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
            AppUserId          = userId,
            RegisteredAt       = DateTime.UtcNow,
            IsApproved         = false,
            RenewalRequestedAt = DateTime.UtcNow,
            ExpiresAt          = null
        };

        _db.Boats.Add(boat);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = boat.Id }, boat);
    }

    // PUT /api/boats/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BoatCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var boat = await _db.Boats.FindAsync(id);
        if (boat is null) return NotFound();

        if (!TryGetCurrentUserId(out int callerId))
            return Unauthorized(new { message = "Invalid token: missing user identifier claim." });

        if (boat.AppUserId != callerId && !User.IsInRole("Admin"))
            return Forbid();

        boat.RegistrationNumber = dto.RegistrationNumber.Trim();
        boat.OwnerName          = dto.OwnerName.Trim();
        boat.BoatName           = dto.BoatName.Trim();
        boat.BoatType           = dto.BoatType.Trim();
        boat.LengthFeet         = dto.LengthFeet;
        boat.Latitude           = dto.Latitude;
        boat.Longitude          = dto.Longitude;
        boat.PhotoUrl           = dto.PhotoUrl;
        boat.IsApproved         = false;
        boat.RenewalRequestedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new
        {
            message = "Boat details updated and submitted for approval.",
            boat
        });
    }

    // PUT /api/boats/{id}/renewal-request
    [HttpPut("{id:int}/renewal-request")]
    public async Task<IActionResult> RequestRenewal(int id)
    {
        var boat = await _db.Boats.FindAsync(id);
        if (boat is null) return NotFound();

        if (!TryGetCurrentUserId(out int callerId))
            return Unauthorized(new { message = "Invalid token: missing user identifier claim." });

        if (boat.AppUserId != callerId && !User.IsInRole("Admin"))
            return Forbid();

        boat.IsApproved         = false;
        boat.RenewalRequestedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Boat renewal request submitted and pending admin approval." });
    }

    // DELETE /api/boats/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var boat = await _db.Boats.FindAsync(id);
        if (boat is null) return NotFound();

        if (!TryGetCurrentUserId(out int callerId))
            return Unauthorized(new { message = "Invalid token: missing user identifier claim." });

        if (boat.AppUserId != callerId && !User.IsInRole("Admin"))
            return Forbid();

        _db.Boats.Remove(boat);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        var candidates = new[]
        {
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.FindFirstValue("sub"),
            User.FindFirstValue(ClaimTypes.Sid)
        };

        foreach (var candidate in candidates)
        {
            if (int.TryParse(candidate, out userId))
                return true;
        }

        return false;
    }
}
