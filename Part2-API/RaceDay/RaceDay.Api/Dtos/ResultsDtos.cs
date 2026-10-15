using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

// The details an organiser sends to record a result
public class ResultRequest
{
    // Finish time written as hours:minutes:seconds, for example 01:45:30
    [Required]
    public string FinishTime { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Position { get; set; }
}

// What the API sends back for a result
public class ResultResponse
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