using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarinePorts.API.Controllers;

/// <summary>
/// Handles photo uploads for boats and moorings.
/// POST /api/upload/image  – saves file to wwwroot/images/{subfolder} and returns the URL.
/// Requires an authenticated (approved) user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    // Allowed MIME types – reject anything that is not a genuine image.
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    // Maximum file size: 5 MB
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public UploadController(IWebHostEnvironment env) => _env = env;

    /// <summary>
    /// Upload a photo for a boat or mooring.
    /// Form field: file     – the image file
    /// Form field: category – "boats" or "moorings" (determines subfolder)
    /// </summary>
    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        [FromForm] string category = "boats")
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file was uploaded." });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = "File size must not exceed 5 MB." });

        if (!AllowedMimeTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Only JPEG, PNG, and WebP images are accepted." });

        // Sanitise the category to prevent path traversal attacks.
        string safeCategory = category.ToLower() == "moorings" ? "moorings" : "boats";

        // Build a unique filename using a GUID to prevent overwrites and enumeration.
        string extension  = Path.GetExtension(file.FileName).ToLower();
        string uniqueName = $"{Guid.NewGuid()}{extension}";

        // Ensure the target directory exists.
        string uploadDir = Path.Combine(_env.WebRootPath, "images", safeCategory);
        Directory.CreateDirectory(uploadDir);

        string physicalPath = Path.Combine(uploadDir, uniqueName);

        // Stream directly to disk – avoids buffering the entire file in memory.
        await using var stream = new FileStream(physicalPath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Return the relative URL that can be stored in Boat.PhotoUrl / Mooring.PhotoUrl.
        string relativeUrl = $"/images/{safeCategory}/{uniqueName}";
        return Ok(new { photoUrl = relativeUrl });
    }
}
