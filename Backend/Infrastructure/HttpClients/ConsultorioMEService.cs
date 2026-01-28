using Application.DTOs;
using Application.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HttpClients
{
    public class ConsultorioMEService : IConsultorioMEService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConsultorioMEService> _logger;
        private readonly string _baseUrl;
        private readonly string _clientId;
        private readonly string _secret;

        public ConsultorioMEService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ConsultorioMEService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _baseUrl = _configuration["ConsultorioME:BaseUrl"] ?? "https://api.consultoriome.com/v1/api";
            _clientId = _configuration["ConsultorioME:ClientId"] ?? throw new InvalidOperationException("ConsultorioME:ClientId is not configured");
            _secret = _configuration["ConsultorioME:Secret"] ?? throw new InvalidOperationException("ConsultorioME:Secret is not configured");
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                // Create a new HttpClient without BaseAddress to have full control
                var client = _httpClientFactory.CreateClient();
                
                // Create Basic Auth header exactly like Postman does
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_secret}"));
                
                // Construct full URL manually (matching Postman)
                var baseUrlString = _baseUrl.TrimEnd('/');
                var fullUrl = $"{baseUrlString}/authorization/token";
                
                // Create request message to set authorization header (like Postman Basic Auth)
                var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                
                // Postman Basic Auth doesn't send a body for token requests

                // Make POST request to token endpoint
                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get token. URL: {Url}, Status: {StatusCode}, Response: {Response}", 
                        fullUrl, response.StatusCode, errorContent);
                    throw new HttpRequestException($"Failed to get token. Status: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                
                // Try to deserialize as JSON object first
                try
                {
                    var tokenResponse = JsonSerializer.Deserialize<ConsultorioMETokenResponseDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
                    {
                        return tokenResponse.Token;
                    }
                }
                catch (JsonException)
                {
                    // If deserialization fails, assume the response is just the token string
                    _logger.LogDebug("Token response is not JSON, treating as plain string");
                }

                // If JSON deserialization failed or token is empty, return the content as-is (might be plain token string)
                return content.Trim('"'); // Remove quotes if present
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ConsultorioME token");
                throw;
            }
        }

        public async Task<IEnumerable<ConsultorioMEProfessionalDto>> GetProfessionalsAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                var client = _httpClientFactory.CreateClient();
                
                // Construct full URL manually
                var baseUrlString = _baseUrl.TrimEnd('/');
                var fullUrl = $"{baseUrlString}/appointment/professionals/";
                
                // Create request message to set authorization header
                var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get professionals. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Failed to get professionals. Status: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var professionals = JsonSerializer.Deserialize<IEnumerable<ConsultorioMEProfessionalDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return professionals ?? Enumerable.Empty<ConsultorioMEProfessionalDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ConsultorioME professionals");
                throw;
            }
        }

        public async Task<T> SendRequestAsync<T>(HttpMethod method, string endpoint, object? body = null)
        {
            try
            {
                var token = await GetTokenAsync();
                var client = _httpClientFactory.CreateClient("ConsultorioME");
                
                HttpRequestMessage request = new HttpRequestMessage(method, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send request to {Endpoint}. Status: {StatusCode}, Response: {Response}", 
                        endpoint, response.StatusCode, errorContent);
                    throw new HttpRequestException($"Failed to send request to {endpoint}. Status: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize response from {endpoint}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending request to {Endpoint}", endpoint);
                throw;
            }
        }
    }
}
