using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace BankApplication.Services
{
    public class FAQService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public FAQService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["OpenAI:ApiKey"];
        }

        public async Task<string> AskQuestionAsync(string question)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant for a banking application." },
                    new { role = "user", content = question }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"OpenAI API error: {response.StatusCode} - {responseBody}";
            }

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var message = doc.RootElement
                                 .GetProperty("choices")[0]
                                 .GetProperty("message")
                                 .GetProperty("content")
                                 .GetString();

                return message?.Trim() ?? "I couldn't generate a response.";
            }
            catch (Exception ex)
            {
                return $"Error parsing OpenAI response: {ex.Message}\nRaw response: {responseBody}";
            }
        }
    }
}
