using PokerLogViewer.Models;
using System.IO;
using System.Text.Json;

namespace PokerLogViewer.Services;

public class JsonHandParser : IJsonHandParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyList<PokerHand> ParseFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}", filePath);

        var json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<PokerHand>();

        var dtos = JsonSerializer.Deserialize<List<JsonHandDto>>(json, JsonOptions);

        if (dtos is null || dtos.Count == 0)
            return Array.Empty<PokerHand>();

        return dtos.Select(MapToPokerHand).ToList();
    }

    private static PokerHand MapToPokerHand(JsonHandDto dto)
    {
        return new PokerHand
        {
            HandID = dto.HandID,
            TableName = dto.TableName,
            Players = dto.Players ?? new List<string>(),
            Winners = dto.Winners ?? new List<string>(),
            WinAmount = dto.WinAmount
        };
    }
}