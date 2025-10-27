using ACCAI.Domain.Entities;
using ACCAI.Infrastructure.Adapters;
using ACCAI.Infrastructure.DataSource;
using Microsoft.EntityFrameworkCore;

namespace Test.Api.ACCAI.Infrastructure.Adapters.ExternalServices;

[TestFixture]
public class GenericRepositoryTests
{
    private DbContextOptions<DataContext> _options;

    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public void Constructor_Should_Initialize_Context_And_DbSet()
    {
        // Arrange
        using var context = new DataContext(_options);

        // Act
        var repo = new GenericRepository<Contrato>(context);

        // Assert
        Assert.That(repo, Is.Not.Null, "El repositorio debe inicializarse correctamente");

        // Validamos los campos privados mediante reflexión
        var ctxField = typeof(GenericRepository<Contrato>)
            .GetField("_ctx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var setField = typeof(GenericRepository<Contrato>)
            .GetField("_set", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.That(ctxField, Is.Not.Null);
        Assert.That(setField, Is.Not.Null);

        var ctxValue = ctxField!.GetValue(repo);
        var setValue = setField!.GetValue(repo);

        Assert.That(ctxValue, Is.SameAs(context), "El contexto interno debe coincidir con el inyectado");
        Assert.That(setValue, Is.Not.Null, "El DbSet debe haberse inicializado");
        Assert.That(setValue, Is.InstanceOf<DbSet<Contrato>>());
    }

    [Test]
    public void Constructor_Should_Throw_When_Context_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            // Simulamos un contexto nulo
            DataContext? ctx = null!;
            _ = new GenericRepository<Contrato>(ctx!);
        });
    }
}
