using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public class EventResultsViewModel
{
    public EventModel Event { get; set; } = new();
    public List<EnrolmentModel> Enrolments { get; set; } = new();
    public List<ResultModel> Results { get; set; } = new();
}

public class ResultRow
{
    public ResultModel Result { get; set; } = new();
    public DateTime EventDate { get; set; }
    public int Finishers { get; set; }
}

public class ProfileFormModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [MaxLength(100, ErrorMessage = "Name must be 100 characters or less.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email.")]
    [MaxLength(150, ErrorMessage = "Email must be 150 characters or less.")]
    public string Email { get; set; } = string.Empty;
}
