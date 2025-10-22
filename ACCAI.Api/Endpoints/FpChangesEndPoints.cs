using ACCAI.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ACCAI.Application.FpChanges;

namespace ACCAI.Api.Endpoints;

/// <summary>
/// 
/// </summary>
public static class FpChangesEndpoints
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/fp-changes").WithTags("FP Changes");

        group.MapPost("/upload", async Task<IResult> (
                [FromForm] IFormFile file,
                IMediator mediator,
                CancellationToken ct
            ) =>
            {
                if (file is null || file.Length == 0)
                {
                    var cid = Guid.NewGuid().ToString("N");
                    return Results.BadRequest(ValidationResponseDto.Fail("Archivo vacío.", cid));
                }
                
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, ct);
                ms.Position = 0;

                var res = await mediator.Send(
                    new ValidateFpChangesCsvCommand(ms, file.Length, file.FileName), ct);

                return res.Errores == 0 ? Results.Ok(res) : Results.BadRequest(res);
            })
            .DisableAntiforgery()                       // para pruebas desde Postman
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ValidationResponseDto>(StatusCodes.Status200OK)
            .Produces<ValidationResponseDto>(StatusCodes.Status400BadRequest)
            .WithName("ValidateFpChangeCsv")
            .WithOpenApi();
    }
}