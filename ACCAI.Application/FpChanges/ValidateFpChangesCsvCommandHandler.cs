using ACCAI.Application.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ACCAI.Application.FpChanges;

public sealed class ValidateFpChangesCsvCommandHandler
    : IRequestHandler<ValidateFpChangesCsvCommand, ValidationResponseDto>
{
    private readonly IFpChangeCsvParser _parser;
    private readonly IValidator<FpChangeCsvRow> _rowValidator;
    private readonly ILogger<ValidateFpChangesCsvCommandHandler> _log;

    private static readonly string[] ExpectedHeaders =
    [
        "Apellidos","Nombres","NroDocum","TipoDocum","Producto","PlanProducto","Contrato","Empresa","Segmento",
        "Ciudad","IdAgte","NombreAgte","IdAgteNuevo","NombreAgteNuevo","SubGrupoFp","descripcion"
    ];

    public ValidateFpChangesCsvCommandHandler(
        IFpChangeCsvParser parser,
        IValidator<FpChangeCsvRow> rowValidator,
        ILogger<ValidateFpChangesCsvCommandHandler> log)
    {
        _parser = parser;
        _rowValidator = rowValidator;
        _log = log;
    }

    public async Task<ValidationResponseDto> Handle(ValidateFpChangesCsvCommand request, CancellationToken ct)
    {
        var cid = Guid.NewGuid().ToString("N");
        
        if (request.FileLength <= 0)
            return ValidationResponseDto.Fail("Archivo vacío.", cid);
        if (request.FileLength > 1_000_000)
            return ValidationResponseDto.Fail("Tamaño máximo 1MB.", cid);
        
        var parsed = await _parser.ParseAsync(request.FileStream, ct);
        
        if (parsed.Header.Count != ExpectedHeaders.Length || !parsed.Header.SequenceEqual(ExpectedHeaders))
        {
            var msg = $"Cabeceras inválidas. Esperado: {string.Join(",", ExpectedHeaders)}. " +
                      $"Recibido: {string.Join(",", parsed.Header)}";
            return ValidationResponseDto.Fail(msg, cid);
        }
        
        if (parsed.Rows.Count > 50)
            return ValidationResponseDto.Fail("Máximo 50 registros por archivo.", cid);
        
        var errors = new List<RowError>();
        for (int i = 0; i < parsed.Rows.Count; i++)
        {
            var row = parsed.Rows[i];
            var rowIndex = i + 2; 

            var result = await _rowValidator.ValidateAsync(row, ct);
            if (result.IsValid) continue;
            foreach (var e in result.Errors)
            {
                var err = new RowError(rowIndex, e.PropertyName, e.ErrorMessage, GetRaw(row, e.PropertyName));
                errors.Add(err);
                _log.LogWarning("CSV validation error {@err} CorrelationId={CorrelationId}", err, cid);
            }
        }

        var response = ValidationResponseDto.From(parsed.Rows.Count, errors, cid);

        if (errors.Count == 0)
            _log.LogInformation("CSV validation OK: {Total} filas. CorrelationId={CorrelationId}", parsed.Rows.Count, cid);
        else
            _log.LogInformation("CSV validation FAILED: {Errors} errores en {Total} filas. CorrelationId={CorrelationId}",
                errors.Count, parsed.Rows.Count, cid);

        return response;
    }

    private static string? GetRaw(FpChangeCsvRow r, string prop) => prop switch
    {
        nameof(FpChangeCsvRow.Apellidos) => r.Apellidos,
        nameof(FpChangeCsvRow.Nombres) => r.Nombres,
        nameof(FpChangeCsvRow.NroDocum)     => r.NroDocum,
        nameof(FpChangeCsvRow.TipoDocum)    => r.TipoDocum,
        nameof(FpChangeCsvRow.Producto)     => r.Producto,
        nameof(FpChangeCsvRow.PlanProducto) => r.PlanProducto,
        nameof(FpChangeCsvRow.Contrato)     => r.Contrato,
        nameof(FpChangeCsvRow.Empresa)      => r.Empresa,
        nameof(FpChangeCsvRow.Segmento)      => r.Segmento,
        nameof(FpChangeCsvRow.Ciudad)      => r.Ciudad,
        nameof(FpChangeCsvRow.IdAgte)       => r.IdAgte,
        nameof(FpChangeCsvRow.IdAgteNuevo)  => r.IdAgteNuevo,
        nameof(FpChangeCsvRow.SubGrupoFp)   => r.SubGrupoFp,
        nameof(FpChangeCsvRow.descripcion)  => r.descripcion,
        _ => null
    };
}
