using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using FlashcardsApi.Tests.Fixtures;
using FlashcardsApi.Tests.Helpers;
using Xunit;

namespace FlashcardsApi.Tests.Tests;

[Collection("Integration")]
public class DecksCreateTests
{
    private readonly HttpClient _client;

    public DecksCreateTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task CreateDeck_ValidRequest_Returns201()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = "Spanish",
            description = "Vocab",
            cards = new[] { new { term = "Hola", definition = "Hello" } }
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        TestHelper.AssertLowercaseUuid(body.Id);
        body.Title.Should().Be("Spanish");
        body.Description.Should().Be("Vocab");
        body.Cards.Should().HaveCount(1);
        TestHelper.AssertIso8601(body.CreatedAt);
        TestHelper.AssertIso8601(body.UpdatedAt);
    }

    [Fact]
    public async Task CreateDeck_CardsHaveCorrectShape()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Shape Test", "desc",
            new List<object>
            {
                new { term = "A", definition = "1" },
                new { term = "B", definition = "2" }
            });

        deck.Cards.Should().HaveCount(2);

        foreach (var card in deck.Cards)
        {
            TestHelper.AssertLowercaseUuid(card.Id);
            card.Term.Should().NotBeNullOrEmpty();
            card.Definition.Should().NotBeNullOrEmpty();
        }

        deck.Cards[0].Position.Should().Be(0);
        deck.Cards[1].Position.Should().Be(1);
    }

    [Fact]
    public async Task CreateDeck_NullDescriptionDefaultsToEmptyString()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = "No Desc",
            cards = new[] { new { term = "T", definition = "D" } }
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        body.Description.Should().Be("");
    }

    [Fact]
    public async Task CreateDeck_MultipleCards_PositionsAreZeroIndexed()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var cards = new List<object>
        {
            new { term = "A", definition = "1" },
            new { term = "B", definition = "2" },
            new { term = "C", definition = "3" },
            new { term = "D", definition = "4" },
            new { term = "E", definition = "5" }
        };

        var deck = await TestHelper.CreateDeckAsync(_client, token, "Five Cards", "desc", cards);

        deck.Cards.Should().HaveCount(5);
        deck.Cards.Select(c => c.Position).Should().BeEquivalentTo(new[] { 0, 1, 2, 3, 4 });
    }

    [Fact]
    public async Task CreateDeck_TermAndDefinitionPreserved()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Preservation", "desc",
            new List<object>
            {
                new { term = "Bonjour", definition = "Good morning" },
                new { term = "Merci", definition = "Thank you" }
            });

        deck.Cards.Should().Contain(c => c.Term == "Bonjour" && c.Definition == "Good morning");
        deck.Cards.Should().Contain(c => c.Term == "Merci" && c.Definition == "Thank you");
    }

    [Fact]
    public async Task CreateDeck_AppearsInDeckList()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "List Visible");

        var request = TestHelper.AuthRequest(HttpMethod.Get, "/api/decks", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<PaginatedDeckListDto>(response);
        body.Items.Should().Contain(d => d.Id == deck.Id);
    }

    [Fact]
    public async Task CreateDeck_GetByIdReturnsMatchingData()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Match Check", "matching desc",
            new List<object>
            {
                new { term = "X", definition = "Y" },
                new { term = "P", definition = "Q" }
            });

        var request = TestHelper.AuthRequest(HttpMethod.Get, $"/api/decks/{deck.Id}", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await TestHelper.ReadAsync<DeckDetailDto>(response);
        fetched.Title.Should().Be("Match Check");
        fetched.Description.Should().Be("matching desc");
        fetched.Cards.Should().HaveCount(2);
        fetched.Cards.Should().Contain(c => c.Term == "X" && c.Definition == "Y");
        fetched.Cards.Should().Contain(c => c.Term == "P" && c.Definition == "Q");
    }

    [Fact]
    public async Task CreateDeck_MissingTitle_Returns400()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            description = "No title here",
            cards = new[] { new { term = "T", definition = "D" } }
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
        body.Details.Should().ContainKey("title");
    }

    [Fact]
    public async Task CreateDeck_TitleTooLong_Returns400()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var longTitle = new string('A', 201);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = longTitle,
            cards = new[] { new { term = "T", definition = "D" } }
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDeck_DescriptionTooLong_Returns400()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var longDesc = new string('A', 1001);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = "Valid Title",
            description = longDesc,
            cards = new[] { new { term = "T", definition = "D" } }
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDeck_EmptyCardsArray_Returns400()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = "Empty Cards",
            cards = Array.Empty<object>()
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task CreateDeck_MissingCards_Returns400()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = "No Cards Field"
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDeck_CardMissingTerm_Returns400()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = "Bad Card",
            cards = new[] { new { definition = "D" } }
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDeck_CardMissingDefinition_Returns400()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = "Bad Card",
            cards = new[] { new { term = "T" } }
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDeck_CardTermTooLong_Returns400()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var longTerm = new string('A', 501);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = "Long Term",
            cards = new[] { new { term = longTerm, definition = "D" } }
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDeck_CardDefinitionTooLong_Returns400()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var longDef = new string('A', 2001);
        var request = TestHelper.AuthRequest(HttpMethod.Post, "/api/decks", token, new
        {
            title = "Long Def",
            cards = new[] { new { term = "T", definition = longDef } }
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDeck_NoToken_Returns401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/decks")
        {
            Content = JsonContent.Create(new
            {
                title = "Unauthorized",
                cards = new[] { new { term = "T", definition = "D" } }
            })
        };

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("unauthorized");
    }
}
