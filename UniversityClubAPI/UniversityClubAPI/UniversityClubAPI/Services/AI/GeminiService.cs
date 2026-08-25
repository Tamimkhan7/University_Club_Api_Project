using System.Text;
using System.Text.Json;

namespace UniversityClubAPI.Services.AI
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string?> GenerateTextAsync(string prompt)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"];
            if (string.IsNullOrWhiteSpace(model))
                model = "gemini-1.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Gemini:ApiKey is not configured. Skipping AI text generation (caller should fall back).");
                return null;
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        using var stream = await response.Content.ReadAsStreamAsync();
                        using var doc = await JsonDocument.ParseAsync(stream);

                        var text = doc.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
                    }

                    var errorBody = await response.Content.ReadAsStringAsync();
                    var isTransient = response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                                       (int)response.StatusCode >= 500;

                    _logger.LogError("Gemini API call failed ({Status}), attempt {Attempt}/{Max}: {Body}",
                        response.StatusCode, attempt, maxAttempts, errorBody);

                    if (!isTransient || attempt == maxAttempts)
                        return null;

                    await Task.Delay(TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt - 1)));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error calling Gemini API, attempt {Attempt}/{Max}.", attempt, maxAttempts);
                    if (attempt == maxAttempts)
                        return null;
                    await Task.Delay(TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt - 1)));
                }
            }

            return null;
        }
    }
}
