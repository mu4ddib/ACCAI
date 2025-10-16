using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ACCAI.Application.FpChanges;

namespace ACCAI.Infrastructure.Parsers;

public sealed class FpChangeCsvParser : IFpChangeCsvParser
{
    public async Task<CsvParseOutput<FpChangeCsvRow>> ParseAsync(Stream csvStream, CancellationToken ct = default)
    {
        if (csvStream.CanSeek) csvStream.Position = 0;
        
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

        var header = Array.Empty<string>();
        var rows = new List<FpChangeCsvRow>();
        
        using var reader = new StreamReader(csvStream, leaveOpen: true);
        using var csv = new CsvReader(reader, cfg);

        if (await csv.ReadAsync())
        {
            csv.ReadHeader(); 
            header = csv.HeaderRecord ?? [];
        }

        while (await csv.ReadAsync())
        {
            var r = new FpChangeCsvRow(
                        
                (csv.GetField("Apellidos") ?? "").Trim(),
                (csv.GetField("Nombres") ?? "").Trim(),
                (csv.GetField("NroDocum") ?? "").Trim(),
                (csv.GetField("TipoDocum") ?? "").Trim(),
                (csv.GetField("Producto") ?? "").Trim(),
                (csv.GetField("PlanProducto") ?? "").Trim(),
                (csv.GetField("Contrato") ?? "").Trim(),
                (csv.GetField("Empresa") ?? "").Trim(),
                (csv.GetField("Segmento") ?? "").Trim(),
                (csv.GetField("Ciudad") ?? "").Trim(),
                (csv.GetField("IdAgte") ?? "").Trim(),
                (csv.GetField("IdAgteNuevo") ?? "").Trim(),
                (csv.GetField("NombreAgteNuevo") ?? "").Trim(),
                (csv.GetField("SubGrupoFp") ?? "").Trim(),
                (csv.GetField("descripcion") ?? "").Trim()
                
            );
            rows.Add(r);
        }

        return new CsvParseOutput<FpChangeCsvRow>(header, rows);
    }
}
