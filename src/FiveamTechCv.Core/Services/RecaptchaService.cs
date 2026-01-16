using System.Text.Json;
using FiveamTechCv.Abstract.Services;
using Microsoft.Extensions.Configuration;

namespace FiveamTechCv.Core.Services;

public class RecaptchaService : IRecaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RecaptchaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        var secretKey = _configuration["Recaptcha:SecretKey"];
        // If no key is configured, skip verification (Dev mode)
        if (string.IsNullOrEmpty(secretKey))
        {
            return true;
        }

        try
        {
            var response = await _httpClient.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}", null);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RecaptchaResponse>(jsonString);

            // You can also check score here if needed, e.g., result.Score > 0.5
            return result?.Success == true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Recaptcha verification failed: {ex.Message}");
            return false;
        }
    }

    private class RecaptchaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("score")]
        public float Score { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("action")]
        public string Action { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("challenge_ts")]
        public string ChallengeTs { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("hostname")]
        public string Hostname { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}
