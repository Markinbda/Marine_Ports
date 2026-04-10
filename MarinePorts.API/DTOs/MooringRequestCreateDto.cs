using System.ComponentModel.DataAnnotations;

namespace MarinePorts.API.DTOs;

/// <summary>
/// Payload for submitting a mooring location request.
/// </summary>
public class MooringRequestCreateDto
{
    [Required]
    public int SectorId { get; set; }

    [Required(ErrorMessage = "Owner name is required.")]
    [StringLength(150)]
    public string OwnerName { get; set; } = string.Empty;

    [StringLength(50)]
    public string ContactPhone { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
