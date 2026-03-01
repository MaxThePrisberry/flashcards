using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using FlashcardsApi.Tests.Fixtures;
using FlashcardsApi.Tests.Helpers;
using Xunit;

namespace FlashcardsApi.Tests.Tests;

[Collection("Integration")]
public class DecksUpdateTests
{
    private readonly HttpClient _client;

    public DecksUpdateTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task UpdateDeck_ValidRequest_Returns200()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Updated Title",
            description = "Updated Description",
            cards = new[] { new { term = "NewTerm", definition = "NewDef" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        body.Title.Should().Be("Updated Title");
        body.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateDeck_ReplacesAllCards()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, cards: new List<object>
        {
            new { term = "A", definition = "DefA" },
            new { term = "B", definition = "DefB" }
        });

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = deck.Title,
            description = deck.Description,
            cards = new[]
            {
                new { term = "C", definition = "DefC" },
                new { term = "D", definition = "DefD" },
                new { term = "E", definition = "DefE" }
            }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        body.Cards.Should().HaveCount(3);
        body.Cards.Select(c => c.Term).Should().ContainInOrder("C", "D", "E");
        body.Cards.Select(c => c.Definition).Should().ContainInOrder("DefC", "DefD", "DefE");

        // Confirm via GET
        var getRequest = TestHelper.AuthRequest(HttpMethod.Get, $"/api/decks/{deck.Id}", token);
        var getResponse = await _client.SendAsync(getRequest);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await TestHelper.ReadAsync<DeckDetailDto>(getResponse);
        fetched.Cards.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdateDeck_NewCardsGetNewIds()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);
        var originalIds = deck.Cards.Select(c => c.Id).ToList();

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = deck.Title,
            description = deck.Description,
            cards = new[]
            {
                new { term = "X", definition = "DefX" },
                new { term = "Y", definition = "DefY" }
            }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        var newIds = body.Cards.Select(c => c.Id).ToList();

        newIds.Should().NotIntersectWith(originalIds);
    }

    [Fact]
    public async Task UpdateDeck_CardPositionsZeroIndexed()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = deck.Title,
            description = deck.Description,
            cards = new[]
            {
                new { term = "First", definition = "D1" },
                new { term = "Second", definition = "D2" },
                new { term = "Third", definition = "D3" }
            }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        body.Cards.Select(c => c.Position).Should().ContainInOrder(0, 1, 2);
    }

    [Fact]
    public async Task UpdateDeck_UpdatesTimestamp()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);
        var createdAt = DateTime.Parse(deck.CreatedAt);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Timestamp Check",
            description = deck.Description,
            cards = new[] { new { term = "T", definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        var updatedAt = DateTime.Parse(body.UpdatedAt);
        updatedAt.Should().BeOnOrAfter(createdAt);
    }

    [Fact]
    public async Task UpdateDeck_EmptyDescriptionIsValid()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = deck.Title,
            description = "",
            cards = new[] { new { term = "T", definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<DeckDetailDto>(response);
        body.Description.Should().Be("");
    }

    [Fact]
    public async Task UpdateDeck_NonexistentDeck_Returns404()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var fakeId = Guid.NewGuid().ToString();

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{fakeId}", token, new
        {
            title = "Ghost",
            description = "Does not exist",
            cards = new[] { new { term = "T", definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("not_found");
    }

    [Fact]
    public async Task UpdateDeck_OtherUsersDeck_Returns404()
    {
        // Arrange
        var tokenA = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, tokenA);

        var tokenB = await TestHelper.GetTokenAsync(_client);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", tokenB, new
        {
            title = "Hijacked",
            description = "Not yours",
            cards = new[] { new { term = "T", definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDeck_MissingTitle_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            description = "No title",
            cards = new[] { new { term = "T", definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task UpdateDeck_TitleTooLong_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);
        var longTitle = new string('A', 201);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = longTitle,
            description = "Valid",
            cards = new[] { new { term = "T", definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task UpdateDeck_DescriptionTooLong_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);
        var longDesc = new string('B', 1001);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Valid Title",
            description = longDesc,
            cards = new[] { new { term = "T", definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task UpdateDeck_EmptyCards_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Valid Title",
            description = "Valid",
            cards = Array.Empty<object>()
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task UpdateDeck_NoToken_Returns401()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/decks/{deck.Id}")
        {
            Content = JsonContent.Create(new
            {
                title = "No Auth",
                description = "Should fail",
                cards = new[] { new { term = "T", definition = "D" } }
            })
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateDeck_MissingDescription_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Valid Title",
            cards = new[] { new { term = "T", definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task UpdateDeck_MissingCards_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Valid Title",
            description = "Valid"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task UpdateDeck_CardMissingTerm_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Valid Title",
            description = "Valid",
            cards = new[] { new { definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task UpdateDeck_CardMissingDefinition_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Valid Title",
            description = "Valid",
            cards = new[] { new { term = "T" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task UpdateDeck_CardTermTooLong_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);
        var longTerm = new string('A', 501);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Valid Title",
            description = "Valid",
            cards = new[] { new { term = longTerm, definition = "D" } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }

    [Fact]
    public async Task UpdateDeck_CardDefinitionTooLong_Returns400()
    {
        // Arrange
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token);
        var longDef = new string('A', 2001);

        var request = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Valid Title",
            description = "Valid",
            cards = new[] { new { term = "T", definition = longDef } }
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("validation_error");
    }
}
