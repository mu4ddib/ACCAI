using ACCAI.Application.Common;
using ACCAI.Infrastructure.Adapters;
using ACCAI.Infrastructure.DataSource;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ACCAI.Domain.Entities;
using ACCAI.Domain.ReadModels;
using System.Runtime.Serialization;

namespace Test.Api.ACCAI.Infrastructure.Adapters.ExternalServices;

[TestFixture]
public class ContractsRepositoryTests
{
    private Mock<ILogger<ContractsRepository>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ContractsRepository>>();
    }



    [Test]
    public async Task UpdateContractsAgentAsync_ShouldReturnZero_WhenNoContractsFound()
    {
        // Arrange
        var context = GetContext("NoContractsDb");
        var repo = new ContractsRepository(context, _loggerMock.Object);
        var changes = new List<ChangeFpItem>
            {
                new ChangeFpItem { Contract = 999, PreviousAgentId = 1, NewAgentId = 2 }
            };

        // Act
        var result = await repo.UpdateContractsAgentAsync(changes);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }
    [Test]
    public void UpdateContractsAgentAsync_ShouldThrowRepositoryException_OnSqlTimeout()
    {
        // Arrange
        var sqlEx = CreateSqlException(-2);
        var failingContext = new FailingDataContext(sqlEx);

        failingContext.Contracts.Add(new Contrato
        {
            Id = 1,
            NumeroContrato = 1,
            IdAgte = 1,
            Producto = "Prod",
            PlanProducto = "PlanA",
            EstadoContrato = "Activo",
            TipoDocum = "CC",
            NroDocum = "123"
        });
        failingContext.SaveChanges();

        var repo = new ContractsRepository(failingContext, _loggerMock.Object);
        var changes = new List<ChangeFpItem>
    {
        new ChangeFpItem { Contract = 1, PreviousAgentId = 1, NewAgentId = 2 }
    };

        // Act & Assert
        Assert.ThrowsAsync<RepositoryException>(() => repo.UpdateContractsAgentAsync(changes));
    }

    [Test]
    public void UpdateContractsAgentAsync_ShouldThrowRepositoryException_OnGenericError()
    {
        // Arrange
        var genericEx = new Exception("Unexpected error");
        var failingContext = new FailingDataContext(genericEx);

        // Debe existir al menos un contrato para forzar SaveChangesAsync
        failingContext.Contracts.Add(new Contrato
        {
            Id = 1,
            NumeroContrato = 1,
            IdAgte = 1,
            Producto = "Prod",
            PlanProducto = "PlanA",
            EstadoContrato = "Activo",
            TipoDocum = "CC",
            NroDocum = "123"
        });
        failingContext.SaveChanges();

        var repo = new ContractsRepository(failingContext, _loggerMock.Object);
        var changes = new List<ChangeFpItem>
    {
        new ChangeFpItem { Contract = 1, PreviousAgentId = 1, NewAgentId = 2 }
    };

        // Act & Assert
        Assert.ThrowsAsync<RepositoryException>(() => repo.UpdateContractsAgentAsync(changes));
    }


    private static SqlException CreateSqlException(int number)
    {
        var ex = (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));

        typeof(SqlException).GetField("_number", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(ex, number);

        return ex;
    }

    public static DataContext GetContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new DataContext(options);
    }
}


public class FailingDataContext : DataContext
{
    private readonly Exception _exceptionToThrow;

    public FailingDataContext(Exception exceptionToThrow)
        : base(new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options)
    {
        _exceptionToThrow = exceptionToThrow;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromException<int>(_exceptionToThrow);
    }
}


