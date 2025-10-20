using ACCAI.Domain.ReadModels;

namespace ACCAI.Domain.Ports.ExternalServices;

public interface IChangeFpService
{
    Task<bool> SendChangeAsync(ChangeFpItem cambio);
}