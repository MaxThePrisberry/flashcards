using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using FlashcardsApi.Tests.Fixtures;
using FlashcardsApi.Tests.Helpers;
using Xunit;

namespace FlashcardsApi.Tests.Tests;

[Collection("Integration")]
public class DecksGetTests
{
    private readonly HttpClient _client;

    public DecksGetTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetDeck_Returns200WithFullDetail()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Detail Test", "full detail",
            new List<object>
            {
                new { term = "A", definition = "1" },
                new { term = "B", definition = "2" }
            });

        var request = TestHelper.AuthRequest(HttpMethod.Get, $"/api/decks/{deck.Id}", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        TestHelper.AssertLowercaseUuid(body.Id);
        body.Title.Should().Be("Detail Test");
        body.Description.Should().Be("full detail");
        TestHelper.AssertIso8601(body.CreatedAt);
        TestHelper.AssertIso8601(body.UpdatedAt);

        body.Cards.Should().HaveCount(2);
        foreach (var card in body.Cards)
        {
            TestHelper.AssertLowercaseUuid(card.Id);
            card.Term.Should().NotBeNullOrEmpty();
            card.Definition.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetDeck_CardsOrderedByPosition()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Order Test", "ordering",
            new List<object>
            {
                new { term = "First", definition = "1" },
                new { term = "Second", definition = "2" },
                new { term = "Third", definition = "3" }
            });

        var request = TestHelper.AuthRequest(HttpMethod.Get, $"/api/decks/{deck.Id}", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        body.Cards.Should().HaveCount(3);
        body.Cards[0].Position.Should().BeLessThan(body.Cards[1].Position);
        body.Cards[1].Position.Should().BeLessThan(body.Cards[2].Position);
    }

    [Fact]
    public async Task GetDeck_NonexistentId_Returns404()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var randomId = Guid.NewGuid().ToString();

        var request = TestHelper.AuthRequest(HttpMethod.Get, $"/api/decks/{randomId}", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("not_found");
        body.Message.Should().NotBeNullOrEmpty();
        body.Details.Should().BeNull();
    }

    [Fact]
    public async Task GetDeck_OtherUsersDeck_Returns404NotForbidden()
    {
        var tokenA = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, tokenA, "Private Deck");

        var tokenB = await TestHelper.GetTokenAsync(_client);

        var request = TestHelper.AuthRequest(HttpMethod.Get, $"/api/decks/{deck.Id}", tokenB);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDeck_NoToken_Returns401()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Auth Required");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/decks/{deck.Id}");
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDeck_InvalidUuidFormat_ReturnsError()
    {
        var token = await TestHelper.GetTokenAsync(_client);

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks/not-a-uuid", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }
}
