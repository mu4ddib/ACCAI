using ACCAI.Domain.Entities;
using ACCAI.Infrastructure.Adapters;
using ACCAI.Infrastructure.DataSource;
using Microsoft.EntityFrameworkCore;

namespace Test.Api.ACCAI.Infrastructure.Adapters.ExternalServices;

[TestFixture]
public class UnitOfWorkTests
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
    public void Constructor_Should_Assign_Context()
    {
        // Arrange
        using var context = new DataContext(_options);

        // Act
        var uow = new UnitOfWork(context);

        // Assert
        Assert.That(uow, Is.Not.Null);
    }

    [Test]
    public async Task SaveChangesAsync_Should_Call_Context_And_Return_Result()
    {
        // Arrange
        using var context = new DataContext(_options);
        var uow = new UnitOfWork(context);

        // Insertamos algo en el contexto
        var contrato = new Contrato
        {
            Id = 1,
            Producto = "TestProducto",
            NumeroContrato = "999",
            PlanProducto = "TestPlan",
            NroDocum = "123",
            TipoDocum = "CC",
            PclidAfi = 10,
            EstadoContrato = "Activo",
            IdAgte = "5"
        };

        await context.Contracts.AddAsync(contrato);

        // Act
        var result = await uow.SaveChangesAsync(CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(1), "Debe devolver el número de registros afectados");

        var saved = await context.Contracts.FindAsync(1);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Producto, Is.EqualTo("TestProducto"));
    }
}