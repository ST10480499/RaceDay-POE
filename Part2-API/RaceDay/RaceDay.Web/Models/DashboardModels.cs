namespace RaceDay.Web.Models;

public class DashboardRow
{
    public EventModel Event { get; set; } = new();
    public int EnrolmentCount { get; set; }
}

public class OrganiserDashboardViewModel
{
    public List<DashboardRow> Rows { get; set; } = new();
    public string? Error { get; set; }

    public int TotalEvents => Rows.Count;
    public int TotalEnrolments => Rows.Sum(r => r.EnrolmentCount);
    public int UpcomingEvents => Rows.Count(r => r.Event.EventDate >= DateTime.Today);
}