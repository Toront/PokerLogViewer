using PokerLogViewer.Models;

namespace PokerLogViewer.Services;

public interface IJsonHandParser
{
    List<PokerHand> ParseFile(string filePath);
}