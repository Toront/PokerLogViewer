using System.Collections.Generic;

namespace PokerLogViewer.Models;

public class PokerHand
{
    public long HandID { get; set; }
    public string TableName { get; set; } = string.Empty;
    public List<string> Players { get; set; } = new();
    public List<string> Winners { get; set; } = new();
    public string WinAmount { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"#{HandID} | {TableName} | {WinAmount}";
    }
}