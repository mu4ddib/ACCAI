namespace ACCAI.Application.FpChanges;

public sealed record FpChangeCsvRow(
    string Contrato,
    string Producto,
    string PlanProducto,
    string NroDocum,
    string TipoDocum,
    string IdAgteNuevo,
    string IdAgte,
    string SubGrupoFp,
    string MotivoCambio
);