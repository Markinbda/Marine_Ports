namespace MarinePorts.API.Models;

/// <summary>
/// Represents a registered mooring in Bermudian waters.
/// </summary>
public class Mooring
{
    public int Id { get; set; }

    /// <summary>Official mooring registration number.</summary>
    public string MooringNumber { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    // ── Location ──────────────────────────────────────────────────────────────
    public double Latitude  { get; set; }
    public double Longitude { get; set; }

    // ── Boat size ─────────────────────────────────────────────────────────────
    public string? BoatSize { get; set; }

    // ── Photo ─────────────────────────────────────────────────────────────────
    public string? PhotoUrl { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // ── Owner FK (nullable for admin-added records) ───────────────────────────
    public int? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    // ── Fixed colour for all moorings ─────────────────────────────────────────
    public const string ColorCode = "#E91E63"; // Color D – Pink/Red
}
