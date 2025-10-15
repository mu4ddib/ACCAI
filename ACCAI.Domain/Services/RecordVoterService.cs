using ACCAI.Domain.Attributes;
using ACCAI.Domain.Entities;
using ACCAI.Domain.Ports;

namespace ACCAI.Domain.Services;

[DomainService]
public sealed class RecordVoterService
{
    private readonly IVoterRepository _repo;
    private readonly IUnitOfWork _uow;

    public RecordVoterService(IVoterRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Voter> RecordAsync(Voter voter, CancellationToken ct = default)
    {
        await _repo.AddAsync(voter, ct);
        await _uow.SaveChangesAsync(ct);
        return voter;
    }
}