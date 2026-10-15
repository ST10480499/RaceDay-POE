using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Tests;

public class EventTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public EventTests(RaceDayFactory factory)
    {
        _factory = factory;
    }

    // An organiser can create an event
    [Fact]
    public async Task Organiser_CanCreateEvent()
    {
        var organiser = await TestHelpers.LoggedInClient(_factory, "Organiser");

        var response = await organiser.PostAsJsonAsync("/api/events", TestHelpers.NewEventBody());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // A participant is rejected when trying to create an event
    [Fact]
    public async Task Participant_CannotCreateEvent()
    {
        var participant = await TestHelpers.LoggedInClient(_factory, "Participant");

        var response = await participant.PostAsJsonAsync("/api/events", TestHelpers.NewEventBody());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Someone who is not logged in is rejected
    [Fact]
    public async Task LoggedOutUser_CannotCreateEvent()
    {
        var client = TestHelpers.NewClient(_factory);

        var response = await client.PostAsJsonAsync("/api/events", TestHelpers.NewEventBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // The event type must be run, walk or cycle
    [Fact]
    public async Task CreateEvent_WithInvalidType_ReturnsBadRequest()
    {
        var organiser = await TestHelpers.LoggedInClient(_factory, "Organiser");

        var response = await organiser.PostAsJsonAsync("/api/events", new
        {
            eventName = "Test Race",
            description = "A race used for testing.",
            eventDate = "2026-12-06T06:00:00",
            location = "Johannesburg",
            distance = 10,
            eventType = "swim"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Both roles can view events
    [Fact]
    public async Task Participant_CanViewEvents()
    {
        var participant = await TestHelpers.LoggedInClient(_factory, "Participant");

        var response = await participant.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // An organiser can update their own event
    [Fact]
    public async Task Organiser_CanUpdateOwnEvent()
    {
        var organiser = await TestHelpers.LoggedInClient(_factory, "Organiser");
        var created = await organiser.PostAsJsonAsync("/api/events", TestHelpers.NewEventBody());
        var eventId = await TestHelpers.GetId(created, "eventID");

        var response = await organiser.PutAsJsonAsync($"/api/events/{eventId}", new
        {
            eventName = "Renamed Race",
            description = "An updated description.",
            eventDate = "2026-12-07T06:00:00",
            location = "Pretoria",
            distance = 21,
            eventType = "cycle"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // An organiser cannot change another organiser's event
    [Fact]
    public async Task Organiser_CannotUpdateAnotherOrganisersEvent()
    {
        var owner = await TestHelpers.LoggedInClient(_factory, "Organiser");
        var otherOrganiser = await TestHelpers.LoggedInClient(_factory, "Organiser");
        var created = await owner.PostAsJsonAsync("/api/events", TestHelpers.NewEventBody());
        var eventId = await TestHelpers.GetId(created, "eventID");

        var response = await otherOrganiser.PutAsJsonAsync($"/api/events/{eventId}", TestHelpers.NewEventBody());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // An organiser can delete their own event, and it is gone afterwards
    [Fact]
    public async Task Organiser_CanDeleteOwnEvent()
    {
        var organiser = await TestHelpers.LoggedInClient(_factory, "Organiser");
        var created = await organiser.PostAsJsonAsync("/api/events", TestHelpers.NewEventBody());
        var eventId = await TestHelpers.GetId(created, "eventID");

        var deleted = await organiser.DeleteAsync($"/api/events/{eventId}");
        var afterwards = await organiser.GetAsync($"/api/events/{eventId}");

        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, afterwards.StatusCode);
    }

    // A participant cannot delete events
    [Fact]
    public async Task Participant_CannotDeleteEvent()
    {
        var organiser = await TestHelpers.LoggedInClient(_factory, "Organiser");
        var participant = await TestHelpers.LoggedInClient(_factory, "Participant");
        var created = await organiser.PostAsJsonAsync("/api/events", TestHelpers.NewEventBody());
        var eventId = await TestHelpers.GetId(created, "eventID");

        var response = await participant.DeleteAsync($"/api/events/{eventId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}