using ACCAI.Application.FpChanges;
using ACCAI.Domain.Attributes;
using ACCAI.Domain.Ports.ExternalServices;
using ACCAI.Domain.Ports;
using ACCAI.Infrastructure.Adapters.ExternalServices;
using ACCAI.Infrastructure.Adapters;
using ACCAI.Infrastructure.Parsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ACCAI.Infrastructure.Extensions;
using Moq;
using ACCAI.Infrastructure.DataSource;

namespace Test.Api.ACCAI.Infrastructure.Externals;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private ServiceCollection _services;
    private IConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();

        var settings = new Dictionary<string, string?>
        {
            { "ExternalApis:Accai", "http://accai.local/api/" },
            { "ExternalApis:Crea", "http://crea.local/api/" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [DomainService]
    private class TestDomainService { }

    [Repository]
    private class TestRepository : ITestRepository { }

    private interface ITestRepository { }

    private abstract class AbstractClass { }

    private interface IInterface { }

    private class NoAttributeClass { }


    [Test]
    public void AddAttributedServices_ShouldRegisterDomainService_AndRepository()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        var mockContext = new Mock<DataContext>(
            new Microsoft.EntityFrameworkCore.DbContextOptions<DataContext>()
        );
        _services.AddSingleton(mockContext.Object);

        // Act
        _services.AddAttributedServices(assembly);
        var provider = _services.BuildServiceProvider();

        // Assert DomainService
        var domainService = provider.GetService<TestDomainService>();
        Assert.That(domainService, Is.Not.Null);

        // Assert Repository
        var repo = provider.GetService<ITestRepository>();
        Assert.That(repo, Is.InstanceOf<TestRepository>());

        // Assert UnitOfWork (ya no lanzará error)
        var uow = provider.GetService<IUnitOfWork>();
        Assert.That(uow, Is.InstanceOf<UnitOfWork>());
    }

    [Test]
    public void AddAttributedServices_ShouldSkip_Abstract_And_Interface()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        _services.AddAttributedServices(assembly);
        var provider = _services.BuildServiceProvider();

        // AbstractClass and Interface should NOT be registered
        var abstractService = provider.GetService<AbstractClass>();
        var ifaceService = provider.GetService<IInterface>();

        Assert.That(abstractService, Is.Null);
        Assert.That(ifaceService, Is.Null);
    }

    [Test]
    public void AddInfrastructureServices_ShouldRegisterHttpClients_AndFactory()
    {
        // Act
        _services.AddInfrastructureServices(_configuration);
        var provider = _services.BuildServiceProvider();

        // Assert: ChangeFpFactory is registered
        var factory = provider.GetService<IChangeFpFactory>();
        Assert.That(factory, Is.InstanceOf<ChangeFpFactory>());

        // Assert: HttpClients registered with expected BaseAddress
        var accaiClient = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(nameof(AccaiChangeFpService));

        var creaClient = provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(nameof(CreaChangeFpService));

        Assert.That(accaiClient.BaseAddress!.ToString(), Is.EqualTo("http://accai.local/api/"));
        Assert.That(creaClient.BaseAddress!.ToString(), Is.EqualTo("http://crea.local/api/"));
    }

    [Test]
    public void AddInfrastructure_ShouldRegisterFpChangeCsvParser()
    {
        // Act
        _services.AddInfrastructure();
        var provider = _services.BuildServiceProvider();

        var parser = provider.GetService<IFpChangeCsvParser>();

        Assert.That(parser, Is.InstanceOf<FpChangeCsvParser>());
    }
}
