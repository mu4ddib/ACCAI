using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ACCAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contrato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Producto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroContrato = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanProducto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NroDocum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoDocum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PclidAfi = table.Column<int>(type: "int", nullable: false),
                    FecInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FecEfectiva = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FecTerminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SaldoUn = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FecSaldo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValorAporteMes = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FecUltimoAporte = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstadoContrato = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdAgte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaldoCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SaldoRendimientos = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Cuenta = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contrato", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Voters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voters", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Contrato",
                columns: new[] { "Id", "Cuenta", "EstadoContrato", "FecEfectiva", "FecInicio", "FecSaldo", "FecTerminacion", "FecUltimoAporte", "IdAgte", "NroDocum", "NumeroContrato", "PclidAfi", "PlanProducto", "Producto", "Saldo", "SaldoCapital", "SaldoRendimientos", "SaldoUn", "TipoDocum", "ValorAporteMes" },
                values: new object[,]
                {
                    { 1, 10001m, "Activo", new DateTime(2022, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "5834", "123456789", "10001", 101, "Plan Básico", "Ahorro Programado", 1500000m, 1200000m, 300000m, 1500000m, "CC", 50000m },
                    { 2, 10002m, "Activo", new DateTime(2023, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "5834", "987654321", "10002", 102, "Premium", "Fondo de Inversión", 3500000m, 3000000m, 500000m, 3500000m, "CC", 200000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Voters_Nid",
                table: "Voters",
                column: "Nid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contrato");

            migrationBuilder.DropTable(
                name: "Voters");
        }
    }
}
