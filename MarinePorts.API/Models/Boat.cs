using System.ComponentModel.DataAnnotations.Schema;

namespace MarinePorts.API.Models;

/// <summary>
/// Represents a registered boat in Bermudian waters.
/// Matches the official Dept. of Marine &amp; Ports Boat Registration Form.
/// Color coding is determined by boat length (feet):
///   ≤10 ft  → Color A (#2196F3 Blue)
///   10–20 ft → Color B (#4CAF50 Green)
///   >20 ft   → Color C (#FF9800 Orange)
/// </summary>
public class Boat
{
    public int Id { get; set; }

    /// <summary>Official Bermuda boat registration number (e.g. BR-0001). Null until assigned by Marine &amp; Ports.</summary>
    public string? RegistrationNumber { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public string BoatName { get; set; } = string.Empty;

    // ── Dimensions ────────────────────────────────────────────────────────────
    /// <summary>Length in feet — drives color coding.</summary>
    public double LengthFeet { get; set; }
    public int? LengthInches { get; set; }
    public int? BeamFeet     { get; set; }
    public int? BeamInches   { get; set; }
    public int? DraughtFeet  { get; set; }
    public int? DraughtInches { get; set; }

    // ── Colours ───────────────────────────────────────────────────────────────
    public string? ColorCabin    { get; set; }
    public string? ColorDecks    { get; set; }
    public string? ColorHull     { get; set; }
    public string? ColorBootLine { get; set; }
    public string? ColorBottom   { get; set; }

    // ── Description ───────────────────────────────────────────────────────────
    /// <summary>Power Boat | Sail Boat | Jet Ski | Punt | Barge | Kayak</summary>
    public string BoatType    { get; set; } = string.Empty;
    public string? Make       { get; set; }
    public string? WhereBuilt { get; set; }
    public string? HullNumber { get; set; }
    public string? Material   { get; set; }
    public int?    YearBuilt  { get; set; }

    // ── Engine ────────────────────────────────────────────────────────────────
    /// <summary>Inboard | In/Outboard | Outboard | Jet</summary>
    public string? EngineType      { get; set; }
    public string? EngineMake      { get; set; }
    public string? EngineSerialVin { get; set; }
    public double? PowerHp         { get; set; }
    /// <summary>Diesel | Gas | Mix</summary>
    public string? Fuel            { get; set; }

    // ── Location ──────────────────────────────────────────────────────────────
    public double Latitude  { get; set; }
    public double Longitude { get; set; }

    // ── Photo ─────────────────────────────────────────────────────────────────
    public string? PhotoUrl { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>The calendar year this registration is valid for (renewed Apr 1–May 31 each year).</summary>
    public int RegistrationYear { get; set; } = DateTime.UtcNow.Year;

    // ── Owner FK ──────────────────────────────────────────────────────────────
    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    // ── Computed colour ───────────────────────────────────────────────────────
    [NotMapped]
    public string ColorCode => LengthFeet <= 10  ? "#2196F3"
                             : LengthFeet <= 20  ? "#4CAF50"
                                                 : "#FF9800";
}
