namespace PokerLogViewer.Services;

public class DialogService : IDialogService
{
    public string? SelectFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select folder with poker logs",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        return dialog.ShowDialog() == DialogResult.OK
            ? dialog.SelectedPath
            : null;
    }
}