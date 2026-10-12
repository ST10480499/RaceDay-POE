namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int MaxParticipants { get; set; } = 100;
    public Event? Event { get; set; }
    public List<Enrolment> Enrolments { get; set; } = new();
}
