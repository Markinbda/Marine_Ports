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
            RegistrationNumber = string.IsNullOrWhiteSpace(dto.RegistrationNumber) ? null : dto.RegistrationNumber.Trim(),
            OwnerName          = dto.OwnerName.Trim(),
            BoatName           = dto.BoatName.Trim(),
            BoatType           = dto.BoatType?.Trim() ?? string.Empty,
            LengthFeet         = dto.LengthFeet,
            LengthInches       = dto.LengthInches,
            BeamFeet           = dto.BeamFeet,
            BeamInches         = dto.BeamInches,
            DraughtFeet        = dto.DraughtFeet,
            DraughtInches      = dto.DraughtInches,
            ColorCabin         = dto.ColorCabin,
            ColorDecks         = dto.ColorDecks,
            ColorHull          = dto.ColorHull,
            ColorBootLine      = dto.ColorBootLine,
            ColorBottom        = dto.ColorBottom,
            Make               = dto.Make,
            WhereBuilt         = dto.WhereBuilt,
            HullNumber         = dto.HullNumber,
            Material           = dto.Material,
            YearBuilt          = dto.YearBuilt,
            EngineType         = dto.EngineType,
            EngineMake         = dto.EngineMake,
            EngineSerialVin    = dto.EngineSerialVin,
            PowerHp            = dto.PowerHp,
            Fuel               = dto.Fuel,
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

        boat.RegistrationNumber = string.IsNullOrWhiteSpace(dto.RegistrationNumber) ? null : dto.RegistrationNumber.Trim();
        boat.OwnerName          = dto.OwnerName.Trim();
        boat.BoatName           = dto.BoatName.Trim();
        boat.BoatType           = dto.BoatType?.Trim() ?? string.Empty;
        boat.LengthFeet         = dto.LengthFeet;
        boat.LengthInches       = dto.LengthInches;
        boat.BeamFeet           = dto.BeamFeet;
        boat.BeamInches         = dto.BeamInches;
        boat.DraughtFeet        = dto.DraughtFeet;
        boat.DraughtInches      = dto.DraughtInches;
        boat.ColorCabin         = dto.ColorCabin;
        boat.ColorDecks         = dto.ColorDecks;
        boat.ColorHull          = dto.ColorHull;
        boat.ColorBootLine      = dto.ColorBootLine;
        boat.ColorBottom        = dto.ColorBottom;
        boat.Make               = dto.Make;
        boat.WhereBuilt         = dto.WhereBuilt;
        boat.HullNumber         = dto.HullNumber;
        boat.Material           = dto.Material;
        boat.YearBuilt          = dto.YearBuilt;
        boat.EngineType         = dto.EngineType;
        boat.EngineMake         = dto.EngineMake;
        boat.EngineSerialVin    = dto.EngineSerialVin;
        boat.PowerHp            = dto.PowerHp;
        boat.Fuel               = dto.Fuel;
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

    // POST /api/boats/{id}/reregister – renew annual registration (Apr 1–May 31)
    [HttpPost("{id:int}/reregister")]
    public async Task<IActionResult> ReRegister(int id)
    {
        var today = DateTime.UtcNow;
        if (!(today.Month == 4 || today.Month == 5))
            return BadRequest(new { message = "Re-registration is only permitted between 1 April and 31 May each year." });

        var boat = await _db.Boats.FindAsync(id);
        if (boat is null) return NotFound();

        int callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (boat.AppUserId != callerId && !User.IsInRole("Admin"))
            return Forbid();

        boat.RegistrationYear = today.Year;
        boat.RegisteredAt     = today;
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Boat registration renewed for {today.Year}.", registrationYear = today.Year });
    }
}
