using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using FlashcardsApi.Tests.Fixtures;
using FlashcardsApi.Tests.Helpers;
using Xunit;

namespace FlashcardsApi.Tests.Tests;

[Collection("Integration")]
public class DecksDeleteTests
{
    private readonly HttpClient _client;

    public DecksDeleteTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task DeleteDeck_Returns204NoContent()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Delete, $"/api/decks/{deck.Id}", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var contentLength = response.Content.Headers.ContentLength;
        var bodyText = await response.Content.ReadAsStringAsync();
        (contentLength is null or 0 || bodyText.Length == 0).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDeck_DeckNoLongerRetrievable()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var deleteRequest = TestHelper.AuthRequest(HttpMethod.Delete, $"/api/decks/{deck.Id}", token);
        await _client.SendAsync(deleteRequest);

        // Act
        var getRequest = TestHelper.AuthRequest(HttpMethod.Get, $"/api/decks/{deck.Id}", token);
        var response = await _client.SendAsync(getRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDeck_DeckDisappearsFromList()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        // Verify the deck is in the list
        var listRequest1 = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var listResponse1 = await _client.SendAsync(listRequest1);
        listResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

        var list1 = await TestHelper.ReadAsync<PaginatedDeckListDto>(listResponse1);
        list1.Items.Should().Contain(d => d.Id == deck.Id);

        // Delete the deck
        var deleteRequest = TestHelper.AuthRequest(HttpMethod.Delete, $"/api/decks/{deck.Id}", token);
        await _client.SendAsync(deleteRequest);

        // Act
        var listRequest2 = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var listResponse2 = await _client.SendAsync(listRequest2);
        listResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        var list2 = await TestHelper.ReadAsync<PaginatedDeckListDto>(listResponse2);

        // Assert
        list2.Items.Should().NotContain(d => d.Id == deck.Id);
    }

    [Fact]
    public async Task DeleteDeck_NonexistentId_Returns404()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var fakeId = Guid.NewGuid().ToString();

        var request = TestHelper.AuthRequest(HttpMethod.Delete, $"/api/decks/{fakeId}", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("not_found");
    }

    [Fact]
    public async Task DeleteDeck_OtherUsersDeck_Returns404()
    {
        // Arrange
        var tokenA = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, tokenA);

        var tokenB = await TestHelper.GetTokenAsync(_client);

        var request = TestHelper.AuthRequest(HttpMethod.Delete, $"/api/decks/{deck.Id}", tokenB);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDeck_AlreadyDeleted_Returns404()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var deleteRequest1 = TestHelper.AuthRequest(HttpMethod.Delete, $"/api/decks/{deck.Id}", token);
        await _client.SendAsync(deleteRequest1);

        // Act
        var deleteRequest2 = TestHelper.AuthRequest(HttpMethod.Delete, $"/api/decks/{deck.Id}", token);
        var response = await _client.SendAsync(deleteRequest2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDeck_NoToken_Returns401()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/decks/{deck.Id}");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
