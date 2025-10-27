using ACCAI.Domain.Entities;
using ACCAI.Infrastructure.DataSource;
using Microsoft.EntityFrameworkCore;

namespace Test.Api.ACCAI.Infrastructure.DataSource;

[TestFixture]
public class DataContextTests
{
    private DbContextOptions<DataContext> _options;

    [SetUp]
    public void Setup()
    {
        // Configuramos base de datos en memoria única por prueba
        _options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public void Constructor_Should_Initialize_Context()
    {
        // Act
        using var context = new DataContext(_options);

        // Assert
        Assert.That(context, Is.Not.Null);
        Assert.That(context.Contracts, Is.Not.Null);
    }

    [Test]
    public void OnModelCreating_Should_Apply_Configurations_From_Assembly()
    {
        // Arrange
        using var context = new DataContext(_options);

        // Act
        var model = context.Model;

        // Assert
        var contratoEntity = model.FindEntityType(typeof(Contrato));
        Assert.That(contratoEntity, Is.Not.Null);
        Assert.That(contratoEntity!.GetTableName(), Is.EqualTo("Contrato"));
    }

    [Test]
    public async Task SaveChangesAsync_Should_Call_Base_Implementation()
    {
        // Arrange
        using var context = new DataContext(_options);

        var contrato = new Contrato
        {
            Id = 99,
            Producto = "Ahorro Futuro",
            NumeroContrato = 50001,
            PlanProducto = "Plan Gold",
            NroDocum = "111222333",
            TipoDocum = "CC",
            PclidAfi = 500,
            EstadoContrato = "Activo",
            IdAgte = 1
        };

        await context.Contracts.AddAsync(contrato);

        // Act
        var result = await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(1));

        var saved = await context.Contracts.FindAsync(99);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Producto, Is.EqualTo("Ahorro Futuro"));
    }

    [Test]
    public void OnModelCreating_Should_Include_Seed_Data()
    {
        // Arrange
        using var context = new DataContext(_options);

        // Act
        context.Database.EnsureCreated();
        var all = context.Contracts.ToListAsync().Result;

        // Assert
        Assert.That(all.Count, Is.EqualTo(2), "Debe contener los dos contratos del seeding");
        Assert.That(all.Exists(c => c.Producto.Contains("Ahorro Programado")), Is.True);
        Assert.That(all.Exists(c => c.Producto.Contains("Fondo de Inversión")), Is.True);
    }
}
