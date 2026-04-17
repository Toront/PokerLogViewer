using PokerLogViewer.Models;
using System.Diagnostics;
using System.IO;

namespace PokerLogViewer.Services;

public class FolderScanService : IFolderScanService
{
    private readonly IJsonHandParser _parser;
    private Thread? _scanThread;
    private volatile bool _isStopped;

    public FolderScanService(IJsonHandParser parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public void ScanFolder(
        string folderPath,
        Action<IReadOnlyList<PokerHand>, int> onCompleted,
        Action<Exception> onError)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Folder path cannot be empty.", nameof(folderPath));

        if (onCompleted is null)
            throw new ArgumentNullException(nameof(onCompleted));

        if (onError is null)
            throw new ArgumentNullException(nameof(onError));

        _isStopped = false;

        _scanThread = new Thread(() => ScanFolderInternal(folderPath, onCompleted, onError))
        {
            IsBackground = true,
            Name = "FolderScanThread"
        };

        _scanThread.Start();
    }

    public void Stop()
    {
        _isStopped = true;
    }

    private void ScanFolderInternal(
        string folderPath,
        Action<IReadOnlyList<PokerHand>, int> onCompleted,
        Action<Exception> onError)
    {
        try
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Folder does not exist: {folderPath}");

            var allHands = new List<PokerHand>();
            var processedFiles = 0;

            var jsonFiles = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);

            foreach (var filePath in jsonFiles)
            {
                if (_isStopped)
                    break;

                try
                {
                    var parsedHands = _parser.ParseFile(filePath);
                    allHands.AddRange(parsedHands);
                    processedFiles++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FolderScanService] Failed to parse '{filePath}': {ex.Message}");
                }
            }

            onCompleted(allHands, processedFiles);
        }
        catch (Exception ex)
        {
            onError(ex);
        }
    }
}