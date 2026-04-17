using System.Collections.ObjectModel;

namespace PokerLogViewer.Models;

public class PokerTable
{
    public string TableName { get; set; } = string.Empty;

    public ObservableCollection<PokerHand> Hands { get; set; } = new();

    public override string ToString() => TableName;
}