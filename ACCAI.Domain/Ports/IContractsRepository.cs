using ACCAI.Domain.ReadModels;

namespace ACCAI.Domain.Ports;

public interface IContractsRepository
{
    Task<int> UpdateContractsAgentAsyncUpdateContractsAgentsAsync(IEnumerable<ChangeFpItem> changes, CancellationToken ct = default);
}
