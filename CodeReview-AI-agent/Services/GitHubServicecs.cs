using System.Net.Http.Headers;

namespace CodeReviewAIAgent.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GitHubService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetPullRequestDiff(string? diffUrl)
        {
            if (string.IsNullOrEmpty(diffUrl))
                return "";

            return await _httpClient.GetStringAsync(diffUrl);
        }

        public async Task PostComment(string? repoFullName, int prNumber, string comment)
        {
            var token = _configuration["GitHub:Token"];

            var url = $"https://api.github.com/repos/{repoFullName}/issues/{prNumber}/comments";

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Headers.UserAgent.ParseAdd("CodeReviewAIAgent");

            request.Content = JsonContent.Create(new
            {
                body = comment
            });

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }
    }
}