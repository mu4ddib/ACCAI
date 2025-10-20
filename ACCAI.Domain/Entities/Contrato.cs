namespace ACCAI.Domain.Entities;

public class Contrato
{
    public int Id { get; set; }
    public string Producto { get; set; }
    public int NumeroContrato { get; set; }
    public string PlanProducto { get; set; }
    public string NroDocum { get; set; }
    public string TipoDocum { get; set; }
    public int PclidAfi { get; set; }
    public DateTime? FecInicio { get; set; }
    public DateTime? FecEfectiva { get; set; }
    public DateTime? FecTerminacion { get; set; }
    public decimal? Saldo { get; set; }
    public decimal? SaldoUn { get; set; }
    public DateTime? FecSaldo { get; set; }
    public decimal? ValorAporteMes { get; set; }
    public DateTime? FecUltimoAporte { get; set; }
    public string EstadoContrato { get; set; }
    public int IdAgte { get; set; }
    public decimal? SaldoCapital { get; set; }
    public decimal? SaldoRendimientos { get; set; }
    public decimal? Cuenta { get; set; }
}
