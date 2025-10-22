namespace ACCAI.Application.Common;

public sealed record ApiError(string Code, string Message, string? Target = null, int? Line = null, string? Field = null);