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
        Ok(await _db.Boats.Include(b => b.AppUser)
            .OrderByDescending(b => b.RegisteredAt).ToListAsync());

    // GET /api/boats/mine  – returns only the caller's boats
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _db.Boats.Where(b => b.AppUserId == userId)
            .OrderByDescending(b => b.RegisteredAt).ToListAsync());
    }

    // GET /api/boats/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var boat = await _db.Boats.Include(b => b.AppUser).FirstOrDefaultAsync(b => b.Id == id);
        return boat is null ? NotFound() : Ok(boat);
    }

    // POST /api/boats
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BoatCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
            RegisteredAt       = DateTime.UtcNow
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

        int callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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

        await _db.SaveChangesAsync();
        return Ok(boat);
    }

    // DELETE /api/boats/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var boat = await _db.Boats.FindAsync(id);
        if (boat is null) return NotFound();

        int callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (boat.AppUserId != callerId && !User.IsInRole("Admin"))
            return Forbid();

        _db.Boats.Remove(boat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
