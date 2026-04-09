namespace MarinePorts.API.Models;

/// <summary>
/// Represents a registered mooring in Bermudian waters.
/// All moorings use Color D (#E91E63 Pink/Red) regardless of size.
/// </summary>
public class Mooring
{
    public int Id { get; set; }

    /// <summary>Official mooring number assigned by the Port Authority.</summary>
    public string MooringNumber { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    // ── Location ──────────────────────────────────────────────────────────────
    public double Latitude  { get; set; }
    public double Longitude { get; set; }

    // ── Photo ─────────────────────────────────────────────────────────────────
    /// <summary>Relative path under wwwroot, e.g. /images/moorings/mooring-7.jpg</summary>
    public string? PhotoUrl { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // ── ArcGIS import fields ──────────────────────────────────────────────────
    /// <summary>Colour as recorded in the ArcGIS survey (e.g. White, Yellow).</summary>
    public string? Colour { get; set; }

    /// <summary>"ArcGIS" for imported records; null for user-registered.</summary>
    public string? Source { get; set; }

    // ── Owner FK (nullable – ArcGIS-imported records have no system user) ─────
    public int? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    // ── Fixed colour for all moorings ─────────────────────────────────────────
    public const string ColorCode = "#E91E63"; // Color D – Pink/Red
}
