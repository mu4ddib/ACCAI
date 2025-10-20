using ACCAI.Domain.Ports.ExternalServices;
using Microsoft.Extensions.DependencyInjection;

namespace ACCAI.Infrastructure.Adapters.ExternalServices;

public class ChangeFpFactory : IChangeFpFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDictionary<string, Type> _serviceMap;

    public ChangeFpFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _serviceMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "ACCAI", typeof(AccaiChangeFpService) },
                { "CREA", typeof(CreaChangeFpService) }
            };
    }

    public IChangeFpService GetService(string product)
    {
        if (!_serviceMap.TryGetValue(product.ToUpper(), out var serviceType))
            throw new InvalidOperationException($"No ChangeFpService registered for product '{product}'.");

        return (IChangeFpService)_serviceProvider.GetRequiredService(serviceType);
    }
}