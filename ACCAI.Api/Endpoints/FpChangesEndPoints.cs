using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;
using ACCAI.Application.FpChanges;
using Microsoft.AspNetCore.Mvc;
namespace ACCAI.Api.Endpoints;
using Microsoft.AspNetCore.Antiforgery; 

public static class FpChangesEndPoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/fp-changes").WithTags("FP Changes");

        group.MapPost("/validate",
                async Task<Results<Ok<ValidationResponseDto>, BadRequest<ValidationResponseDto>>> (
                    [FromForm] IFormFile file,
                    IValidator<FpChangeCsvRow> rowValidator) => {

            var correlationId = Guid.NewGuid().ToString("N");
            
            var errors = new List<RowError>();
            if (file is null || file.Length == 0)
                return TypedResults.BadRequest(ValidationResponseDto.Fail("Archivo vacío.", correlationId));

            if (file.Length > 1_000_000) // 1MB
                return TypedResults.BadRequest(ValidationResponseDto.Fail("Tamaño máximo 1MB.", correlationId));

         
            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                BadDataFound = null,
                MissingFieldFound = null,
                DetectDelimiter = false, 
                Delimiter = ","
            };

            var expectedHeaders = new[]
            {
                "Contrato","Producto","PlanProducto","NroDocum","TipoDocum",
                "IdAgteNuevo","IdAgte","SubGrupoFp","MotivoCambio"
            };

            var rows = new List<FpChangeCsvRow>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, cfg))
            {
  
                if (!csv.Read() || !csv.ReadHeader())
                    return TypedResults.BadRequest(ValidationResponseDto.Fail("Cabecera no encontrada.", correlationId));

                var header = csv.HeaderRecord ?? Array.Empty<string>();
                if (header.Length != expectedHeaders.Length || !header.SequenceEqual(expectedHeaders))
                {
                    var msg = $"Cabeceras inválidas. Esperado: {string.Join(",", expectedHeaders)}. " +
                              $"Recibido: {string.Join(",", header)}";
                    return TypedResults.BadRequest(ValidationResponseDto.Fail(msg, correlationId));
                }

     
                int rowIndex = 1; 
                while (await csv.ReadAsync())
                {
                    rowIndex++;
                    var row = new FpChangeCsvRow(
                        csv.GetField("Contrato")?.Trim() ?? "",
                        csv.GetField("Producto")?.Trim() ?? "",
                        csv.GetField("PlanProducto")?.Trim() ?? "",
                        csv.GetField("NroDocum")?.Trim() ?? "",
                        csv.GetField("TipoDocum")?.Trim() ?? "",
                        csv.GetField("IdAgteNuevo")?.Trim() ?? "",
                        csv.GetField("IdAgte")?.Trim() ?? "",
                        csv.GetField("SubGrupoFp")?.Trim() ?? "",
                        csv.GetField("MotivoCambio")?.Trim() ?? ""
                    );

                    var res = await rowValidator.ValidateAsync(row);
                    if (!res.IsValid)
                    {
                        foreach (var e in res.Errors)
                        {
                            var err = new RowError(rowIndex, e.PropertyName, e.ErrorMessage, GetRawValue(row, e.PropertyName));
                            errors.Add(err);
                            Log.Warning("CSV validation error {@err} CorrelationId={CorrelationId}", err, correlationId);
                        }
                    }
                    rows.Add(row);

                    if (rows.Count > 50)
                        return TypedResults.BadRequest(ValidationResponseDto.Fail("Máximo 50 registros por archivo.", correlationId));
                }
            }

            var response = ValidationResponseDto.From(rows.Count, errors, correlationId);

            if (errors.Count == 0)
            {
                Log.Information("CSV validation OK: {Total} filas. CorrelationId={CorrelationId}", rows.Count, correlationId);
                return TypedResults.Ok(response);
            }
            else
            {
                Log.Information("CSV validation FAILED: {Errors} errores en {Total} filas. CorrelationId={CorrelationId}",
                    errors.Count, rows.Count, correlationId);
                return TypedResults.BadRequest(response);
            }
                }).DisableAntiforgery()   
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<ValidationResponseDto>(StatusCodes.Status200OK)
        .Produces<ValidationResponseDto>(StatusCodes.Status400BadRequest)
        .WithName("ValidateFpChangeCsv")
        .WithOpenApi();
    }

    private static string? GetRawValue(FpChangeCsvRow r, string propertyName) => propertyName switch
    {
        nameof(FpChangeCsvRow.Contrato) => r.Contrato,
        nameof(FpChangeCsvRow.Producto) => r.Producto,
        nameof(FpChangeCsvRow.PlanProducto) => r.PlanProducto,
        nameof(FpChangeCsvRow.NroDocum) => r.NroDocum,
        nameof(FpChangeCsvRow.TipoDocum) => r.TipoDocum,
        nameof(FpChangeCsvRow.IdAgteNuevo) => r.IdAgteNuevo,
        nameof(FpChangeCsvRow.IdAgte) => r.IdAgte,
        nameof(FpChangeCsvRow.SubGrupoFp) => r.SubGrupoFp,
        nameof(FpChangeCsvRow.MotivoCambio) => r.MotivoCambio,
        _ => null
    };
    
    public sealed record ValidationResponseDto(int TotalFilas, int Errores, string CorrelationId, IEnumerable<RowError> Detalle)
    {
        public static ValidationResponseDto From(int total, List<RowError> errs, string cid)
            => new(total, errs.Count, cid, errs);
        public static ValidationResponseDto Fail(string message, string cid)
            => new(0, 1, cid, new[] { new RowError(0, "_archivo", message, null) });
    }

    public sealed record RowError(int Linea, string Campo, string Mensaje, string? Valor);
}