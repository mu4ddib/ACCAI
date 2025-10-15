using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using ACCAI.Application.Voters;
using ACCAI.Application.Dtos;
using ACCAI.Domain.ReadModels;
using Microsoft.AspNetCore.OpenApi;

namespace ACCAI.Api.Endpoints;
public static class  VoterEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/voters");
        group.MapPost("/", async Task<Results<Created<VoterDto>, ValidationProblem>> (IMediator mediator, VoterRegisterCommand cmd) =>
        {
            var dto = await mediator.Send(cmd);
            return TypedResults.Created($"/api/voters/{dto.Id}", dto);
        })
        .AddEndpointFilter(new Filters.ValidationFilter<VoterRegisterCommand>())
        .WithName("RegisterVoter")
        .WithOpenApi();
        group.MapGet("/{id:guid}", async Task<Results<Ok<VoterDto>, NotFound>> (Guid id, IMediator mediator) =>
        {
            var dto = await mediator.Send(new VoterByIdQuery(id));
            return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
        })
        .WithName("GetVoterById")
        .WithOpenApi();
        group.MapGet("/", async Task<Ok<IEnumerable<VoterSimpleDto>>> (IMediator mediator) =>
        {
            var rows = await mediator.Send(new VoterSimpleListQuery());
            return TypedResults.Ok(rows);
        })
        .WithName("ListVotersSimple")
        .WithOpenApi();
    }
}
