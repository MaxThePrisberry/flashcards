using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FlashcardsApi.Tests.Helpers;

public static class TestHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static readonly Regex LowercaseUuidRegex = new(
        @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
        RegexOptions.Compiled);

    public static string UniqueEmail() => $"test-{Guid.NewGuid()}@example.com";

    /// <summary>
    /// Signs up a new user and returns the auth response.
    /// </summary>
    public static async Task<AuthResponseDto> SignupAsync(HttpClient client,
        string? email = null, string password = "Test1234!", string displayName = "TestUser")
    {
        email ??= UniqueEmail();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email, password, displayName })
        };
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions))!;
    }

    /// <summary>
    /// Signs up and returns just the bearer token.
    /// </summary>
    public static async Task<string> GetTokenAsync(HttpClient client,
        string? email = null, string password = "Test1234!", string displayName = "TestUser")
    {
        var auth = await SignupAsync(client, email, password, displayName);
        return auth.Token;
    }

    /// <summary>
    /// Creates a deck and returns the detail DTO.
    /// </summary>
    public static async Task<DeckDetailDto> CreateDeckAsync(HttpClient client, string token,
        string title = "Test Deck",
        string? description = "A test deck",
        List<object>? cards = null)
    {
        cards ??= new List<object>
        {
            new { term = "Term1", definition = "Def1" },
            new { term = "Term2", definition = "Def2" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/decks")
        {
            Content = JsonContent.Create(new { title, description, cards })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DeckDetailDto>(JsonOptions))!;
    }

    /// <summary>
    /// Creates an HttpRequestMessage with Bearer auth.
    /// </summary>
    public static HttpRequestMessage AuthRequest(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body != null)
            request.Content = JsonContent.Create(body);
        return request;
    }

    /// <summary>
    /// Deserializes JSON response body.
    /// </summary>
    public static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }

    /// <summary>
    /// Asserts that a string is a lowercase UUID.
    /// </summary>
    public static void AssertLowercaseUuid(string value)
    {
        if (!LowercaseUuidRegex.IsMatch(value))
            throw new Xunit.Sdk.XunitException(
                $"Expected lowercase UUID but got: \"{value}\"");
    }

    /// <summary>
    /// Asserts that a string is a valid ISO 8601 datetime with T separator.
    /// </summary>
    public static void AssertIso8601(string value)
    {
        if (!DateTime.TryParse(value, out _))
            throw new Xunit.Sdk.XunitException(
                $"Expected valid ISO 8601 datetime but got: \"{value}\"");
        if (!value.Contains('T'))
            throw new Xunit.Sdk.XunitException(
                $"Expected ISO 8601 with 'T' separator but got: \"{value}\"");
    }
}
