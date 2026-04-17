using System.Collections.ObjectModel;
using System.Windows.Threading;
using PokerLogViewer.Commands;
using PokerLogViewer.Models;
using PokerLogViewer.Services;

namespace PokerLogViewer.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IFolderScanService _folderScanService;
    private readonly IDialogService _dialogService;
    private readonly Dispatcher _dispatcher;

    private string _statusText = "Ready";
    private string _selectedFolderPath = string.Empty;
    private bool _isScanning;

    public MainViewModel()
    {
        _dispatcher = System.Windows.Application.Current.Dispatcher;
        _folderScanService = new FolderScanService(new JsonHandParser());
        _dialogService = new DialogService();

        SelectFolderCommand = new RelayCommand(SelectFolder);
        StartScanCommand = new RelayCommand(StartScan, CanStartScan);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string SelectedFolderPath
    {
        get => _selectedFolderPath;
        set
        {
            if (SetProperty(ref _selectedFolderPath, value))
                StartScanCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            if (SetProperty(ref _isScanning, value))
                StartScanCommand.RaiseCanExecuteChanged();
        }
    }

    public ObservableCollection<PokerHand> Hands { get; } = new();

    public RelayCommand SelectFolderCommand { get; }
    public RelayCommand StartScanCommand { get; }

    private void SelectFolder()
    {
        var folder = _dialogService.SelectFolder();

        if (string.IsNullOrWhiteSpace(folder))
            return;

        SelectedFolderPath = folder;
        StatusText = $"Selected folder: {folder}";
    }

    private bool CanStartScan()
    {
        return !string.IsNullOrWhiteSpace(SelectedFolderPath) && !IsScanning;
    }

    private void StartScan()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath))
            return;

        Hands.Clear();
        IsScanning = true;
        StatusText = "Scanning...";

        _folderScanService.ScanFolder(
            SelectedFolderPath,
            onCompleted: hands =>
            {
                _dispatcher.Invoke(() =>
                {
                    Hands.Clear();
                    foreach (var hand in hands)
                        Hands.Add(hand);

                    StatusText = $"Done. Found {hands.Count} hands.";
                    IsScanning = false;
                });
            },
            onError: ex =>
            {
                _dispatcher.Invoke(() =>
                {
                    StatusText = $"Error: {ex.Message}";
                    IsScanning = false;
                });
            });
    }
}