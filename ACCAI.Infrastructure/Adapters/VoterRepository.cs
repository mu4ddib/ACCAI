using Microsoft.EntityFrameworkCore; 
using ACCAI.Domain.Attributes; 
using ACCAI.Domain.Entities; 
using ACCAI.Domain.Ports; 
using ACCAI.Infrastructure.DataSource; 
namespace ACCAI.Infrastructure.Adapters;

[Repository]
public sealed class VoterRepository : GenericRepository<Voter>, IVoterRepository
{
    public VoterRepository(DataContext ctx):base(ctx){} public async Task AddAsync(Voter entity, CancellationToken ct=default)=>await _set.AddAsync(entity, ct); public Task<Voter?> GetByIdAsync(Guid id, CancellationToken ct=default)=>_set.FirstOrDefaultAsync(x=>x.Id==id, ct);
}