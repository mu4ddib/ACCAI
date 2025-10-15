using MediatR; 
using ACCAI.Application.Dtos; 
using ACCAI.Domain.Ports; 
namespace ACCAI.Application.Voters;

public sealed class VoterByIdQueryHandler : IRequestHandler<VoterByIdQuery, VoterDto?>
{
    private readonly IVoterRepository _repo; public VoterByIdQueryHandler(IVoterRepository repo)=>_repo=repo; public async Task<VoterDto?> Handle(VoterByIdQuery request, CancellationToken ct){ var v=await _repo.GetByIdAsync(request.Id, ct); return v is null? null : new VoterDto(v.Id, v.Nid, v.Origin, v.DateOfBirth);}
}