using MarinePorts.API.Data;
using MarinePorts.API.DTOs;
using MarinePorts.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarinePorts.API.Controllers;

/// <summary>
/// Handles mooring location requests submitted by users.
/// Admins can review and approve/reject; approval creates a Mooring record.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MooringRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    public MooringRequestsController(AppDbContext db) => _db = db;

    // GET /api/mooringrequests  (Admin – all requests)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.MooringRequests
            .Include(r => r.AppUser)
            .Include(r => r.Sector)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync());

    // GET /api/mooringrequests/mine  (user's own requests)
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _db.MooringRequests
            .Include(r => r.Sector)
            .Where(r => r.AppUserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync());
    }

    // POST /api/mooringrequests  – submit a mooring request
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MooringRequestCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Check sector exists and is not full
        var sector = await _db.Sectors.FindAsync(dto.SectorId);
        if (sector is null) return BadRequest(new { message = "Invalid sector." });
        if (sector.IsFull)
            return StatusCode(409, new
            {
                message  = $"The {sector.Parish} sector is currently full. " +
                           $"Please contact Marine & Ports at {sector.ContactEmail}.",
                isFull   = true,
                email    = sector.ContactEmail,
                parish   = sector.Parish
            });

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user   = await _db.Users.FindAsync(userId);
        if (user is null || !user.IsApproved)
            return StatusCode(403, new { message = "Your account must be approved before submitting requests." });

        var request = new MooringRequest
        {
            SectorId     = dto.SectorId,
            AppUserId    = userId,
            OwnerName    = dto.OwnerName.Trim(),
            ContactPhone = dto.ContactPhone?.Trim() ?? string.Empty,
            Latitude     = dto.Latitude,
            Longitude    = dto.Longitude,
            Notes        = dto.Notes?.Trim(),
            Status       = "Pending",
            RequestedAt  = DateTime.UtcNow
        };

        _db.MooringRequests.Add(request);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMine), new { }, new { message = "Your mooring request has been submitted and is pending review.", id = request.Id });
    }

    // PATCH /api/mooringrequests/{id}/review  – Admin approve or reject
    [HttpPatch("{id:int}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewMooringRequestDto dto)
    {
        var request = await _db.MooringRequests.FindAsync(id);
        if (request is null) return NotFound();

        request.Status     = dto.Approve ? "Approved" : "Rejected";
        request.ReviewedAt = DateTime.UtcNow;

        // If approved, create a Mooring record at the requested location
        if (dto.Approve)
        {
            var mooring = new Mooring
            {
                MooringNumber  = dto.MooringNumber?.Trim() ?? $"REQ-{request.Id}",
                OwnerName      = request.OwnerName,
                Latitude       = request.Latitude,
                Longitude      = request.Longitude,
                AppUserId      = request.AppUserId,
                Source         = "Request",
                RegisteredAt   = DateTime.UtcNow,
                RegistrationYear = DateTime.UtcNow.Year
            };
            _db.Moorings.Add(mooring);
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Request {request.Status.ToLower()}." });
    }
}

public record ReviewMooringRequestDto(bool Approve, string? MooringNumber);
