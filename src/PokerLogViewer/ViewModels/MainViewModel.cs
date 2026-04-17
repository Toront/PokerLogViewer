using PokerLogViewer.Commands;
using PokerLogViewer.Converters;
using PokerLogViewer.Models;
using PokerLogViewer.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
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

    private string _tableSearchText = string.Empty;
    private string _handSearchText = string.Empty;

    private PokerTable? _selectedTable;
    private PokerHand? _selectedHand;

    private readonly ICollectionView _tablesView;
    private readonly ICollectionView _handsView;

    public MainViewModel()
    {
        _dispatcher = System.Windows.Application.Current.Dispatcher;
        _folderScanService = new FolderScanService(new JsonHandParser());
        _dialogService = new DialogService();

        SelectFolderCommand = new RelayCommand(SelectFolder);
        StartScanCommand = new RelayCommand(StartScan, CanStartScan);

        _tablesView = CollectionViewSource.GetDefaultView(Tables);
        _tablesView.Filter = FilterTables;

        _handsView = CollectionViewSource.GetDefaultView(SelectedTableHands);
        _handsView.Filter = FilterHands;

        Tables.CollectionChanged += OnTablesChanged;
        SelectedTableHands.CollectionChanged += OnSelectedTableHandsChanged;
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

    public string TableSearchText
    {
        get => _tableSearchText;
        set
        {
            if (SetProperty(ref _tableSearchText, value))
            {
                _tablesView.Refresh();
                UpdateSelectionAfterTableFilter();
            }
        }
    }

    public string HandSearchText
    {
        get => _handSearchText;
        set
        {
            if (SetProperty(ref _handSearchText, value))
                _handsView.Refresh();
        }
    }

    public ObservableCollection<PokerHand> Hands { get; } = new();
    public ObservableCollection<PokerTable> Tables { get; } = new();
    public ObservableCollection<PokerHand> SelectedTableHands { get; } = new();

    public ICollectionView TablesView => _tablesView;
    public ICollectionView HandsView => _handsView;

    public PokerTable? SelectedTable
    {
        get => _selectedTable;
        set
        {
            if (SetProperty(ref _selectedTable, value))
            {
                UpdateSelectedTableHands();
                OnPropertyChanged(nameof(HasSelectedTable));
                OnPropertyChanged(nameof(HasHands));
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
                OnPropertyChanged(nameof(HasSelectedHand));
        }
    }

    public bool HasSelectedTable => SelectedTable is not null;
    public bool HasTables => Tables.Count > 0;
    public bool HasHands => SelectedTableHands.Count > 0;
    public bool HasSelectedHand => SelectedHand is not null;

    public RelayCommand SelectFolderCommand { get; }
    public RelayCommand StartScanCommand { get; }

    private void OnTablesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasTables));
    }

    private void OnSelectedTableHandsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasHands));
        _handsView.Refresh();
    }

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
        _tablesView.Refresh();

        if (Tables.Count > 0)
            SelectedTable = Tables[0];
    }

    private void UpdateSelectedTableHands()
    {
        SelectedTableHands.Clear();

        if (SelectedTable is null)
        {
            SelectedHand = null;
            _handsView.Refresh();
            OnPropertyChanged(nameof(HasHands));
            OnPropertyChanged(nameof(HasSelectedHand));
            return;
        }

        foreach (var hand in SelectedTable.Hands)
            SelectedTableHands.Add(hand);

        SelectedHand = null;
        _handsView.Refresh();

        OnPropertyChanged(nameof(HasHands));
        OnPropertyChanged(nameof(HasSelectedHand));
    }

    private void UpdateSelectionAfterTableFilter()
    {
        if (SelectedTable is null)
            return;

        if (!FilterTables(SelectedTable))
            SelectedTable = Tables.FirstOrDefault();
    }

    private bool FilterTables(object obj)
    {
        if (obj is not PokerTable table)
            return false;

        var search = TableSearchText?.Trim();

        if (string.IsNullOrWhiteSpace(search))
            return true;

        return table.TableName.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private bool FilterHands(object obj)
    {
        if (obj is not PokerHand hand)
            return false;

        var search = HandSearchText?.Trim();

        if (string.IsNullOrWhiteSpace(search))
            return true;

        return hand.HandID.ToString().Contains(search, StringComparison.OrdinalIgnoreCase);
    }
}