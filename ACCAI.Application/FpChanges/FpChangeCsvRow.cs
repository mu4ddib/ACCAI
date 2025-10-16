namespace ACCAI.Application.FpChanges;

public sealed record FpChangeCsvRow(
    string Apellidos,
    string Nombres,
    string NroDocum,
    string TipoDocum,
    string Producto,
    string PlanProducto,
    string Contrato,
    string Empresa,
    string Segmento,
    string Ciudad,
    string IdAgte,
    string IdAgteNuevo,
    string NombreAgteNuevo,
    string SubGrupoFp,
    string descripcion
);
