using System.Collections.Concurrent;
using ACCAI.Application.Dtos;
using ACCAI.Domain.Ports;
using ACCAI.Domain.Ports.ExternalServices;
using ACCAI.Domain.ReadModels;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ACCAI.Application.Common;

namespace ACCAI.Application.FpChanges;

public sealed class ValidateFpChangesCsvCommandHandler
    : IRequestHandler<ValidateFpChangesCsvCommand, ValidationResponseDto>
{
    private readonly IFpChangeCsvParser _parser;
    private readonly IChangeFpFactory _factory;
    private readonly IValidator<FpChangeCsvRow> _rowValidator;
    private readonly ILogger<ValidateFpChangesCsvCommandHandler> _logger;
    private readonly IContractsRepository _contractsRepository;

    private static readonly string[] ExpectedHeaders =
    [
        "Apellidos","Nombres","NroDocum","TipoDocum","Producto","PlanProducto","Contrato","Empresa","Segmento",
        "Ciudad","IdAgte","NombreAgte","IdAgteNuevo","NombreAgteNuevo","SubGrupoFp","descripcion"
    ];

    public ValidateFpChangesCsvCommandHandler(
        IFpChangeCsvParser parser,
        IValidator<FpChangeCsvRow> rowValidator,
        ILogger<ValidateFpChangesCsvCommandHandler> logger,
        IChangeFpFactory factory,
        IContractsRepository contractsRepository)
    {
        _parser = parser;
        _rowValidator = rowValidator;
        _logger = logger;
        _factory = factory;
        _contractsRepository = contractsRepository;
    }

    public async Task<ValidationResponseDto> Handle(ValidateFpChangesCsvCommand request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        var errors = new List<RowError>();

        if (!ValidateFile(request, out var validationError))
            return ValidationResponseDto.Fail(validationError!, correlationId);

        var parsed = await _parser.ParseAsync(request.FileStream, ct);

        if (!ValidateHeader(parsed.Header, out var headerError))
            return ValidationResponseDto.Fail(headerError!, correlationId);

        if (parsed.Rows.Count > 50)
            return ValidationResponseDto.Fail("Máximo 50 registros por archivo.", correlationId);

        await ValidateRowsAsync(parsed.Rows, errors, correlationId, ct);

        var rowIndexMap = parsed.Rows                
            .Select((r, i) => new { r, line = i + 2 })   
            .ToDictionary(x => x.r, x => x.line);
        
        await ProcessFpChangesAsync(parsed.Rows, rowIndexMap, errors, correlationId, ct);

        return ValidationResponseDto.From(parsed.Rows.Count, errors, correlationId);
    }

    /// <summary>
    /// Validates the uploaded file for length constraints.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private static bool ValidateFile(ValidateFpChangesCsvCommand request, out string? message)
    {
        message = null;
        if (request.FileLength <= 0)
        {
            message = "Archivo vacío.";
            return false;
        }

        if (request.FileLength > 1_000_000)
        {
            message = "Tamaño máximo permitido: 1MB.";
            return false;
        }

        return true;
    }

    private static bool ValidateHeader(IReadOnlyList<string> header, out string? message)
    {
        message = null;
        if (header.Count != ExpectedHeaders.Length || !header.SequenceEqual(ExpectedHeaders))
        {
            message = $"Cabeceras inválidas. Esperado: {string.Join(",", ExpectedHeaders)}. Recibido: {string.Join(",", header)}";
            return false;
        }
        return true;
    }

    private async Task ValidateRowsAsync(IEnumerable<FpChangeCsvRow> rows, List<RowError> errors, string correlationId, CancellationToken ct)
    {
        var index = 2; 
        foreach (var row in rows)
        {
            var result = await _rowValidator.ValidateAsync(row, ct);
            if (!result.IsValid)
            {
                foreach (var e in result.Errors)
                {
                    var error = new RowError(index, e.PropertyName, e.ErrorMessage, GetRaw(row, e.PropertyName));
                    errors.Add(error);
                    _logger.LogWarning("Validation error {@Error} CorrelationId={CorrelationId}", error, correlationId);
                }
            }
            index++;
        }
    }
    /// <summary>
    /// Processes the valid rows by grouping them by product and sending changes to external services.
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="errors"></param>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>

    private async Task ProcessFpChangesAsync(
        IEnumerable<FpChangeCsvRow> rows,
        IReadOnlyDictionary<FpChangeCsvRow,int> rowIndexMap,  // ← NUEVO
        List<RowError> errors,
        string correlationId,
        CancellationToken ct)
    {
        var grouped = rows.GroupBy(r => r.Producto);
        var tasks = grouped.Select(g => HandleProductGroupAsync(g.Key, g, rowIndexMap, errors, correlationId, ct));
        await Task.WhenAll(tasks);
    }

    private async Task HandleProductGroupAsync(
    string product,
    IEnumerable<FpChangeCsvRow> group,
    IReadOnlyDictionary<FpChangeCsvRow,int> rowIndexMap,  // ← NUEVO
    List<RowError> errors,
    string correlationId,
    CancellationToken ct)
{
    try
    {
        var service = _factory.GetService(product);
        var changes = new ConcurrentBag<ChangeFpItem>();

        var tasks = group.Select(async row =>
        {
            try
            {
                var change = MapToChange(row);

                // El servicio ahora LANZA excepciones canónicas si hay error
                await service.SendChangeAsync(change);

                changes.Add(change);
            }
            catch (Exception ex)
            {
                // Mapeo compacto: code + message
                var apiErr = ex.ToApiError(target: $"external:{product.ToLower()}");
                AddError(errors, rowIndexMap[row], nameof(row.Producto),
                         $"{apiErr.Code}: {apiErr.Message}", row.Contrato, correlationId);
            }
        });

        await Task.WhenAll(tasks);

        if (changes.Any())
        {
            try
            {
                // Asegúrate del nombre correcto del método en tu repo:
                var affected = await _contractsRepository.UpdateContractsAgentAsync(changes.ToList(), ct);
                _logger.LogInformation("Product {Product}: {Count} contracts updated in DB. CorrelationId={CorrelationId}",
                    product, affected, correlationId);
            }
            catch (Exception ex)
            {
                var apiErr = ex.ToApiError(target: "db:contratos");
                foreach (var row in group) // puedes optar por 1 solo error general si prefieres
                    AddError(errors, rowIndexMap[row], "_db",
                             $"{apiErr.Code}: {apiErr.Message}", row.Contrato, correlationId);
            }
        }
    }
    catch (Exception ex) // error resolviendo el servicio del producto
    {
        var apiErr = ex.ToApiError(target: $"external:{product.ToLower()}",
                                   codeOverride: "external.service_resolution",
                                   messageOverride: $"No se pudo resolver el servicio {product}");
        foreach (var row in group)
            AddError(errors, rowIndexMap[row], nameof(row.Producto),
                     $"{apiErr.Code}: {apiErr.Message}", row.Contrato, correlationId);
    }
}
    
    /// <summary>
    /// Maps a CSV row to a ChangeFpItem
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>

    private static ChangeFpItem MapToChange(FpChangeCsvRow row) => new()
    {
        PreviousAgentId = int.TryParse(row.IdAgte, out var prev) ? prev : 0,
        NewAgentId = int.TryParse(row.IdAgteNuevo, out var next) ? next : 0,
        Product = row.Producto,
        ProductPlan = row.PlanProducto,
        Contract = int.TryParse(row.Contrato, out var contract) ? contract : 0
    };

    private void AddError(List<RowError> errors, int line, string field, string message, string? value, string correlationId)
    {
        var error = new RowError(line, field, message, value);
        errors.Add(error);
        _logger.LogWarning("Processing error {@Error} CorrelationId={CorrelationId}", error, correlationId);
    }

    private static string? GetRaw(FpChangeCsvRow r, string prop) => prop switch
    {
        nameof(FpChangeCsvRow.Apellidos) => r.Apellidos,
        nameof(FpChangeCsvRow.Nombres) => r.Nombres,
        nameof(FpChangeCsvRow.NroDocum) => r.NroDocum,
        nameof(FpChangeCsvRow.TipoDocum) => r.TipoDocum,
        nameof(FpChangeCsvRow.Producto) => r.Producto,
        nameof(FpChangeCsvRow.PlanProducto) => r.PlanProducto,
        nameof(FpChangeCsvRow.Contrato) => r.Contrato,
        nameof(FpChangeCsvRow.Empresa) => r.Empresa,
        nameof(FpChangeCsvRow.Segmento) => r.Segmento,
        nameof(FpChangeCsvRow.Ciudad) => r.Ciudad,
        nameof(FpChangeCsvRow.IdAgte) => r.IdAgte,
        nameof(FpChangeCsvRow.IdAgteNuevo) => r.IdAgteNuevo,
        nameof(FpChangeCsvRow.SubGrupoFp) => r.SubGrupoFp,
        nameof(FpChangeCsvRow.descripcion) => r.descripcion,
        _ => null
    };
}
