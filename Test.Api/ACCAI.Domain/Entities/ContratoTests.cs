using ACCAI.Domain.Entities;

namespace Test.Api.ACCAI.Domain.Entities;

[TestFixture]
public class ContratoTests
{
    [Test]
    public void Should_Assign_And_Read_All_Properties_Correctly()
    {
        // Arrange
        var fecha = DateTime.UtcNow;
        var contrato = new Contrato
        {
            Id = 1,
            Producto = "ACCAI",
            NumeroContrato = 12345,
            PlanProducto = "Plan Premium",
            NroDocum = "100200300",
            TipoDocum = "C",
            PclidAfi = 10,
            FecInicio = fecha.AddDays(-30),
            FecEfectiva = fecha.AddDays(-15),
            FecTerminacion = fecha.AddDays(15),
            Saldo = 1000.50m,
            SaldoUn = 2000.75m,
            FecSaldo = fecha.AddDays(-5),
            ValorAporteMes = 500.00m,
            FecUltimoAporte = fecha.AddDays(-2),
            EstadoContrato = "Activo",
            IdAgte = 99,
            SaldoCapital = 800.25m,
            SaldoRendimientos = 120.75m,
            Cuenta = 999.99m
        };

        // Act & Assert — cada propiedad conserva su valor asignado
        Assert.That(contrato.Id, Is.EqualTo(1));
        Assert.That(contrato.Producto, Is.EqualTo("ACCAI"));
        Assert.That(contrato.NumeroContrato, Is.EqualTo(12345));
        Assert.That(contrato.PlanProducto, Is.EqualTo("Plan Premium"));
        Assert.That(contrato.NroDocum, Is.EqualTo("100200300"));
        Assert.That(contrato.TipoDocum, Is.EqualTo("C"));
        Assert.That(contrato.PclidAfi, Is.EqualTo(10));
        Assert.That(contrato.FecInicio, Is.EqualTo(fecha.AddDays(-30)));
        Assert.That(contrato.FecEfectiva, Is.EqualTo(fecha.AddDays(-15)));
        Assert.That(contrato.FecTerminacion, Is.EqualTo(fecha.AddDays(15)));
        Assert.That(contrato.Saldo, Is.EqualTo(1000.50m));
        Assert.That(contrato.SaldoUn, Is.EqualTo(2000.75m));
        Assert.That(contrato.FecSaldo, Is.EqualTo(fecha.AddDays(-5)));
        Assert.That(contrato.ValorAporteMes, Is.EqualTo(500.00m));
        Assert.That(contrato.FecUltimoAporte, Is.EqualTo(fecha.AddDays(-2)));
        Assert.That(contrato.EstadoContrato, Is.EqualTo("Activo"));
        Assert.That(contrato.IdAgte, Is.EqualTo(99));
        Assert.That(contrato.SaldoCapital, Is.EqualTo(800.25m));
        Assert.That(contrato.SaldoRendimientos, Is.EqualTo(120.75m));
        Assert.That(contrato.Cuenta, Is.EqualTo(999.99m));
    }

    [Test]
    public void Should_Handle_Nullable_Properties()
    {
        // Arrange
        var contrato = new Contrato
        {
            Saldo = null,
            SaldoUn = null,
            FecInicio = null,
            FecEfectiva = null,
            FecTerminacion = null,
            FecSaldo = null,
            ValorAporteMes = null,
            FecUltimoAporte = null,
            SaldoCapital = null,
            SaldoRendimientos = null,
            Cuenta = null
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(contrato.Saldo, Is.Null);
            Assert.That(contrato.FecInicio, Is.Null);
            Assert.That(contrato.Cuenta, Is.Null);
        });
    }
}
