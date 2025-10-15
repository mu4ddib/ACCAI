using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ACCAI.Domain.Attributes;
using ACCAI.Domain.Ports;
using ACCAI.Infrastructure.Adapters;
namespace ACCAI.Infrastructure.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttributedServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var type in assemblies.SelectMany(a => a.DefinedTypes))
        {
            if (type.IsAbstract || type.IsInterface) continue;
            if (type.GetCustomAttribute<DomainServiceAttribute>() is not null)
            { services.AddTransient(type.AsType()); continue; }
            if (type.GetCustomAttribute<RepositoryAttribute>() is not null)
            {
                var iface = type.ImplementedInterfaces.FirstOrDefault();
                if (iface is not null) services.AddScoped(iface, type.AsType());
                else services.AddScoped(type.AsType());
            }
        }
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
