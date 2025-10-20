using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ACCAI.Domain.Attributes;
using ACCAI.Domain.Ports;
using ACCAI.Infrastructure.Adapters;
using ACCAI.Application.FpChanges;
using ACCAI.Infrastructure.Parsers;
using ACCAI.Domain.Ports.ExternalServices;
using ACCAI.Infrastructure.Adapters.ExternalServices;
using Microsoft.Extensions.Configuration;
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

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<AccaiChangeFpService>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalApis:Accai"]);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<CreaChangeFpService>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalApis:Crea"]);
        });

        services.AddScoped<IChangeFpFactory, ChangeFpFactory>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IFpChangeCsvParser, FpChangeCsvParser>();
        return services;
    }
}
