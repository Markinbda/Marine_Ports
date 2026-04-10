using MarinePorts.API.Data;
using MarinePorts.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarinePorts.API.Controllers;

/// <summary>
/// Manages mooring sectors (one per Bermuda parish).
/// GET is public. Status updates are Admin-only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SectorsController : ControllerBase
{
    private readonly AppDbContext _db;
    public SectorsController(AppDbContext db) => _db = db;

    // GET /api/sectors
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sectors = await _db.Sectors.OrderBy(s => s.Parish).ToListAsync();
        return Ok(sectors);
    }

    // PATCH /api/sectors/{id}/status  { "isFull": true }
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetSectorStatusDto dto)
    {
        var sector = await _db.Sectors.FindAsync(id);
        if (sector is null) return NotFound();
        sector.IsFull = dto.IsFull;
        await _db.SaveChangesAsync();
        return Ok(sector);
    }
}

public record SetSectorStatusDto(bool IsFull);
