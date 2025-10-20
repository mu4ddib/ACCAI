using ACCAI.Domain.Ports.ExternalServices;
using ACCAI.Domain.ReadModels;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace ACCAI.Infrastructure.Adapters.ExternalServices;

public class AccaiChangeFpService : IChangeFpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccaiChangeFpService> _logger;

    public AccaiChangeFpService(HttpClient httpClient, ILogger<AccaiChangeFpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendChangeAsync(ChangeFpItem change)
    {
        try
        {
            _logger.LogInformation("Sending ChangeFp request: {@ChangeFpItem}", change);

            var response = await _httpClient.PostAsJsonAsync("api/CambioFp", change);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send ChangeFp. Status: {StatusCode}. Response: {Body}",
                    response.StatusCode, body);
                return false;
            }

            _logger.LogInformation("ChangeFp request sent successfully");
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while sending ChangeFp request.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SendChangeAsync");
            return false;
        }
    }
}