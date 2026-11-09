using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// ---------- Shapes the Part 2 API sends back ----------

public class CategoryModel
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
}

public class EventModel
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public List<CategoryModel> Categories { get; set; } = new();
}

public class EnrolmentModel
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantEmail { get; set; } = string.Empty;
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrolmentDate { get; set; }
}

public class ResultModel
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string FinishTime { get; set; } = string.Empty;
    public int Position { get; set; }
}

// ---------- Forms the user fills in ----------

public class EventFormModel
{
    public int EventID { get; set; }

    [Required(ErrorMessage = "Please enter the event name.")]
    [MaxLength(150, ErrorMessage = "Event name must be 150 characters or less.")]
    [Display(Name = "Event name")]
    public string EventName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a description.")]
    [MaxLength(1000, ErrorMessage = "Description must be 1000 characters or less.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose the event date.")]
    [Display(Name = "Event date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

    [Required(ErrorMessage = "Please enter the location.")]
    [MaxLength(200, ErrorMessage = "Location must be 200 characters or less.")]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal Distance { get; set; } = 5;

    [Required(ErrorMessage = "Please choose an event type.")]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = "run";
}

public class CategoryFormModel
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }

    [Required(ErrorMessage = "Please enter the category name.")]
    [MaxLength(50, ErrorMessage = "Category name must be 50 characters or less.")]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;

    [Range(1, 100000, ErrorMessage = "Maximum participants must be between 1 and 100000.")]
    [Display(Name = "Maximum participants")]
    public int MaxParticipants { get; set; } = 100;
}