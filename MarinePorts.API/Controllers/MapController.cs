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
                PhotoUrl  = b.PhotoUrl,
                Label     = $"{b.OwnerName} – {b.LengthFeet} ft ({b.BoatName})"
            })
            .ToListAsync();

        // Fetch moorings and project to MapPinDto
        var mooringPins = await _db.Moorings
            .Select(m => new MapPinDto
            {
                Type      = "Mooring",
                Latitude  = m.Latitude,
                Longitude = m.Longitude,
                ColorCode = "#E91E63",  // Color D – Pink/Red (all moorings)
                PhotoUrl  = m.PhotoUrl,
                Label     = $"{m.OwnerName} – Mooring {m.MooringNumber}"
            })
            .ToListAsync();

        return Ok(boatPins.Concat(mooringPins));
    }
}
