using MarinePorts.API.Data;
using MarinePorts.API.DTOs;
using MarinePorts.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarinePorts.API.Controllers;

/// <summary>
/// Returns colour-coded map pins and provides a search endpoint.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MapController : ControllerBase
{
    private readonly AppDbContext _db;
    public MapController(AppDbContext db) => _db = db;

    // ── Helper ─────────────────────────────────────────────────────────────────
    private static string BoatColor(double len) =>
        len <= 10 ? "#2196F3" : len <= 20 ? "#4CAF50" : "#FF9800";

    private static string RegStatus(int regYear)
    {
        var today = DateTime.UtcNow;
        if (regYear >= today.Year) return "Active";
        bool inWindow = today.Month == 4 || today.Month == 5;
        return inWindow ? "Expired – Re-register now" : "Expired – Re-registration opens 1 Apr";
    }

    // GET /api/map/pins  (public – all records)
    [HttpGet("pins")]
    public async Task<IActionResult> GetPins()
    {
        var boatPins = await _db.Boats
            .Select(b => new MapPinDto
            {
                Id               = b.Id,
                Type             = "Boat",
                Latitude         = b.Latitude,
                Longitude        = b.Longitude,
                ColorCode        = b.LengthFeet <= 10 ? "#2196F3" : b.LengthFeet <= 20 ? "#4CAF50" : "#FF9800",
                PhotoUrl         = b.PhotoUrl,
                Label            = $"{b.OwnerName} – {b.LengthFeet} ft ({b.BoatName})",
                RegistrationYear = b.RegistrationYear
            })
            .ToListAsync();

        var mooringPins = await _db.Moorings
            .Select(m => new MapPinDto
            {
                Id               = m.Id,
                Type             = "Mooring",
                Latitude         = m.Latitude,
                Longitude        = m.Longitude,
                ColorCode        = "#E91E63",
                PhotoUrl         = m.PhotoUrl,
                Label            = m.MooringNumber,
                RegistrationYear = m.RegistrationYear
            })
            .ToListAsync();

        return Ok(boatPins.Concat(mooringPins));
    }

    // GET /api/map/pins/mine – calling user's boats, moorings and pending requests
    [HttpGet("pins/mine")]
    [Authorize]
    public async Task<IActionResult> GetMyPins()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var boatPins = await _db.Boats
            .Where(b => b.AppUserId == userId)
            .Select(b => new MapPinDto
            {
                Id               = b.Id,
                Type             = "Boat",
                Latitude         = b.Latitude,
                Longitude        = b.Longitude,
                ColorCode        = b.LengthFeet <= 10 ? "#2196F3" : b.LengthFeet <= 20 ? "#4CAF50" : "#FF9800",
                PhotoUrl         = b.PhotoUrl,
                Label            = $"{b.OwnerName} – {b.LengthFeet} ft ({b.BoatName})",
                RegistrationYear = b.RegistrationYear
            })
            .ToListAsync();

        var mooringPins = await _db.Moorings
            .Where(m => m.AppUserId == userId)
            .Select(m => new MapPinDto
            {
                Id               = m.Id,
                Type             = "Mooring",
                Latitude         = m.Latitude,
                Longitude        = m.Longitude,
                ColorCode        = "#E91E63",
                PhotoUrl         = m.PhotoUrl,
                Label            = m.MooringNumber,
                RegistrationYear = m.RegistrationYear
            })
            .ToListAsync();

        // Pending mooring requests shown as grey pins
        var requestPins = await _db.MooringRequests
            .Where(r => r.AppUserId == userId && r.Status == "Pending")
            .Select(r => new MapPinDto
            {
                Id               = r.Id,
                Type             = "MooringRequest",
                Latitude         = r.Latitude,
                Longitude        = r.Longitude,
                ColorCode        = "#9E9E9E",
                PhotoUrl         = null,
                Label            = $"⏳ Pending – {r.Sector!.Parish}",
                RegistrationYear = 0,
                IsPending        = true
            })
            .ToListAsync();

        return Ok(boatPins.Concat(mooringPins).Concat(requestPins));
    }

    // GET /api/map/search?q=...  – search boats + moorings by number/name/owner
    // Public callers only see mooring number + location; Admin sees full registration detail.
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Ok(Array.Empty<SearchResultDto>());

        var term     = q.Trim().ToLower();
        var today    = DateTime.UtcNow;
        bool inWindow = today.Month == 4 || today.Month == 5;
        bool isAdmin  = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");

        var boats = await _db.Boats
            .Where(b => b.RegistrationNumber.ToLower().Contains(term)
                     || b.BoatName.ToLower().Contains(term)
                     || b.OwnerName.ToLower().Contains(term))
            .ToListAsync();

        // Non-admin: search moorings by number only (no owner-name lookup)
        var moorings = await _db.Moorings
            .Where(m => m.MooringNumber.ToLower().Contains(term)
                     || (isAdmin && m.OwnerName.ToLower().Contains(term)))
            .ToListAsync();

        var results = new List<SearchResultDto>();

        foreach (var b in boats)
        {
            bool expired = b.RegistrationYear < today.Year;
            results.Add(new SearchResultDto
            {
                Id                 = b.Id,
                Type               = "Boat",
                Label              = $"{b.BoatName} – {b.OwnerName} ({b.LengthFeet} ft)",
                RegistrationNumber = b.RegistrationNumber,
                RegistrationYear   = b.RegistrationYear,
                StatusLabel        = expired
                                       ? (inWindow ? "Expired – Re-register now" : "Expired – Re-registration opens 1 Apr")
                                       : "Active",
                CanReRegister      = expired && inWindow,
                Latitude           = b.Latitude,
                Longitude          = b.Longitude
            });
        }

        foreach (var m in moorings)
        {
            bool expired = m.RegistrationYear < today.Year;
            // Public callers receive only the mooring number and co-ordinates
            results.Add(new SearchResultDto
            {
                Id               = m.Id,
                Type             = "Mooring",
                Label            = isAdmin
                                     ? $"Mooring {m.MooringNumber} – {m.OwnerName}"
                                     : $"Mooring {m.MooringNumber}",
                MooringNumber    = m.MooringNumber,
                RegistrationYear = isAdmin ? m.RegistrationYear : 0,
                StatusLabel      = isAdmin
                                     ? (expired
                                         ? (inWindow ? "Expired – Re-register now" : "Expired – Re-registration opens 1 Apr")
                                         : "Active")
                                     : string.Empty,
                CanReRegister    = isAdmin && expired && inWindow,
                Latitude         = m.Latitude,
                Longitude        = m.Longitude
            });
        }

        return Ok(results);
    }
}

