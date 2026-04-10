namespace MarinePorts.API.Models;

/// <summary>
/// Data Transfer Object returned by the /api/map/pins endpoint.
/// Consumed by the Leaflet.js frontend to render colour-coded markers.
/// </summary>
public class MapPinDto
{
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
    public string Label            { get; set; } = string.Empty;

    public int    Id               { get; set; }
    public int    RegistrationYear { get; set; }
    /// <summary>True for pending mooring requests (shown differently on the map).</summary>
    public bool   IsPending        { get; set; }
}
