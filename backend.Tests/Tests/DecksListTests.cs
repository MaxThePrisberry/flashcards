using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using FlashcardsApi.Tests.Fixtures;
using FlashcardsApi.Tests.Helpers;
using Xunit;

namespace FlashcardsApi.Tests.Tests;

[Collection("Integration")]
public class DecksListTests
{
    private readonly HttpClient _client;

    public DecksListTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetDecks_EmptyList_ReturnsPaginatedResponse()
    {
        var token = await TestHelper.GetTokenAsync(_client);

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.Items.Should().BeEmpty();
        body.Page.Should().Be(1);
        body.PageSize.Should().Be(20);
        body.TotalCount.Should().Be(0);
        body.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetDecks_ReturnsCreatedDecks()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        await TestHelper.CreateDeckAsync(_client, token, "Deck One");
        await TestHelper.CreateDeckAsync(_client, token, "Deck Two");
        await TestHelper.CreateDeckAsync(_client, token, "Deck Three");

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.TotalCount.Should().Be(3);
        body.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetDecks_DeckSummaryShape()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        await TestHelper.CreateDeckAsync(_client, token, "Shape Deck");

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.Items.Should().NotBeEmpty();

        foreach (var item in body.Items)
        {
            TestHelper.AssertLowercaseUuid(item.Id);
            item.Title.Should().NotBeNullOrEmpty();
            item.Description.Should().NotBeNull();
            item.CardCount.Should().BeOfType(typeof(int));
            TestHelper.AssertIso8601(item.CreatedAt);
            TestHelper.AssertIso8601(item.UpdatedAt);
        }
    }

    [Fact]
    public async Task GetDecks_CardCountIsInteger()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        await TestHelper.CreateDeckAsync(_client, token, "Count Test", "desc",
            new List<object>
            {
                new { term = "A", definition = "1" },
                new { term = "B", definition = "2" },
                new { term = "C", definition = "3" }
            });

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        var deck = body.Items.Should().ContainSingle().Subject;
        deck.CardCount.Should().Be(3);
    }

    [Fact]
    public async Task GetDecks_OrderedByUpdatedAtDescending()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deckA = await TestHelper.CreateDeckAsync(_client, token, "Deck A");
        var deckB = await TestHelper.CreateDeckAsync(_client, token, "Deck B");

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.Items.Should().HaveCountGreaterOrEqualTo(2);
        body.Items[0].Id.Should().Be(deckB.Id);
        body.Items[1].Id.Should().Be(deckA.Id);
    }

    [Fact]
    public async Task GetDecks_DefaultPagination()
    {
        var token = await TestHelper.GetTokenAsync(_client);

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.Page.Should().Be(1);
        body.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetDecks_CustomPageAndPageSize()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        for (var i = 0; i < 5; i++)
        {
            await TestHelper.CreateDeckAsync(_client, token, $"Paginated {i}");
        }

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks?page=2&pageSize=2", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.Page.Should().Be(2);
        body.PageSize.Should().Be(2);
        body.TotalCount.Should().Be(5);
        body.TotalPages.Should().Be(3);
        body.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDecks_PageSizeClampedToMax100()
    {
        var token = await TestHelper.GetTokenAsync(_client);

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks?pageSize=200", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetDecks_OnlyReturnsOwnDecks()
    {
        var tokenA = await TestHelper.GetTokenAsync(_client);
        await TestHelper.CreateDeckAsync(_client, tokenA, "A's Deck");

        var tokenB = await TestHelper.GetTokenAsync(_client);

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", tokenB);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.Items.Should().BeEmpty();
        body.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetDecks_NoToken_Returns401()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/decks");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("unauthorized");
    }

    [Fact]
    public async Task GetDecks_IdsAreLowercaseUuids()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        await TestHelper.CreateDeckAsync(_client, token, "UUID Check");

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.Items.Should().NotBeEmpty();

        foreach (var item in body.Items)
        {
            TestHelper.AssertLowercaseUuid(item.Id);
        }
    }

    [Fact]
    public async Task GetDecks_TimestampsAreIso8601()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        await TestHelper.CreateDeckAsync(_client, token, "Timestamp Check");

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.Items.Should().NotBeEmpty();

        foreach (var item in body.Items)
        {
            TestHelper.AssertIso8601(item.CreatedAt);
            TestHelper.AssertIso8601(item.UpdatedAt);
        }
    }
}
