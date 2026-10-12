namespace RaceDay.Api.Models;

public class User
{
    public int UserID { get; set; }
    public int RoleID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public Role? Role { get; set; }
}