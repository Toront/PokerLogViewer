using PokerLogViewer.Models;

namespace PokerLogViewer.Services;

public interface IJsonHandParser
{
    IReadOnlyList<PokerHand> ParseFile(string filePath);
}