using PokerLogViewer.Commands;
using PokerLogViewer.Models;
using PokerLogViewer.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace PokerLogViewer.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IFolderScanService _folderScanService;
    private readonly IDialogService _dialogService;
    private readonly Dispatcher _dispatcher;

    private string _statusText = "Ready";
    private string _selectedFolderPath = string.Empty;
    private bool _isScanning;

    private PokerTable? _selectedTable;
    private PokerHand? _selectedHand;

    public MainViewModel()
    {
        _dispatcher = System.Windows.Application.Current.Dispatcher;
        _folderScanService = new FolderScanService(new JsonHandParser());
        _dialogService = new DialogService();

        SelectFolderCommand = new RelayCommand(SelectFolder);
        StartScanCommand = new RelayCommand(StartScan, CanStartScan);

        Tables.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(HasTables));
            OnPropertyChanged(nameof(EmptyTablesMessageVisibility));
        };

        SelectedTableHands.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(HasHands));
            OnPropertyChanged(nameof(EmptyHandsMessageVisibility));
        };
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
    public ObservableCollection<PokerTable> Tables { get; } = new();
    public ObservableCollection<PokerHand> SelectedTableHands { get; } = new();

    public bool HasTables => Tables.Count > 0;
    public bool HasHands => SelectedTableHands.Count > 0;
    public bool HasSelectedHand => SelectedHand is not null;

    public PokerTable? SelectedTable
    {
        get => _selectedTable;
        set
        {
            if (SetProperty(ref _selectedTable, value))
            {
                UpdateSelectedTableHands();
                OnPropertyChanged(nameof(HasSelectedTable));
                OnPropertyChanged(nameof(EmptyHandsMessageVisibility));
                OnPropertyChanged(nameof(HasSelectedHand));
            }
        }
    }

    public PokerHand? SelectedHand
    {
        get => _selectedHand;
        set
        {
            if (SetProperty(ref _selectedHand, value))
            {
                OnPropertyChanged(nameof(HasSelectedHand));
                OnPropertyChanged(nameof(EmptyDetailsMessageVisibility));
            }
        }
    }

    public bool HasSelectedTable => SelectedTable is not null;

    public Visibility EmptyTablesMessageVisibility => HasTables ? Visibility.Collapsed : Visibility.Visible;
    public Visibility EmptyHandsMessageVisibility => HasHands ? Visibility.Collapsed : Visibility.Visible;
    public Visibility EmptyDetailsMessageVisibility => HasSelectedHand ? Visibility.Collapsed : Visibility.Visible;

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
        Tables.Clear();
        SelectedTableHands.Clear();
        SelectedTable = null;
        SelectedHand = null;

        OnPropertyChanged(nameof(HasTables));
        OnPropertyChanged(nameof(HasHands));
        OnPropertyChanged(nameof(HasSelectedHand));
        OnPropertyChanged(nameof(EmptyTablesMessageVisibility));
        OnPropertyChanged(nameof(EmptyHandsMessageVisibility));
        OnPropertyChanged(nameof(EmptyDetailsMessageVisibility));

        IsScanning = true;
        StatusText = "Сканирование...";

        _folderScanService.ScanFolder(
            SelectedFolderPath,
            onCompleted: (hands, processedFiles) =>
            {
                _dispatcher.Invoke(() =>
                {
                    Hands.Clear();
                    foreach (var hand in hands)
                        Hands.Add(hand);

                    BuildTables();

                    StatusText = $"✅ Готово ({processedFiles} файлов обработано)";
                    IsScanning = false;
                });
            },
            onError: ex =>
            {
                _dispatcher.Invoke(() =>
                {
                    StatusText = $"❌ Ошибка: {ex.Message}";
                    IsScanning = false;
                });
            });
    }

    private void BuildTables()
    {
        Tables.Clear();

        var grouped = Hands
            .GroupBy(h => h.TableName)
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            Tables.Add(new PokerTable
            {
                TableName = group.Key,
                Hands = new ObservableCollection<PokerHand>(group.OrderBy(h => h.HandID))
            });
        }

        OnPropertyChanged(nameof(HasTables));
        OnPropertyChanged(nameof(EmptyTablesMessageVisibility));

        if (Tables.Count > 0)
            SelectedTable = Tables[0];
    }

    private void UpdateSelectedTableHands()
    {
        SelectedTableHands.Clear();

        if (SelectedTable is null)
            return;

        foreach (var hand in SelectedTable.Hands)
            SelectedTableHands.Add(hand);

        OnPropertyChanged(nameof(HasHands));
        OnPropertyChanged(nameof(EmptyHandsMessageVisibility));

        SelectedHand = null;
    }
}