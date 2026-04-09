using System.ComponentModel.DataAnnotations;

namespace MarinePorts.API.DTOs;

/// <summary>
/// Payload for creating or updating a mooring registration.
/// </summary>
public class MooringCreateDto
{
    [Required(ErrorMessage = "Mooring number is required.")]
    [StringLength(50)]
    public string MooringNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Owner name is required.")]
    [StringLength(150)]
    public string OwnerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }

    /// <summary>Relative URL populated after a successful photo upload, e.g. /images/moorings/abc.jpg</summary>
    public string? PhotoUrl { get; set; }
}
