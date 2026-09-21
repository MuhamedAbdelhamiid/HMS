using HMS.Services.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HMS.Infrastructure.ExternalServices
{
    public class AiModerationService : IAiModerationService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiModerationService> _logger;

        public AiModerationService(IConfiguration configuration, HttpClient httpClient, ILogger<AiModerationService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<bool> IsUserCommentAcceptedAsFeedbackAsync(string comment)
        {
            var prompt = _configuration["OpenAiSettings:ModerationPrompt"];
            var apiKey = _configuration["OpenAiSettings:ApiKey"];
            var baseUrl = _configuration["OpenAiSettings:BaseUrl"];
            var targetModel = _configuration["OpenAiSettings:TargetModel"];


            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            var requestContent = new
            {
                model = targetModel,
                messages = new ArrayList()
                {
                    new
                    {
                        role = "system",
                        content = prompt
                    },
                    new
                    {
                        role = "user",
                        content = comment
                    },
                }
            };

            var stringContent = new StringContent(JsonSerializer.Serialize(requestContent), Encoding.UTF8, "application/json");
            request.Content = stringContent;

            var response = await _httpClient.SendAsync(request);
            Console.WriteLine(response);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to get response from Groq API. Status: {StatusCode}, Body: {ErrorBody}",
                    response.StatusCode, errorBody);
                return false;
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            var choices = jsonResponse.GetProperty("choices").EnumerateArray();

            if (choices.MoveNext())
            {
                var firstChoice = choices.Current;

                var content = firstChoice.GetProperty("message").GetProperty("content").GetString();


                var isApproved = content?.Trim().ToLower() == "true";

                return isApproved;
            }
            return false;
        }
    }
}
