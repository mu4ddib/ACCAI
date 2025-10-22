namespace ACCAI.Application.Common;

public sealed record ApiResult<T>(string CorrelationId, bool Ok, T? Data, IReadOnlyList<ApiError> Errors)
{
    public static ApiResult<T> Success(string cid, T data) => new(cid, true, data, Array.Empty<ApiError>());
    public static ApiResult<T> Fail(string cid, params ApiError[] errors) => new(cid, false, default, errors);
}