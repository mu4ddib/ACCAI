using System.Collections.Concurrent;
using System.IO;
using ACCAI.Application.Common;
using ACCAI.Application.Dtos;
using ACCAI.Domain.Ports;
using ACCAI.Domain.Ports.ExternalServices;
using ACCAI.Domain.ReadModels;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ACCAI.Application.FpChanges;

public sealed class ValidateFpChangesCsvCommandHandler
    : IRequestHandler<ValidateFpChangesCsvCommand, ValidationResponseDto>
{
    private readonly IFpChangeCsvParser _parser;
    private readonly IChangeFpFactory _factory;
    private readonly IValidator<FpChangeCsvRow> _rowValidator;
    private readonly ILogger<ValidateFpChangesCsvCommandHandler> _logger;
    private readonly IContractsRepository _contractsRepository;
    
    private const string V001_ExtensionCsv = "V001_EXTENSION_CSV";
    private const string V002_FileSize     = "V002_FILE_SIZE";
    private const string V003_Headers      = "V003_HEADERS";
    private const string V004_MaxRows      = "V004_MAX_ROWS";
    private const string V005_RowRules     = "V005_ROW_VALIDATIONS";
    
    private static readonly HashSet<string> AllowedProducts =
        new(StringComparer.OrdinalIgnoreCase) { "ACCAI" };

    private static bool IsAllowedProduct(string? p) =>
        !string.IsNullOrWhiteSpace(p) && AllowedProducts.Contains(p);


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

        _logger.LogInformation("Start CSV validation CorrelationId={CorrelationId} FileName={FileName} Length={Length}",
            correlationId, request.FileName, request.FileLength);

        if (!ValidateFile(request, correlationId, out var validationError))
            return ValidationResponseDto.Fail(validationError!, correlationId);

        var parsed = await _parser.ParseAsync(request.FileStream, ct);
        _logger.LogInformation("Parsed CSV CorrelationId={CorrelationId} HeaderCount={HeaderCount} Rows={Rows}",
            correlationId, parsed.Header.Count, parsed.Rows.Count);

        if (!ValidateHeader(parsed.Header, correlationId, out var headerError))
            return ValidationResponseDto.Fail(headerError!, correlationId);

        if (parsed.Rows.Count > 50)
        {
            _logger.LogWarning("[{Code}] Max rows exceeded CorrelationId={CorrelationId} Max=50 Received={Rows}",
                V004_MaxRows, correlationId, parsed.Rows.Count);
            return ValidationResponseDto.Fail("Máximo 50 registros por archivo.", correlationId);
        }
        _logger.LogInformation("[{Code}] Max rows check passed CorrelationId={CorrelationId} Rows={Rows}",
            V004_MaxRows, correlationId, parsed.Rows.Count);

        await ValidateRowsAsync(parsed.Rows, errors, correlationId, ct);

        var rowIndexMap = parsed.Rows
            .Select((r, i) => new { r, line = i + 2 })
            .ToDictionary(x => x.r, x => x.line);
        
        var toProcess = new List<FpChangeCsvRow>();
        var skippedCount = 0;

        foreach (var row in parsed.Rows)
        {
            if (IsAllowedProduct(row.Producto))
            {
                toProcess.Add(row);
            }
            else
            {
                skippedCount++;
                _logger.LogWarning(
                    "Row skipped by product filter. Line={Line} Product={Product} Contract={Contract} CorrelationId={CorrelationId}",
                    rowIndexMap[row], row.Producto, row.Contrato, correlationId);
            }
        }
        
        _logger.LogInformation(
            "Product filter summary. Allowed={Allowed} Skipped={Skipped} AllowedProducts={AllowedList} CorrelationId={CorrelationId}",
            toProcess.Count, skippedCount, string.Join(",", AllowedProducts), correlationId);

        await ProcessFpChangesAsync(toProcess, rowIndexMap, errors, correlationId, ct);
        
        var result = ValidationResponseDto.From(parsed.Rows.Count, errors, correlationId);

        _logger.LogInformation("End CSV validation CorrelationId={CorrelationId} TotalRows={Total} Errors={Errors}",
            correlationId, result.TotalFilas, result.Errores);

        return result;
    }
    

    /// <summary>
    /// Valida extensión .csv (no .csv.xlsx) y tamaño (<= 1MB), logueando cada paso.
    /// </summary>
    private bool ValidateFile(ValidateFpChangesCsvCommand request, string correlationId, out string? message)
    {
        message = null;
        
        var ext = Path.GetExtension(request.FileName ?? string.Empty);
        var isCsv = string.Equals(ext, ".csv", StringComparison.OrdinalIgnoreCase);
        if (!isCsv)
        {
            _logger.LogWarning("[{Code}] Extension check failed CorrelationId={CorrelationId} FileName={FileName} Extension={Extension}",
                V001_ExtensionCsv, correlationId, request.FileName, ext);
            message = "El archivo debe tener extensión .csv.";
            return false;
        }
        _logger.LogInformation("[{Code}] Extension check passed CorrelationId={CorrelationId} FileName={FileName}",
            V001_ExtensionCsv, correlationId, request.FileName);
        
        switch (request.FileLength)
        {
            case <= 0:
                _logger.LogWarning("[{Code}] File empty CorrelationId={CorrelationId}", V002_FileSize, correlationId);
                message = "Archivo vacío.";
                return false;
            case > 1_000_000:
                _logger.LogWarning("[{Code}] File too large CorrelationId={CorrelationId} Max=1000000 Length={Length}",
                    V002_FileSize, correlationId, request.FileLength);
                message = "Tamaño máximo permitido: 1MB.";
                return false;
            default:
                _logger.LogInformation("[{Code}] File size check passed CorrelationId={CorrelationId} Length={Length}",
                    V002_FileSize, correlationId, request.FileLength);

                return true;
        }
    }

    private bool ValidateHeader(IReadOnlyList<string> header, string correlationId, out string? message)
    {
        message = null;

        if (header.Count != ExpectedHeaders.Length || !header.SequenceEqual(ExpectedHeaders))
        {
            _logger.LogWarning(
                "[{Code}] Header check failed CorrelationId={CorrelationId} ExpectedCount={ExpectedCount} ActualCount={ActualCount}",
                V003_Headers, correlationId, ExpectedHeaders.Length, header.Count);
            
            _logger.LogWarning("[{Code}] Expected={Expected} Received={Received} CorrelationId={CorrelationId}",
                V003_Headers,
                string.Join(",", ExpectedHeaders),
                string.Join(",", header),
                correlationId);

            message = $"Cabeceras inválidas. Esperado: {string.Join(",", ExpectedHeaders)}. Recibido: {string.Join(",", header)}";
            return false;
        }

        _logger.LogInformation("[{Code}] Header check passed CorrelationId={CorrelationId}", V003_Headers, correlationId);
        return true;
    }
    

    private async Task ValidateRowsAsync(IEnumerable<FpChangeCsvRow> rows, List<RowError> errors, string correlationId, CancellationToken ct)
    {
        var total = rows.Count();
        _logger.LogInformation("[{Code}] Row validations start CorrelationId={CorrelationId} Total={Total}",
            V005_RowRules, correlationId, total);

        var index = 2; // primera fila de datos
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

        _logger.LogInformation("[{Code}] Row validations end CorrelationId={CorrelationId} Errors={Errors}",
            V005_RowRules, correlationId, errors.Count);
    }
    

    private async Task ProcessFpChangesAsync(
        IEnumerable<FpChangeCsvRow> rows,
        IReadOnlyDictionary<FpChangeCsvRow,int> rowIndexMap,
        List<RowError> errors,
        string correlationId,
        CancellationToken ct)
    {
        rows = rows.Where(r => IsAllowedProduct(r.Producto));

        var grouped = rows.GroupBy(r => r.Producto);
        var tasks = grouped.Select(g => HandleProductGroupAsync(g.Key, g, rowIndexMap, errors, correlationId, ct));
        await Task.WhenAll(tasks);
    }


    private async Task HandleProductGroupAsync(
        string product,
        IEnumerable<FpChangeCsvRow> group,
        IReadOnlyDictionary<FpChangeCsvRow,int> rowIndexMap,
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
                    await service.SendChangeAsync(change); 
                    changes.Add(change);
                }
                catch (Exception ex)
                {
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
                    var affected = await _contractsRepository.UpdateContractsAgentAsync(changes.ToList(), ct);
                    _logger.LogInformation("Product {Product}: {Count} contracts updated in DB. CorrelationId={CorrelationId}",
                        product, affected, correlationId);
                }
                catch (Exception ex)
                {
                    var apiErr = ex.ToApiError(target: "db:contratos");
                    foreach (var row in group)
                        AddError(errors, rowIndexMap[row], "_db",
                                 $"{apiErr.Code}: {apiErr.Message}", row.Contrato, correlationId);
                }
            }
        }
        catch (Exception ex)
        {
            var apiErr = ex.ToApiError(
                target: $"external:{product.ToLower()}",
                codeOverride: "external.service_resolution",
                messageOverride: $"No se pudo resolver el servicio {product}");

            foreach (var row in group)
                AddError(errors, rowIndexMap[row], nameof(row.Producto),
                         $"{apiErr.Code}: {apiErr.Message}", row.Contrato, correlationId);
        }
    }
    

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
