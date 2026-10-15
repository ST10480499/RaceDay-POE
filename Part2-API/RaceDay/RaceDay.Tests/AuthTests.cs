using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Tests;

public class AuthTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public AuthTests(RaceDayFactory factory)
    {
        _factory = factory;
    }

    // Registering with valid details works
    [Fact]
    public async Task Register_WithValidDetails_ReturnsCreated()
    {
        var client = TestHelpers.NewClient(_factory);

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Sipho Nkosi",
            email = $"{Guid.NewGuid():N}@test.com",
            password = "Password123",
            role = "Organiser"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // The same email cannot be registered twice
    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        var client = TestHelpers.NewClient(_factory);
        var body = new
        {
            fullName = "Sipho Nkosi",
            email = $"{Guid.NewGuid():N}@test.com",
            password = "Password123",
            role = "Participant"
        };

        await client.PostAsJsonAsync("/api/auth/register", body);
        var second = await client.PostAsJsonAsync("/api/auth/register", body);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    // Only Organiser and Participant are valid roles
    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        var client = TestHelpers.NewClient(_factory);

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Sipho Nkosi",
            email = $"{Guid.NewGuid():N}@test.com",
            password = "Password123",
            role = "Admin"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Logging in with the right password works
    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsOk()
    {
        var client = TestHelpers.NewClient(_factory);
        var email = $"{Guid.NewGuid():N}@test.com";

        await client.PostAsJsonAsync("/api/auth/register",
            new { fullName = "Sipho Nkosi", email, password = "Password123", role = "Participant" });
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Password123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Logging in with the wrong password is refused
    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = TestHelpers.NewClient(_factory);
        var email = $"{Guid.NewGuid():N}@test.com";

        await client.PostAsJsonAsync("/api/auth/register",
            new { fullName = "Sipho Nkosi", email, password = "Password123", role = "Participant" });
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongPassword" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // A logged-out user cannot see a profile
    [Fact]
    public async Task Profile_WhenLoggedOut_ReturnsUnauthorized()
    {
        var client = TestHelpers.NewClient(_factory);

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // A logged-in user can see their own profile
    [Fact]
    public async Task Profile_WhenLoggedIn_ReturnsOk()
    {
        var client = await TestHelpers.LoggedInClient(_factory, "Participant");

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}