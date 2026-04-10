namespace MarinePorts.API.DTOs;

/// <summary>
/// Returned by the /api/map/search endpoint.
/// Contains enough data for the frontend to fly-to the location and show status.
/// </summary>
public class SearchResultDto
{
    public int    Id                 { get; set; }
    /// <summary>"Boat" | "Mooring"</summary>
    public string Type               { get; set; } = string.Empty;
    public string Label              { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? MooringNumber      { get; set; }
    public int    RegistrationYear   { get; set; }
    /// <summary>"Active" | "Expired – Re-register now" | "Expired – Opens 1 Apr"</summary>
    public string StatusLabel        { get; set; } = string.Empty;
    public bool   CanReRegister      { get; set; }
    public double Latitude           { get; set; }
    public double Longitude          { get; set; }
}
