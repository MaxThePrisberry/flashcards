using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using FlashcardsApi.Tests.Fixtures;
using FlashcardsApi.Tests.Helpers;
using Xunit;

namespace FlashcardsApi.Tests.Tests;

[Collection("Integration")]
public class AuthLoginTests
{
    private readonly HttpClient _client;

    public AuthLoginTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200()
    {
        var email = TestHelper.UniqueEmail();
        var password = "Test1234!";
        var displayName = "LoginUser";

        var signup = await TestHelper.SignupAsync(_client, email, password, displayName);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email, password })
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<AuthResponseDto>(response);

        body.Token.Should().NotBeNullOrEmpty();
        body.ExpiresIn.Should().BePositive();
        body.User.Should().NotBeNull();

        body.User.Email.Should().Be(email.ToLowerInvariant());
        body.User.DisplayName.Should().Be(displayName);
        body.User.Id.Should().Be(signup.User.Id);
    }

    [Fact]
    public async Task Login_TokenIsUsable()
    {
        var email = TestHelper.UniqueEmail();
        var password = "Test1234!";
        await TestHelper.SignupAsync(_client, email, password);

        var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email, password })
        };
        var loginResponse = await _client.SendAsync(loginRequest);
        var auth = await TestHelper.ReadAsync<AuthResponseDto>(loginResponse);

        var meRequest = TestHelper.AuthRequest(HttpMethod.Get, "/api/users/me", auth.Token);
        var meResponse = await _client.SendAsync(meRequest);

        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_EmailCaseInsensitive()
    {
        var email = $"user-{Guid.NewGuid()}@test.com";
        var password = "Test1234!";
        await TestHelper.SignupAsync(_client, email, password);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email = email.ToUpperInvariant(), password })
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = TestHelper.UniqueEmail();
        await TestHelper.SignupAsync(_client, email);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "WrongPassword99!" })
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("unauthorized");
        body.Message.Should().NotBeNullOrEmpty();
        body.Details.Should().BeNull();
    }

    [Fact]
    public async Task Login_NonexistentEmail_Returns401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email = TestHelper.UniqueEmail(), password = "Test1234!" })
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("unauthorized");
    }

    [Fact]
    public async Task Login_MissingEmail_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { password = "Test1234!" })
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task Login_MissingPassword_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email = "x@y.com" })
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task Login_EmptyBody_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { })
        };
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
