namespace ACCAI.Domain.Ports.ExternalServices;

public interface IChangeFpFactory
{
    IChangeFpService GetService(string product);
}