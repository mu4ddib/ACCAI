namespace ACCAI.Application.FpChanges;

public interface IFpChangeCsvParser
{
    Task<CsvParseOutput<FpChangeCsvRow>> ParseAsync(Stream csvStream, CancellationToken ct = default);
}

public sealed record CsvParseOutput<T>(IReadOnlyList<string> Header, IReadOnlyList<T> Rows);