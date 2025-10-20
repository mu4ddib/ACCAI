namespace ACCAI.Domain.ReadModels;

public sealed class AgentChangeItem
{
    public int OldAgentId { get; set; }
    public int NewAgentId { get; set; }
    public int ContractNumber { get; set; }
}
