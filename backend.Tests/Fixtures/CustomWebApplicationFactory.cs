using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Flashcards.APIs.Services.OpenAi;

namespace FlashcardsApi.Tests.Fixtures;

public class FakeOpenAiService : IOpenAiService
{
    public bool ShouldFail { get; set; }

    public Task<List<CardDistractors>> GenerateDistractorsAsync(List<CardInfo> cards)
    {
        if (ShouldFail)
            throw new Flashcards.APIs.Exceptions.LlmUnavailableException("Fake LLM failure for testing.");

        var result = cards.Select(c => new CardDistractors(
            c.Index,
            new List<string> { $"FakeDef1_{c.Index}", $"FakeDef2_{c.Index}", $"FakeDef3_{c.Index}" },
            new List<string> { $"FakeTerm1_{c.Index}", $"FakeTerm2_{c.Index}", $"FakeTerm3_{c.Index}" }
        )).ToList();

        return Task.FromResult(result);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeOpenAiService FakeOpenAi { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Use the test database connection string from environment variable
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=flashcards_test;Username=flashcards;Password=flashcards";

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Replace OpenAI service with fake for testing
            var openAiDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IOpenAiService));
            if (openAiDescriptor != null)
                services.Remove(openAiDescriptor);

            services.AddSingleton<IOpenAiService>(FakeOpenAi);
        });

        builder.UseEnvironment("Development");
    }
}
