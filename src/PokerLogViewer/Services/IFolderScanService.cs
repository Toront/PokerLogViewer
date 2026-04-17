using PokerLogViewer.Models;

namespace PokerLogViewer.Services;

public interface IFolderScanService
{
    void ScanFolder(
        string folderPath,
        Action<IReadOnlyList<PokerHand>> onCompleted,
        Action<Exception> onError);

    void Stop();
}