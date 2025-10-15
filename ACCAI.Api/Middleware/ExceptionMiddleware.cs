using System.Net; 
using ACCAI.Domain.Exceptions; 
namespace ACCAI.Api.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next; 
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next=next; _logger=logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CoreBusinessException ex)
        {
            _logger.LogWarning(ex, "Business error");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error"); 
            context.Response.StatusCode=(int)HttpStatusCode.InternalServerError; 
            await context.Response.WriteAsJsonAsync(new { error = "Unexpected error", traceId = context.TraceIdentifier });
        }
    }
}