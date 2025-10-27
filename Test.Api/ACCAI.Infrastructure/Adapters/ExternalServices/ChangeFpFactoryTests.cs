using ACCAI.Domain.Ports.ExternalServices;
using ACCAI.Infrastructure.Adapters.ExternalServices;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Api.ACCAI.Infrastructure.Adapters.ExternalServices;

[TestFixture]
public class ChangeFpFactoryTests
{
    private ServiceCollection _services;
    private ServiceProvider _provider;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();

        _services.AddHttpClient();

        _services.AddLogging();

        _services.AddTransient<AccaiChangeFpService>();
        _services.AddTransient<CreaChangeFpService>();

        _services.AddTransient<IChangeFpService, AccaiChangeFpService>();

        _provider = _services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _provider?.Dispose();
    }

    [Test]
    public void GetService_ShouldReturnAccaiChangeFpService_WhenProductIsAccai()
    {
        var factory = new ChangeFpFactory(_provider);
        var service = factory.GetService("accai");

        Assert.That(service, Is.InstanceOf<AccaiChangeFpService>());
    }

    [Test]
    public void GetService_ShouldReturnCreaChangeFpService_WhenProductIsCrea()
    {
        var factory = new ChangeFpFactory(_provider);
        var service = factory.GetService("CREA");

        Assert.That(service, Is.InstanceOf<CreaChangeFpService>());
    }

    [Test]
    public void GetService_ShouldThrowException_WhenProductNotRegistered()
    {
        var factory = new ChangeFpFactory(_provider);

        var ex = Assert.Throws<InvalidOperationException>(() => factory.GetService("UNKNOWN"));
        Assert.That(ex.Message, Does.Contain("No ChangeFpService registered"));
    }
}