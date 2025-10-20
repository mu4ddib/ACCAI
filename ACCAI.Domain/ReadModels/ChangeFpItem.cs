using System.Text.Json.Serialization;

namespace ACCAI.Domain.ReadModels;

public class ChangeFpItem
{
    [JsonPropertyName("idAgenteAnterior")]
    public int PreviousAgentId { get; set; }

    [JsonPropertyName("idAgenteNuevo")]
    public int NewAgentId { get; set; }

    [JsonPropertyName("producto")]
    public string Product { get; set; }

    [JsonPropertyName("planProducto")]
    public string ProductPlan { get; set; }

    [JsonPropertyName("contrato")]
    public int Contract { get; set; }
}
