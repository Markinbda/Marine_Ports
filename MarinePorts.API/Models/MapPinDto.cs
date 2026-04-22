namespace MarinePorts.API.Models;

/// <summary>
/// Data Transfer Object returned by the /api/map/pins endpoint.
/// Consumed by the Leaflet.js frontend to render colour-coded markers.
/// </summary>
public class MapPinDto
{
    /// <summary>Unique identifier (Boat or Mooring ID)</summary>
    public int Id           { get; set; }

    /// <summary>Owning user ID for filtering "My Boats/Moorings" on the map.</summary>
    public int? AppUserId   { get; set; }

    /// <summary>"Boat" or "Mooring"</summary>
    public string Type      { get; set; } = string.Empty;
    public double Latitude  { get; set; }
    public double Longitude { get; set; }

    /// <summary>Hex colour code used for the map marker.</summary>
    public string ColorCode { get; set; } = string.Empty;

    /// <summary>URL path for the photo popup, e.g. /images/boats/boat-1.jpg</summary>
    public string? PhotoUrl  { get; set; }

    /// <summary>
    /// Human-readable label shown in the popup.
    /// Boats:    "{OwnerName} – {LengthFeet} ft"
    /// Moorings: "{OwnerName} – Mooring {MooringNumber}"
    /// </summary>
    public string Label      { get; set; } = string.Empty;

    /// <summary>Short identifier shown directly on the map marker at close zoom (Reg# or Mooring#).</summary>
    public string ShortLabel { get; set; } = string.Empty;
}
