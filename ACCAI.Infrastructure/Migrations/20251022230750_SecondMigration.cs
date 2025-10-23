using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ACCAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Contrato",
                columns: new[] { "Id", "Cuenta", "EstadoContrato", "FecEfectiva", "FecInicio", "FecSaldo", "FecTerminacion", "FecUltimoAporte", "IdAgte", "NroDocum", "NumeroContrato", "PclidAfi", "PlanProducto", "Producto", "Saldo", "SaldoCapital", "SaldoRendimientos", "SaldoUn", "TipoDocum", "ValorAporteMes" },
                values: new object[,]
                {
                    { 1, 10001m, "Activo", new DateTime(2022, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "123456789", 10001, 101, "Plan Básico", "Ahorro Programado", 1500000m, 1200000m, 300000m, 1500000m, "CC", 50000m },
                    { 2, 10002m, "Activo", new DateTime(2023, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, "987654321", 10002, 102, "Premium", "Fondo de Inversión", 3500000m, 3000000m, 500000m, 3500000m, "CC", 200000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Contrato",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Contrato",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
