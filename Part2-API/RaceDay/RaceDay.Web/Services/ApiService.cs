using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Services;

public class ApiResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Role { get; set; }
    public string? Name { get; set; }
}

public class ApiService
{
    private const string CookieKey = "ApiCookie";
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public ApiService(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    private ISession Session => _accessor.HttpContext!.Session;

    // Sends a request to the API, forwarding the saved API session cookie
    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (body != null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        var cookie = Session.GetString(CookieKey);
        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.Add("Cookie", cookie);
        }

        var response = await _http.SendAsync(request);

        // Save the API session cookie so later requests stay logged in
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            foreach (var value in values)
            {
                var pair = value.Split(';')[0];
                if (pair.StartsWith("RaceDay.Session=", StringComparison.OrdinalIgnoreCase))
                {
                    Session.SetString(CookieKey, pair);
                }
            }
        }

        return response;
    }

    public async Task<ApiResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await SendAsync(HttpMethod.Post, "api/auth/login",
                new { Email = email, Password = password });
            var text = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult { Success = false, Error = ReadError(text, "Invalid email or password.") };
            }

            var result = new ApiResult { Success = true };
            FillUserInfo(text, result);
            return result;
        }
        catch
        {
            return new ApiResult { Success = false, Error = "We could not reach the server. Please try again." };
        }
    }

    public async Task<ApiResult> RegisterAsync(string fullName, string email, string password, string role)
    {
        try
        {
            var response = await SendAsync(HttpMethod.Post, "api/auth/register",
                new { FullName = fullName, Email = email, Password = password, Role = role });
            var text = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult { Success = false, Error = ReadError(text, "Registration failed. Please check your details.") };
            }

            return new ApiResult { Success = true };
        }
        catch
        {
            return new ApiResult { Success = false, Error = "We could not reach the server. Please try again." };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await SendAsync(HttpMethod.Post, "api/auth/logout");
        }
        catch
        {
            // ignore, we clear the MVC session anyway
        }
        Session.Remove(CookieKey);
    }

    // ---------- helpers ----------

    private static string? Pick(JsonElement root, params string[] names)
    {
        if (root.ValueKind != JsonValueKind.Object) return null;
        foreach (var property in root.EnumerateObject())
        {
            foreach (var name in names)
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value.ToString();
                }
            }
        }
        return null;
    }

    private static void FillUserInfo(string json, ApiResult result)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            result.Role = Pick(doc.RootElement, "role");
            result.Name = Pick(doc.RootElement, "fullName", "email");
        }
        catch
        {
            // response was not JSON, nothing to read
        }
    }

    // Turns an API error into a friendly message (never raw errors)
    private static string ReadError(string json, string fallback)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var message = Pick(doc.RootElement, "message");
            if (!string.IsNullOrWhiteSpace(message)) return message;
        }
        catch
        {
            // not JSON
        }
        return fallback;
    }
}