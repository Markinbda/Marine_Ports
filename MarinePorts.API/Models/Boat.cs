namespace MarinePorts.API.Models;

/// <summary>
/// Represents a registered boat in Bermudian waters.
/// Color coding is determined by boat length (feet):
///   ≤10 ft  → Color A (#2196F3 Blue)
///   10–20 ft → Color B (#4CAF50 Green)
///   >20 ft   → Color C (#FF9800 Orange)
/// </summary>
public class Boat
{
    public int Id { get; set; }

    /// <summary>Official Bermuda boat registration number (e.g. BR-0001).</summary>
    public string RegistrationNumber { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    /// <summary>Length of the boat in feet — drives color coding.</summary>
    public double LengthFeet { get; set; }

    public string BoatName { get; set; } = string.Empty;
    public string BoatType { get; set; } = string.Empty;  // e.g. Dinghy, Sailboat, Motorboat

    // ── Location ──────────────────────────────────────────────────────────────
    public double Latitude  { get; set; }
    public double Longitude { get; set; }

    // ── Photo ─────────────────────────────────────────────────────────────────
    /// <summary>Relative path under wwwroot, e.g. /images/boats/boat-42.jpg</summary>
    public string? PhotoUrl { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // ── Owner FK ──────────────────────────────────────────────────────────────
    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    // ── Computed colour ───────────────────────────────────────────────────────
    /// <summary>Returns the hex color code based on boat length.</summary>
    public string ColorCode => LengthFeet <= 10  ? "#2196F3"   // Color A – Blue
                             : LengthFeet <= 20  ? "#4CAF50"   // Color B – Green
                                                 : "#FF9800";  // Color C – Orange
}
