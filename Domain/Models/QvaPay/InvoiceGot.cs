using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace Domain.Models.QvaPay;
public sealed class InvoiceGot
{
    [JsonProperty("amount")]
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("uuid")]
    [JsonPropertyName("uuid")]
    public Guid QvapayId { get; set; }


    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
}
