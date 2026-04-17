namespace PokerLogViewer.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _statusText = "Ready";

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
}