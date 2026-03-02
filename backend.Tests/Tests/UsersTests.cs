using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using FlashcardsApi.Tests.Fixtures;
using FlashcardsApi.Tests.Helpers;
using Xunit;

namespace FlashcardsApi.Tests.Tests;

[Collection("Integration")]
public class UsersTests
{
    private readonly HttpClient _client;

    public UsersTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetMe_Returns200WithUserDto()
    {
        // Arrange
        var email = TestHelper.UniqueEmail();
        var displayName = "GetMeUser";
        var token = await TestHelper.GetTokenAsync(_client, email, displayName: displayName);

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/users/me", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await TestHelper.ReadAsync<UserDto>(response);
        user.Id.Should().BeOfType<string>().And.NotBeNullOrEmpty();
        user.Email.Should().BeOfType<string>().And.Be(email.ToLowerInvariant());
        user.DisplayName.Should().BeOfType<string>().And.Be(displayName);
        user.CreatedAt.Should().BeOfType<string>().And.NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetMe_IdIsLowercaseUuid()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/users/me", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await TestHelper.ReadAsync<UserDto>(response);
        TestHelper.AssertLowercaseUuid(user.Id);
    }

    [Fact]
    public async Task GetMe_CreatedAtIsIso8601()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/users/me", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await TestHelper.ReadAsync<UserDto>(response);
        TestHelper.AssertIso8601(user.CreatedAt);
    }

    [Fact]
    public async Task GetMe_ConsistentWithSignupResponse()
    {
        // Arrange
        var email = TestHelper.UniqueEmail();
        var displayName = "ConsistencyUser";
        var authResponse = await TestHelper.SignupAsync(_client, email, displayName: displayName);
        var signupUser = authResponse.User;

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/users/me", authResponse.Token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var meUser = await TestHelper.ReadAsync<UserDto>(response);
        meUser.Id.Should().Be(signupUser.Id);
        meUser.Email.Should().Be(signupUser.Email);
        meUser.DisplayName.Should().Be(signupUser.DisplayName);
    }

    [Fact]
    public async Task GetMe_NoToken_Returns401()
    {
        // Arrange — no authorization header
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        error.Error.Should().Be("unauthorized");
        error.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetMe_InvalidToken_Returns401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "garbage-token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        error.Error.Should().Be("unauthorized");
    }

    [Fact]
    public async Task GetMe_MalformedAuthHeader_Returns401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.TryAddWithoutValidation("Authorization", "NotBearer xxx");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
