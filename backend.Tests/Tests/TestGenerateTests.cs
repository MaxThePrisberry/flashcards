using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using FlashcardsApi.Tests.Fixtures;
using FlashcardsApi.Tests.Helpers;
using Xunit;

namespace FlashcardsApi.Tests.Tests;

[Collection("Integration")]
public class TestGenerateTests
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public TestGenerateTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
        // Reset fake service state before each test
        fixture.Factory.FakeOpenAi.ShouldFail = false;
    }

    [Fact]
    public async Task GenerateTest_ValidDeck_Returns200WithCorrectShape()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Test Quiz Deck", "desc",
            new List<object>
            {
                new { term = "Hola", definition = "Hello" },
                new { term = "Gracias", definition = "Thank you" },
                new { term = "Adiós", definition = "Goodbye" }
            });

        var request = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<TestResponseDto>(response);
        TestHelper.AssertLowercaseUuid(body.DeckId);
        body.DeckId.Should().Be(deck.Id);
        body.DeckTitle.Should().Be("Test Quiz Deck");
        body.QuestionCount.Should().Be(3);
        body.Questions.Should().HaveCount(3);
    }

    [Fact]
    public async Task GenerateTest_QuestionsHaveCorrectStructure()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Structure Test", "desc",
            new List<object>
            {
                new { term = "Cat", definition = "A small domesticated carnivorous mammal" },
                new { term = "Dog", definition = "A domesticated canine" }
            });

        var request = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token);
        var response = await _client.SendAsync(request);
        var body = await TestHelper.ReadAsync<TestResponseDto>(response);

        foreach (var question in body.Questions)
        {
            TestHelper.AssertLowercaseUuid(question.CardId);
            question.Direction.Should().BeOneOf("term_to_definition", "definition_to_term");
            question.Prompt.Should().NotBeNullOrEmpty();
            question.Options.Should().HaveCountGreaterOrEqualTo(2);
            question.Options.Should().HaveCountLessOrEqualTo(4);
            question.CorrectOptionIndex.Should().BeGreaterOrEqualTo(0);
            question.CorrectOptionIndex.Should().BeLessThan(question.Options.Count);

            // Verify correct option contains valid text
            var correctOption = question.Options[question.CorrectOptionIndex];
            correctOption.Text.Should().NotBeNullOrEmpty();

            // Verify option indices are sequential
            for (int i = 0; i < question.Options.Count; i++)
            {
                question.Options[i].Index.Should().Be(i);
            }
        }
    }

    [Fact]
    public async Task GenerateTest_CorrectAnswerIsActualCardContent()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Answer Verify", "desc",
            new List<object>
            {
                new { term = "Sun", definition = "The star at the center of the solar system" }
            });

        var request = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token);
        var response = await _client.SendAsync(request);
        var body = await TestHelper.ReadAsync<TestResponseDto>(response);

        body.Questions.Should().HaveCount(1);
        var question = body.Questions[0];

        var correctText = question.Options[question.CorrectOptionIndex].Text;

        if (question.Direction == "term_to_definition")
        {
            question.Prompt.Should().Be("Sun");
            correctText.Should().Be("The star at the center of the solar system");
        }
        else
        {
            question.Prompt.Should().Be("The star at the center of the solar system");
            correctText.Should().Be("Sun");
        }
    }

    [Fact]
    public async Task GenerateTest_CachedDistractors_NoLlmCallNeeded()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Cache Test", "desc",
            new List<object>
            {
                new { term = "A", definition = "1" }
            });

        // First call generates and caches
        var request1 = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token);
        var response1 = await _client.SendAsync(request1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Now make the LLM fail — second call should still work from cache
        _fixture.Factory.FakeOpenAi.ShouldFail = true;

        var request2 = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token);
        var response2 = await _client.SendAsync(request2);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<TestResponseDto>(response2);
        body.Questions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateTest_AfterDeckEdit_RegeneratesDistractors()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Edit Cache Test", "desc",
            new List<object>
            {
                new { term = "X", definition = "Y" }
            });

        // Generate test to populate cache
        var request1 = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token);
        var response1 = await _client.SendAsync(request1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Edit the deck (full replacement — clears Pairs and cascade-deletes distractors)
        var editRequest = TestHelper.AuthRequest(HttpMethod.Put, $"/api/decks/{deck.Id}", token, new
        {
            title = "Edit Cache Test Updated",
            description = "desc",
            cards = new[] { new { term = "NewTerm", definition = "NewDef" } }
        });
        var editResponse = await _client.SendAsync(editRequest);
        editResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Make LLM fail — this should fail because cache was cleared
        _fixture.Factory.FakeOpenAi.ShouldFail = true;

        var request2 = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token);
        var response2 = await _client.SendAsync(request2);
        response2.StatusCode.Should().Be((HttpStatusCode)502);
    }

    [Fact]
    public async Task GenerateTest_Unauthenticated_Returns401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/decks/00000000-0000-0000-0000-000000000001/test");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateTest_NonexistentDeck_Returns404()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var fakeId = Guid.NewGuid();
        var request = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{fakeId}/test", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("not_found");
    }

    [Fact]
    public async Task GenerateTest_OtherUsersDeck_Returns404()
    {
        var token1 = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token1, "Private Deck", "desc");

        var token2 = await TestHelper.GetTokenAsync(_client);
        var request = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token2);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GenerateTest_LlmFailure_Returns502()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "LLM Fail Test", "desc",
            new List<object>
            {
                new { term = "Fail", definition = "Test" }
            });

        _fixture.Factory.FakeOpenAi.ShouldFail = true;

        var request = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be((HttpStatusCode)502);

        var body = await TestHelper.ReadAsync<ErrorResponseDto>(response);
        body.Error.Should().Be("llm_unavailable");
    }

    [Fact]
    public async Task GenerateTest_SingleCardDeck_Works()
    {
        var token = await TestHelper.GetTokenAsync(_client);
        var deck = await TestHelper.CreateDeckAsync(_client, token, "Single Card", "desc",
            new List<object>
            {
                new { term = "Only", definition = "One" }
            });

        var request = TestHelper.AuthRequest(HttpMethod.Post, $"/api/decks/{deck.Id}/test", token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await TestHelper.ReadAsync<TestResponseDto>(response);
        body.QuestionCount.Should().Be(1);
        body.Questions.Should().HaveCount(1);
        body.Questions[0].Options.Should().HaveCount(4);
    }
}
