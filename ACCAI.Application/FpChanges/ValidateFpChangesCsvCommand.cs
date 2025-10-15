using ACCAI.Application.Dtos;
using MediatR;

namespace ACCAI.Application.FpChanges;

public sealed record ValidateFpChangesCsvCommand(Stream FileStream, long FileLength)
    : IRequest<ValidationResponseDto>;