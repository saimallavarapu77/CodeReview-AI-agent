using System.Text;
using System.Text.Json;

namespace CodeReviewAIAgent.Services;

public class AIReviewService
{
    private readonly HttpClient _httpClient;

    public AIReviewService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ReviewCode(string diff)
    {
        var prompt = $"""
        You are a senior .NET code reviewer.

        Review this pull request diff.

        Find:
        - Security Issues
        - Bugs
        - Code Smells
        - Performance Problems

        Diff:

        {diff}
        """;

        var body = new
        {
            model = "llama3.2:3b",
            prompt = prompt,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            body);

        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);

        return doc.RootElement
                  .GetProperty("response")
                  .GetString()!;
    }
}