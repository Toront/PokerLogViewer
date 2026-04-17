using System.Text.Json.Serialization;

namespace PokerLogViewer.Models;

public class JsonHandDto
{
    [JsonPropertyName("HandID")]
    public long HandID { get; set; }

    [JsonPropertyName("TableName")]
    public string TableName { get; set; } = string.Empty;

    [JsonPropertyName("Players")]
    public List<string> Players { get; set; } = new();

    [JsonPropertyName("Winners")]
    public List<string> Winners { get; set; } = new();

    [JsonPropertyName("WinAmount")]
    public string WinAmount { get; set; } = string.Empty;
}