using ACCAI.Domain.Entities; 
namespace ACCAI.Domain.Ports;

public interface IVoterRepository
{
    Task AddAsync(Voter entity, CancellationToken ct = default); Task<Voter?> GetByIdAsync(Guid id, CancellationToken ct = default);
}