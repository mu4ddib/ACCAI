using ACCAI.Domain.ReadModels;

namespace ACCAI.Domain.Ports;

public interface IContractsRepository
{
    Task<int> UpdateContractsAgentAsync(IEnumerable<ChangeFpItem> changes, CancellationToken ct = default);
}
