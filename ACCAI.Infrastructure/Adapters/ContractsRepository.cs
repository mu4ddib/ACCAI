using ACCAI.Domain.Attributes;
using ACCAI.Domain.Entities;
using ACCAI.Domain.Ports;
using ACCAI.Domain.ReadModels;
using ACCAI.Infrastructure.DataSource;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ACCAI.Infrastructure.Adapters;
[Repository]
public class ContractsRepository : GenericRepository<Voter>, IContractsRepository
{
    private readonly ILogger<ContractsRepository> _logger;
    public ContractsRepository(DataContext ctx, ILogger<ContractsRepository> logger) : base(ctx)
    {
        _logger = logger;
    }

    public async Task<int> UpdateContractsAgentAsyncUpdateContractsAgentsAsync(IEnumerable<ChangeFpItem> changes, CancellationToken ct = default)
    {
        var contractNumbers = changes.Select(c => c.Contract).ToList();
        var contracts = await _ctx.Contracts
            .Where(c => contractNumbers.Contains(c.NumeroContrato))
            .ToListAsync(ct);

        if (contracts.Count == 0)
        {
            _logger.LogWarning("No contracts found matching the provided list.");
            return 0;
        }

        foreach (var contract in contracts)
        {
            var change = changes.FirstOrDefault(c =>
                c.Contract == contract.NumeroContrato &&
                c.PreviousAgentId == contract.IdAgte);

            if (change != null)
            {
                contract.IdAgte = change.NewAgentId;
            }
        }

        var affected = await _ctx.SaveChangesAsync(ct);
        _logger.LogInformation("Updated {Count} contracts successfully.", affected);

        return affected;
    }


}
