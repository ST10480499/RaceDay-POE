using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RaceDay.Tests;

// Small helpers that every test class uses
public static class TestHelpers
{
    // A client that remembers the login cookie, like a browser
    public static HttpClient NewClient(RaceDayFactory factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    // Registers a brand new user with the given role and logs them in
    public static async Task<HttpClient> LoggedInClient(RaceDayFactory factory, string role)
    {
        var client = NewClient(factory);
        var email = $"{Guid.NewGuid():N}@test.com";

        await client.PostAsJsonAsync("/api/auth/register",
            new { fullName = "Test User", email, password = "Password123", role });
        await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Password123" });

        return client;
    }

    public static object NewEventBody()
    {
        return new
        {
            eventName = "Test Race",
            description = "A race used for testing.",
            eventDate = "2026-12-06T06:00:00",
            location = "Johannesburg",
            distance = 10,
            eventType = "run"
        };
    }

    // Reads a number such as eventID out of a JSON response
    public static async Task<int> GetId(HttpResponseMessage response, string property)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty(property).GetInt32();
    }

    // Creates an event with one category as the given organiser
    public static async Task<(int EventId, int CategoryId)> CreateEventWithCategory(HttpClient organiser)
    {
        var eventResponse = await organiser.PostAsJsonAsync("/api/events", NewEventBody());
        var eventId = await GetId(eventResponse, "eventID");

        var categoryResponse = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories",
            new { categoryName = "10km", maxParticipants = 50 });
        var categoryId = await GetId(categoryResponse, "categoryID");

        return (eventId, categoryId);
    }
}