using System.IO;
using System.Text.Json;
using PokerLogViewer.Models;

namespace PokerLogViewer.Services;

public class JsonHandParser : IJsonHandParser
{
    public List<PokerHand> ParseFile(string filePath)
    {
        var json = File.ReadAllText(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<List<PokerHand>>(json, options);
        return result ?? new List<PokerHand>();
    }
}