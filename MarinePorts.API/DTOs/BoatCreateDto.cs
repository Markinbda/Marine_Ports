using System.ComponentModel.DataAnnotations;

namespace MarinePorts.API.DTOs;

/// <summary>
/// Payload for creating or updating a boat registration.
/// Matches the official Dept. of Marine &amp; Ports Boat Registration Form.
/// </summary>
public class BoatCreateDto
{
    // ── Identity ──────────────────────────────────────────────────────────────
    [StringLength(50)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Owner name is required.")]
    [StringLength(150)]
    public string OwnerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Boat name is required.")]
    [StringLength(150)]
    public string BoatName { get; set; } = string.Empty;

    // ── Dimensions ────────────────────────────────────────────────────────────
    [Required(ErrorMessage = "Boat length (feet) is required.")]
    [Range(0.1, 2000)]
    public double LengthFeet { get; set; }

    [Range(0, 11)] public int? LengthInches  { get; set; }
    [Range(0, 999)] public int? BeamFeet     { get; set; }
    [Range(0, 11)] public int? BeamInches    { get; set; }
    [Range(0, 999)] public int? DraughtFeet  { get; set; }
    [Range(0, 11)] public int? DraughtInches { get; set; }

    // ── Colours ───────────────────────────────────────────────────────────────
    [StringLength(50)] public string? ColorCabin    { get; set; }
    [StringLength(50)] public string? ColorDecks    { get; set; }
    [StringLength(50)] public string? ColorHull     { get; set; }
    [StringLength(50)] public string? ColorBootLine { get; set; }
    [StringLength(50)] public string? ColorBottom   { get; set; }

    // ── Description ───────────────────────────────────────────────────────────
    [StringLength(80)]  public string  BoatType   { get; set; } = string.Empty;
    [StringLength(100)] public string? Make       { get; set; }
    [StringLength(100)] public string? WhereBuilt { get; set; }
    [StringLength(80)]  public string? HullNumber { get; set; }
    [StringLength(80)]  public string? Material   { get; set; }
    [Range(1800, 2100)] public int?    YearBuilt  { get; set; }

    // ── Engine ────────────────────────────────────────────────────────────────
    [StringLength(50)]  public string? EngineType      { get; set; }
    [StringLength(100)] public string? EngineMake      { get; set; }
    [StringLength(100)] public string? EngineSerialVin { get; set; }
    [Range(0, 10000)]   public double? PowerHp         { get; set; }
    [StringLength(20)]  public string? Fuel             { get; set; }

    // ── Location ──────────────────────────────────────────────────────────────
    [Range(-90, 90)]   public double Latitude  { get; set; }
    [Range(-180, 180)] public double Longitude { get; set; }

    public string? PhotoUrl { get; set; }
}
