using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using FlashcardsApi.Tests.Fixtures;
using FlashcardsApi.Tests.Helpers;
using Xunit;

namespace FlashcardsApi.Tests.Tests;

[Collection("Integration")]
public class AuthSignupTests
{
    private readonly HttpClient _client;

    public AuthSignupTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Signup_ValidRequest_Returns201WithAuthResponse()
    {
        var email = TestHelper.UniqueEmail();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email, password = "Test1234!", displayName = "Jane" })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await TestHelper.ReadAsync<AuthResponseDto>(response);

        body.Token.Should().NotBeNullOrEmpty();
        body.ExpiresIn.Should().BePositive();
        body.User.Should().NotBeNull();

        TestHelper.AssertLowercaseUuid(body.User.Id);
        body.User.Email.Should().Be(email.ToLowerInvariant());
        body.User.DisplayName.Should().Be("Jane");
        TestHelper.AssertIso8601(body.User.CreatedAt);
    }

    [Fact]
    public async Task Signup_EmailNormalizedToLowercase()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email = "Test@EXAMPLE.COM", password = "Test1234!", displayName = "Jane" })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await TestHelper.ReadAsync<AuthResponseDto>(response);
        body.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Signup_TokenIsUsable()
    {
        var auth = await TestHelper.SignupAsync(_client);

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/users/me", auth.Token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Signup_ExpiresInIsPositiveSeconds()
    {
        var auth = await TestHelper.SignupAsync(_client);

        auth.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task Signup_DuplicateEmail_Returns409Conflict()
    {
        var email = TestHelper.UniqueEmail();

        await TestHelper.SignupAsync(_client, email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email, password = "Test1234!", displayName = "Other" })
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("conflict");
        body.Message.Should().NotBeNullOrEmpty();
        body.Details.Should().BeNull();
    }

    [Fact]
    public async Task Signup_DuplicateEmailCaseInsensitive_Returns409()
    {
        var email = $"User-{Guid.NewGuid()}@Test.com";
        await TestHelper.SignupAsync(_client, email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email = email.ToLowerInvariant(), password = "Test1234!", displayName = "Other" })
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("conflict");
    }

    [Fact]
    public async Task Signup_MissingEmail_Returns400WithDetails()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { password = "Test1234!", displayName = "Jane" })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
        body.Message.Should().NotBeNullOrEmpty();
        body.Details.Should().ContainKey("email");
    }

    [Fact]
    public async Task Signup_InvalidEmailFormat_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email = "not-an-email", password = "Test1234!", displayName = "Jane" })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
        body.Details.Should().ContainKey("email");
    }

    [Fact]
    public async Task Signup_PasswordTooShort_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email = TestHelper.UniqueEmail(), password = "short", displayName = "Jane" })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
        body.Details.Should().ContainKey("password");
    }

    [Fact]
    public async Task Signup_MissingPassword_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email = TestHelper.UniqueEmail(), displayName = "Jane" })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
        body.Details.Should().ContainKey("password");
    }

    [Fact]
    public async Task Signup_MissingDisplayName_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email = TestHelper.UniqueEmail(), password = "Test1234!" })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
        body.Details.Should().ContainKey("displayName");
    }

    [Fact]
    public async Task Signup_DisplayNameTooLong_Returns400()
    {
        var longName = new string('A', 101);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { email = TestHelper.UniqueEmail(), password = "Test1234!", displayName = longName })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
        body.Details.Should().ContainKey("displayName");
    }

    [Fact]
    public async Task Signup_EmptyBody_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/signup")
        {
            Content = JsonContent.Create(new { })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }
}
