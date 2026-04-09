using System.ComponentModel.DataAnnotations;

namespace MarinePorts.API.DTOs;

/// <summary>
/// Payload sent by the login form to POST /api/auth/login.
/// Returns a JWT token on success.
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
