using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// The details a new user sends when registering
public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    // Must be Organiser or Participant
    [Required]
    public string Role { get; set; } = string.Empty;
}

// The details a user sends when logging in
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

