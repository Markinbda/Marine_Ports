using MarinePorts.API.Data;
using MarinePorts.API.DTOs;
using MarinePorts.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarinePorts.API.Controllers;

/// <summary>
/// CRUD operations for registered moorings.
/// GET endpoints are public. Write operations require an authenticated approved user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MooringsController : ControllerBase
{
    private readonly AppDbContext _db;
    public MooringsController(AppDbContext db) => _db = db;

    // GET /api/moorings
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Moorings.Include(m => m.AppUser)
            .OrderByDescending(m => m.RegisteredAt).ToListAsync());

    // GET /api/moorings/mine  – returns only the caller's moorings
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _db.Moorings.Where(m => m.AppUserId == userId)
            .OrderByDescending(m => m.RegisteredAt).ToListAsync());
    }

    // GET /api/moorings/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var mooring = await _db.Moorings.Include(m => m.AppUser).FirstOrDefaultAsync(m => m.Id == id);
        return mooring is null ? NotFound() : Ok(mooring);
    }

    // POST /api/moorings
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MooringCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user   = await _db.Users.FindAsync(userId);
        if (user is null || !user.IsApproved)
            return StatusCode(403, new { message = "Your account must be approved before adding registrations." });

        var mooring = new Mooring
        {
            MooringNumber = dto.MooringNumber.Trim(),
            OwnerName     = dto.OwnerName.Trim(),
            Latitude      = dto.Latitude,
            Longitude     = dto.Longitude,
            PhotoUrl      = dto.PhotoUrl,
            AppUserId     = userId,
            RegisteredAt  = DateTime.UtcNow
        };

        _db.Moorings.Add(mooring);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = mooring.Id }, mooring);
    }

    // PUT /api/moorings/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MooringCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var mooring = await _db.Moorings.FindAsync(id);
        if (mooring is null) return NotFound();

        int callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (mooring.AppUserId != callerId && !User.IsInRole("Admin"))
            return Forbid();

        mooring.MooringNumber = dto.MooringNumber.Trim();
        mooring.OwnerName     = dto.OwnerName.Trim();
        mooring.Latitude      = dto.Latitude;
        mooring.Longitude     = dto.Longitude;
        mooring.PhotoUrl      = dto.PhotoUrl;

        await _db.SaveChangesAsync();
        return Ok(mooring);
    }

    // DELETE /api/moorings/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var mooring = await _db.Moorings.FindAsync(id);
        if (mooring is null) return NotFound();

        int callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (mooring.AppUserId != callerId && !User.IsInRole("Admin"))
            return Forbid();

        _db.Moorings.Remove(mooring);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/moorings/{id}/reregister – renew annual registration (Apr 1–May 31)
    [HttpPost("{id:int}/reregister")]
    public async Task<IActionResult> ReRegister(int id)
    {
        var today = DateTime.UtcNow;
        if (!(today.Month == 4 || today.Month == 5))
            return BadRequest(new { message = "Re-registration is only permitted between 1 April and 31 May each year." });

        var mooring = await _db.Moorings.FindAsync(id);
        if (mooring is null) return NotFound();

        int callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (mooring.AppUserId != callerId && !User.IsInRole("Admin"))
            return Forbid();

        mooring.RegistrationYear = today.Year;
        mooring.RegisteredAt     = today;
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Mooring registration renewed for {today.Year}.", registrationYear = today.Year });
    }
}
