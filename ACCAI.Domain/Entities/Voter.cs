using ACCAI.Domain.Exceptions;
namespace ACCAI.Domain.Entities;
public sealed class Voter : DomainEntity
{
    public string Nid { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string Origin { get; private set; }
    private Voter() { Nid = string.Empty; Origin = string.Empty; }
    public Voter(string nid, DateTime dateOfBirth, string origin)
    {
        if (string.IsNullOrWhiteSpace(nid) || nid.Length < 8)
            throw new CoreBusinessException("NID requires at least 8 characters.");
        Nid = nid; DateOfBirth = dateOfBirth; Origin = origin;
        if (IsUnderAge()) throw new UnderAgeException();
        if (!CanVoteBasedOnLocation()) throw new WrongCountryException();
    }
    public bool IsUnderAge() => DateOfBirth > DateTime.UtcNow.AddYears(-18);
    public bool CanVoteBasedOnLocation() => string.Equals(Origin, "Colombia", StringComparison.OrdinalIgnoreCase);
}
