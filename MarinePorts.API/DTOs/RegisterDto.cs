using System.ComponentModel.DataAnnotations;

namespace MarinePorts.API.DTOs;

/// <summary>
/// Payload sent by the frontend registration form to POST /api/auth/register.
/// All fields marked [Required] must be present; validation errors return HTTP 400.
/// </summary>
public class RegisterDto
{
    // ── Personal details ──────────────────────────────────────────────────────
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string PhoneNumber { get; set; } = string.Empty;

    // ── Address ───────────────────────────────────────────────────────────────
    [Required(ErrorMessage = "Address line 1 is required.")]
    [StringLength(200)]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(200)]
    public string AddressLine2 { get; set; } = string.Empty;

    /// <summary>
    /// One of: Pembroke, Sandys, St. George's, Hamilton, Devonshire,
    ///         Paget, Warwick, Southampton, Smith's, St. David's
    /// </summary>
    [Required(ErrorMessage = "Parish is required.")]
    public string Parish { get; set; } = string.Empty;

    // ── Professional / organisational details (optional – removed from public form) ───
    /// <summary>Optional – company or organisation name.</summary>
    public string? OrganisationName { get; set; }

    /// <summary>Optional – Bermuda government-issued ID or marine licence number.</summary>
    [StringLength(50)]
    public string? GovernmentIdOrLicenceNumber { get; set; }

    /// <summary>
    /// Requested account role. Defaults to BoatOwner.
    /// Values: BoatOwner | MooringOwner | PortAuthority
    /// </summary>
    public string Role { get; set; } = "BoatOwner";
}
