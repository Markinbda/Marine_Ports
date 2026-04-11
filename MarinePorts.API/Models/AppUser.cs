namespace MarinePorts.API.Models;

/// <summary>
/// Represents a registered user of the Marine &amp; Ports system.
/// Users must register and be approved before they can add boats or moorings.
/// </summary>
public class AppUser
{
    public int Id { get; set; }

    // ── Personal details ──────────────────────────────────────────────────────
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Computed full name for convenience.</summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>Email used for login and correspondence.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt-hashed password. Never store plain text.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Primary contact phone number (e.g. +1 441 555 0100).</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    // ── Address ───────────────────────────────────────────────────────────────
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;

    /// <summary>Bermuda parish (e.g. Pembroke, Sandys, St. George's).</summary>
    public string Parish { get; set; } = string.Empty;

    // ── Organisational / professional details ─────────────────────────────────
    /// <summary>Company or organisation name (optional for individuals).</summary>
    public string? OrganisationName { get; set; }

    /// <summary>
    /// Bermuda marine licence or government-issued ID number for verification.
    /// </summary>
    public string GovernmentIdOrLicenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Role within the system.
    /// Values: BoatOwner | MooringOwner | PortAuthority | Admin
    /// </summary>
    public string Role { get; set; } = "BoatOwner";

    // ── Account state ─────────────────────────────────────────────────────────
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Set to true by an Admin once identity has been verified.
    /// Unapproved users cannot add or edit registrations.
    /// </summary>
    public bool IsApproved { get; set; } = false;

    // ── Navigation properties ─────────────────────────────────────────────────
    public ICollection<Boat>    Boats    { get; set; } = new List<Boat>();
    public ICollection<Mooring> Moorings { get; set; } = new List<Mooring>();
}
