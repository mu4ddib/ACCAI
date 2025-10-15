using ACCAI.Domain.ReadModels; 
namespace ACCAI.Domain.Ports;

public interface IVoterSimpleQueryRepository
{
    Task<IEnumerable<VoterSimpleDto>> ListAsync(CancellationToken ct = default);
}