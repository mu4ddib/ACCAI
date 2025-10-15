namespace ACCAI.Application.Dtos;

public sealed record RowError(int Linea, string Campo, string Mensaje, string? Valor);

public sealed record ValidationResponseDto(
    int TotalFilas, int Errores, string CorrelationId, IReadOnlyList<RowError> Detalle)
{
    public static ValidationResponseDto From(int total, List<RowError> errs, string cid)
        => new(total, errs.Count, cid, errs);

    public static ValidationResponseDto Fail(string message, string cid)
        => new(0, 1, cid, new List<RowError> { new RowError(0, "_archivo", message, null) });
}