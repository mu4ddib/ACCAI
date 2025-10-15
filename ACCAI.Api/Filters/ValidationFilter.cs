using FluentValidation;
namespace ACCAI.Api.Filters;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator=context.HttpContext.RequestServices.GetService<IValidator<T>>(); 
        if(validator is null) return await next(context); 
        var arg=context.Arguments.FirstOrDefault(a=>a is T) as T; if(arg is null) 
            return Results.BadRequest(new { error="Invalid request payload."}); 
        var result=await validator.ValidateAsync(arg);
        if (!result.IsValid) {
            var problem=new Dictionary<string,string[]>(StringComparer.OrdinalIgnoreCase); 
            foreach(var e in result.Errors.GroupBy(e=>e.PropertyName)) problem[e.Key]=e.Select(x=>x.ErrorMessage).ToArray(); 
            return Results.ValidationProblem(problem);
        } return await next(context);
    }
}