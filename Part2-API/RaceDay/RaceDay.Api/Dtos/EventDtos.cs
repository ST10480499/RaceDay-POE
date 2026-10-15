using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// The details an organiser sends to create or update an event
public class EventRequest
{
    [Required]
    [MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    // Distance in kilometres
    [Range(0.1, 1000)]
    public decimal Distance { get; set; }

    // Must be run, walk or cycle
    [Required]
    public string EventType { get; set; } = string.Empty;
}

// A short view of a category, shown inside an event
public class CategorySummary
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
}

// What the API sends back when someone views an event
public class EventResponse
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
    public List<CategorySummary> Categories { get; set; } = new();
}