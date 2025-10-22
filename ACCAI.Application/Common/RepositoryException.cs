namespace ACCAI.Application.Common;

public sealed class RepositoryException : Exception
{
    public string Code { get; }
    public string? Target { get; }
    public RepositoryException(string code, string message, string? target = null, Exception? inner = null)
        : base(message, inner)
    {
        Code = code; Target = target;
    }
}