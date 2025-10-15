using Microsoft.EntityFrameworkCore; 
using ACCAI.Infrastructure.DataSource; 
namespace ACCAI.Infrastructure.Adapters;

public class GenericRepository<T> where T : class
{
    protected readonly DataContext _ctx; 
    protected readonly DbSet<T> _set; 
    public GenericRepository(DataContext ctx){ _ctx=ctx; _set=ctx.Set<T>(); }
}