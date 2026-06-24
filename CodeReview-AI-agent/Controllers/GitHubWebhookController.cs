using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CodeReviewAIAgent.Services;

namespace CodeReviewAIAgent.Controllers
{
    [ApiController]
    [Route("api/github")]
    public class GitHubWebhookController : ControllerBase
    {
        private readonly GitHubService _gitHubService;
        private readonly AIReviewService _aiReviewService;

        public GitHubWebhookController(
            GitHubService gitHubService,
            AIReviewService aiReviewService)
        {
            _gitHubService = gitHubService;
            _aiReviewService = aiReviewService;
        }
        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] JsonElement payload)
        {
            if (!payload.TryGetProperty("action", out JsonElement actionElement))
            {
                return Ok("Ignored non pull request event");
            }

            var action = actionElement.GetString();

            if (action != "opened" && action != "synchronize")
            {
                return Ok("Ignored PR event");
            }

            var repoFullName = payload
                .GetProperty("repository")
                .GetProperty("full_name")
                .GetString();

            var prNumber = payload
                .GetProperty("pull_request")
                .GetProperty("number")
                .GetInt32();

            var diffUrl = payload
                .GetProperty("pull_request")
                .GetProperty("diff_url")
                .GetString();

            var diff = await _gitHubService.GetPullRequestDiff(diffUrl);

            var review = await _aiReviewService.ReviewCode(diff);

            await _gitHubService.PostComment(repoFullName, prNumber, review);

            return Ok("PR webhook received successfully");
        }

    }
}