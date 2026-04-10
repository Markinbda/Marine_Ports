namespace MarinePorts.API.Models;

/// <summary>
/// Represents a pending mooring location request submitted by a logged-in user.
/// An Admin reviews and either approves (which creates a Mooring record) or rejects it.
/// </summary>
public class MooringRequest
{
    public int Id { get; set; }

    // ── Sector / parish ───────────────────────────────────────────────────────
    public int SectorId { get; set; }
    public Sector? Sector { get; set; }

    // ── Owner ─────────────────────────────────────────────────────────────────
    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public string OwnerName    { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;

    // ── Requested location ────────────────────────────────────────────────────
    public double Latitude  { get; set; }
    public double Longitude { get; set; }

    // ── Additional details ────────────────────────────────────────────────────
    public string? Notes { get; set; }

    // ── Workflow ──────────────────────────────────────────────────────────────
    /// <summary>Pending | Approved | Rejected</summary>
    public string Status { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
