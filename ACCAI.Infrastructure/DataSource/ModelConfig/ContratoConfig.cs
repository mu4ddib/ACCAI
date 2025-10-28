using ACCAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACCAI.Infrastructure.DataSource.ModelConfig;

public class ContratoConfig : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("Contrato");
        builder.HasKey(c => c.Id);

        SeedContratoData(builder);
    }

    private static void SeedContratoData(EntityTypeBuilder<Contrato> builder)
    {
        builder.HasData(
              new Contrato
              {
                  Id = 1,
                  Producto = "Ahorro Programado",
                  NumeroContrato = "10001",
                  PlanProducto = "Plan Básico",
                  NroDocum = "123456789",
                  TipoDocum = "CC",
                  PclidAfi = 101,
                  FecInicio = new DateTime(2022, 1, 1),
                  FecEfectiva = new DateTime(2022, 1, 15),
                  FecTerminacion = new DateTime(2025, 1, 15),
                  Saldo = 1500000m,
                  SaldoUn = 1500000m,
                  FecSaldo = new DateTime(2025, 1, 1),
                  ValorAporteMes = 50000m,
                  FecUltimoAporte = new DateTime(2025, 1, 10),
                  EstadoContrato = "Activo",
                  IdAgte = "5834",
                  SaldoCapital = 1200000m,
                  SaldoRendimientos = 300000m,
                  Cuenta = 10001
              },
              new Contrato
              {
                  Id = 2,
                  Producto = "Fondo de Inversión",
                  NumeroContrato = "10002",
                  PlanProducto = "Premium",
                  NroDocum = "987654321",
                  TipoDocum = "CC",
                  PclidAfi = 102,
                  FecInicio = new DateTime(2023, 5, 10),
                  FecEfectiva = new DateTime(2023, 5, 15),
                  FecTerminacion = new DateTime(2026, 5, 15),
                  Saldo = 3500000m,
                  SaldoUn = 3500000m,
                  FecSaldo = new DateTime(2025, 1, 1),
                  ValorAporteMes = 200000m,
                  FecUltimoAporte = new DateTime(2025, 1, 5),
                  EstadoContrato = "Activo",
                  IdAgte = "5834",
                  SaldoCapital = 3000000m,
                  SaldoRendimientos = 500000m,
                  Cuenta = 10002
              }
          );
    }
}
