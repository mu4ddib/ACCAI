using ACCAI.Domain.Attributes;
using ACCAI.Domain.Entities;
using ACCAI.Domain.Ports;
using ACCAI.Domain.ReadModels;
using ACCAI.Infrastructure.DataSource;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ACCAI.Application.Common;
using Microsoft.Data.SqlClient;

namespace ACCAI.Infrastructure.Adapters;
[Repository]
public class ContractsRepository : GenericRepository<Contrato>, IContractsRepository
{
    private readonly ILogger<ContractsRepository> _logger;
    public ContractsRepository(DataContext ctx, ILogger<ContractsRepository> logger) : base(ctx)
    {
        _logger = logger;
    }

    public async Task<int> UpdateContractsAgentAsync(IEnumerable<ChangeFpItem> changes, CancellationToken ct = default)
    {

        try
        {
            var changeFpItems = changes as ChangeFpItem[] ?? changes.ToArray();
            var contractNumbers = changeFpItems.Select(c => c.Contract).ToList();
            var contracts = await _ctx.Contracts
                .Where(c => contractNumbers.Contains<string>(c.NumeroContrato))
                .ToListAsync(ct);

            if (contracts.Count == 0)
            {
                _logger.LogWarning("No contracts found matching the provided list.");
                return 0;
            }

            foreach (var contract in contracts)
            {
                var change = changeFpItems.FirstOrDefault(c =>
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
        catch (SqlException ex) when (ex.Number == -2) 
            {
                throw new RepositoryException("db.timeout", "Timeout al actualizar contratos.", "db:contratos", ex);
            }
        catch (Exception ex)
            {
                throw new RepositoryException("db.update_failed", "No se pudieron actualizar los contratos.", "db:contratos", ex);
            }
    }
}
