using ACCAI.Domain.Ports.ExternalServices;
using ACCAI.Domain.ReadModels;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using ACCAI.Application.Common;

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
                _logger.LogError("ChangeFp non-success. Status: {StatusCode}. Body: {Body}", response.StatusCode, body);

                throw new ExternalServiceException(
                    code: "http.non_success",
                    message: $"HTTP {(int)response.StatusCode} al llamar a ACCAI.",
                    target: "external:accai",
                    statusCode: (int)response.StatusCode
                );
            }

            _logger.LogInformation("ChangeFp request sent successfully");
            return true;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout sending ChangeFp.");
            throw new ExternalServiceException("http.timeout", "Timeout al llamar a ACCAI.", "external:accai", inner: ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while sending ChangeFp.");
            throw new ExternalServiceException("http.network", "Error de red al llamar a ACCAI.", "external:accai", inner: ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SendChangeAsync");
            throw new ExternalServiceException("external.error", "Error inesperado llamando a ACCAI.", "external:accai", inner: ex);
        }
    }
}