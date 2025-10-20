using Microsoft.EntityFrameworkCore;
using ACCAI.Domain.Entities;
using CsvHelper.Configuration.Attributes;
namespace ACCAI.Infrastructure.DataSource;
public sealed class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    //public DbSet<Voter> Voters => Set<Voter>();
    public DbSet<Contrato> Contracts { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedOn").CurrentValue = now;
                entry.Property("LastModifiedOn").CurrentValue = now;
            }
            if (entry.State == EntityState.Modified)
            {
                entry.Property("LastModifiedOn").CurrentValue = now;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
