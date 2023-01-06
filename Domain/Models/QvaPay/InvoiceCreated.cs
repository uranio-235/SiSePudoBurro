using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace Domain.Models.QvaPay;
public sealed class InvoiceCreated
{
    [JsonProperty("transation_uuid")]
    [JsonPropertyName("transation_uuid")]
    public Guid QvapayId { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonProperty("amount")]
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("remote_id")]
    [JsonPropertyName("remote_id")]
    public string RemoteId { get; set; }
}
