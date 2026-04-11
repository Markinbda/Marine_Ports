using MarinePorts.API.Data;
using MarinePorts.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarinePorts.API.Controllers;

/// <summary>
/// Returns all boats and moorings as colour-coded map pins.
/// GET /api/map/pins – public endpoint consumed by the Leaflet.js frontend.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MapController : ControllerBase
{
    private readonly AppDbContext _db;
    public MapController(AppDbContext db) => _db = db;

    // GET /api/map/pins
    [HttpGet("pins")]
    public async Task<IActionResult> GetPins()
    {
        // Fetch boats and project to MapPinDto
        var boatPins = await _db.Boats
            .Select(b => new MapPinDto
            {
                Type      = "Boat",
                Latitude  = b.Latitude,
                Longitude = b.Longitude,
                ColorCode = b.LengthFeet <= 10 ? "#2196F3"   // Color A – Blue
                          : b.LengthFeet <= 20 ? "#4CAF50"   // Color B – Green
                                               : "#FF9800",  // Color C – Orange
                PhotoUrl   = b.PhotoUrl,
                Label      = $"{b.OwnerName} – {b.LengthFeet} ft ({b.BoatName})",
                ShortLabel = b.RegistrationNumber
            })
            .ToListAsync();

        // Fetch moorings – color after DB fetch so we can use C# string logic
        var mooringData = await _db.Moorings
            .Select(m => new { m.Latitude, m.Longitude, m.PhotoUrl, m.OwnerName, m.MooringNumber, m.BoatSize })
            .ToListAsync();

        var mooringPins = mooringData.Select(m => new MapPinDto
        {
            Type      = "Mooring",
            Latitude  = m.Latitude,
            Longitude = m.Longitude,
            ColorCode = MooringColor(m.BoatSize),
            PhotoUrl   = m.PhotoUrl,
            Label      = $"{m.OwnerName} \u2013 Mooring {m.MooringNumber}",
            ShortLabel = m.MooringNumber
        }).ToList();

        return Ok(boatPins.Concat(mooringPins));
    }

    private static string MooringColor(string? boatSize)
    {
        var s = boatSize?.ToLowerInvariant() ?? string.Empty;
        if (s.Contains("small"))  return "#9C27B0"; // Purple  – small mooring
        if (s.Contains("med"))    return "#E91E63"; // Pink    – medium mooring
        if (s.Contains("large")) return "#F44336"; // Red     – large mooring
        return "#FF9800"; // Orange – unknown size
    }
}
