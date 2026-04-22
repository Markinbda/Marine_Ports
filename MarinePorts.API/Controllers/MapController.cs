using MarinePorts.API.Data;
using MarinePorts.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;
    private const string PinsCacheKey = "map-pins-v1";

    public MapController(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // GET /api/map/pins
    [HttpGet("pins")]
    public async Task<IActionResult> GetPins()
    {
        if (_cache.TryGetValue(PinsCacheKey, out List<MapPinDto>? cachedPins) && cachedPins is not null)
        {
            Response.Headers.CacheControl = "public,max-age=30";
            return Ok(cachedPins);
        }

        // Skip EF change tracking for read-only map queries.
        var boatPins = await _db.Boats
            .AsNoTracking()
            .Select(b => new MapPinDto
            {
                Id        = b.Id,
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

        var mooringData = await _db.Moorings
            .AsNoTracking()
            .Select(m => new { m.Id, m.Latitude, m.Longitude, m.PhotoUrl, m.OwnerName, m.MooringNumber, m.BoatSize })
            .ToListAsync();

        var mooringPins = mooringData.Select(m => new MapPinDto
        {
            Id        = m.Id,
            Type      = "Mooring",
            Latitude  = m.Latitude,
            Longitude = m.Longitude,
            ColorCode = MooringColor(m.BoatSize),
            PhotoUrl   = m.PhotoUrl,
            Label      = $"{m.OwnerName} – Mooring {m.MooringNumber}",
            ShortLabel = m.MooringNumber
        });

        var pins = boatPins.Concat(mooringPins).ToList();
        _cache.Set(PinsCacheKey, pins, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        });

        Response.Headers.CacheControl = "public,max-age=30";
        return Ok(pins);
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
