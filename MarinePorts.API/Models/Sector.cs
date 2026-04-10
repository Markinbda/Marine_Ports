namespace MarinePorts.API.Models;

/// <summary>
/// Represents a mooring sector defined by Bermuda parish.
/// Admins can mark a sector as full to prevent new mooring requests.
/// </summary>
public class Sector
{
    public int Id { get; set; }

    /// <summary>Bermuda parish name – also used as the display label.</summary>
    public string Parish { get; set; } = string.Empty;

    /// <summary>When true, new mooring requests in this sector are blocked.</summary>
    public bool IsFull { get; set; } = false;

    /// <summary>Contact email shown to users when sector is full.</summary>
    public string ContactEmail { get; set; } = "marineports@gov.bm";

    // ── Approximate map centre for fly-to ─────────────────────────────────────
    public double CenterLat { get; set; }
    public double CenterLng { get; set; }

    /// <summary>Default zoom level when this sector is selected.</summary>
    public int Zoom { get; set; } = 14;

    public ICollection<MooringRequest> MooringRequests { get; set; } = new List<MooringRequest>();
}
