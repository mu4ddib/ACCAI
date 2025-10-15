namespace ACCAI.Domain.Exceptions;

public sealed class WrongCountryException : CoreBusinessException
{
    public WrongCountryException() : base("Voter must be from Colombia.") {}
}