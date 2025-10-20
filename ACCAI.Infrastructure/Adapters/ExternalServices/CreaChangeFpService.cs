using System.Net.Http.Json;
using ACCAI.Domain.Ports.ExternalServices;
using ACCAI.Domain.ReadModels;
using Microsoft.Extensions.Logging;

namespace ACCAI.Infrastructure.Adapters.ExternalServices;

public class CreaChangeFpService : IChangeFpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CreaChangeFpService> _logger;

    public CreaChangeFpService(HttpClient httpClient, ILogger<CreaChangeFpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendChangeAsync(ChangeFpItem change)
    {
        _logger.LogInformation("Sending CREA ChangeFp request: {@ChangeFpItem}", change);
        var response = await _httpClient.PostAsJsonAsync("api/CambioFp", change);
        return response.IsSuccessStatusCode;
    }
}
