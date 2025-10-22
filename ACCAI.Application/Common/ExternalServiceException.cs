namespace ACCAI.Application.Common;

public sealed class ExternalServiceException : Exception
{
    public string Code { get; }
    public string? Target { get; }
    public int? StatusCode { get; }
    public ExternalServiceException(string code, string message, string? target = null, int? statusCode = null, Exception? inner = null)
        : base(message, inner)
    {
        Code = code; Target = target; StatusCode = statusCode;
    }
}