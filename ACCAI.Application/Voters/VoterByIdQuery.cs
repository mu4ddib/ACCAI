using MediatR; 
using ACCAI.Application.Dtos; 
namespace ACCAI.Application.Voters; 
public sealed record VoterByIdQuery(Guid Id) : IRequest<VoterDto?>;