using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ACCAI.Domain.Attributes;
using ACCAI.Domain.Ports;
using ACCAI.Domain.ReadModels;
namespace ACCAI.Infrastructure.Adapters;
[Repository]
public sealed class VoterSimpleRepository : IVoterSimpleQueryRepository
{
    private readonly string _connString;
    public VoterSimpleRepository(IConfiguration configuration)
    {
        _connString = configuration.GetConnectionString("db") ?? throw new InvalidOperationException("Missing connection string 'db'.");
    }
    private IDbConnection OpenConnection() => new SqlConnection(_connString);
    public async Task<IEnumerable<VoterSimpleDto>> ListAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT Id, Nid FROM Voters ORDER BY Nid";
        using var cn = OpenConnection();
        var rows = await cn.QueryAsync<VoterSimpleDto>(sql);
        return rows;
    }
}
