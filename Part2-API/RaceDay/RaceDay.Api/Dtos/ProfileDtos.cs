using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// The details a user can change on their own profile
public class UpdateProfileRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;
}