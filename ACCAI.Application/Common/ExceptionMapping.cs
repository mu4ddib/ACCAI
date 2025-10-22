using System.Net.Http;
using System.Net.Sockets;

namespace ACCAI.Application.Common;

public static class ExceptionMapping
{
    public static ApiError ToApiError(this Exception ex, string target, string? codeOverride = null, string? messageOverride = null)
    {
        if (!string.IsNullOrWhiteSpace(codeOverride) || !string.IsNullOrWhiteSpace(messageOverride))
            return new ApiError(codeOverride ?? "error.custom", messageOverride ?? "Error", target);

        return ex switch
        {
            ExternalServiceException ese
                => new ApiError(ese.Code, ese.Message, ese.Target ?? target),

            RepositoryException re
                => new ApiError(re.Code, re.Message, re.Target ?? target),

            TaskCanceledException
                => new ApiError("http.timeout", "Tiempo de espera agotado al llamar al servicio externo.", target),

            HttpRequestException hre when hre.InnerException is SocketException se &&
                                          (se.SocketErrorCode == SocketError.HostNotFound || se.SocketErrorCode == SocketError.TryAgain)
                => new ApiError("http.dns_unresolved", "No se pudo resolver el host del servicio externo.", target),

            HttpRequestException hre when hre.StatusCode.HasValue
                => new ApiError($"http.{(int)hre.StatusCode.Value}", "Error al llamar al servicio externo.", target),

            _ => new ApiError("unknown", "Error inesperado.", target)
        };
    }
}