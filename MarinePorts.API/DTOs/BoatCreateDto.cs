using System.ComponentModel.DataAnnotations;

namespace MarinePorts.API.DTOs;

/// <summary>
/// Payload for creating or updating a boat registration.
/// Used by both the authenticated user endpoint and the admin endpoint.
/// </summary>
public class BoatCreateDto
{
    [Required(ErrorMessage = "Registration number is required.")]
    [StringLength(50)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Owner name is required.")]
    [StringLength(150)]
    public string OwnerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Boat name is required.")]
    [StringLength(150)]
    public string BoatName { get; set; } = string.Empty;

    /// <summary>e.g. Dinghy, Sailboat, Motorboat, Yacht, Fishing Vessel</summary>
    [StringLength(80)]
    public string BoatType { get; set; } = string.Empty;

    /// <summary>Length in feet – determines the colour code on the map.</summary>
    [Required(ErrorMessage = "Boat length is required.")]
    [Range(0.1, 2000, ErrorMessage = "Length must be between 0.1 and 2000 feet.")]
    public double LengthFeet { get; set; }

    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }

    /// <summary>Relative URL populated after a successful photo upload, e.g. /images/boats/abc.jpg</summary>
    public string? PhotoUrl { get; set; }
}
