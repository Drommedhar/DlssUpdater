using System.Collections.Concurrent;
using DlssUpdater.Defines;
using DlssUpdater.Singletons;

namespace DLSSUpdater.Singletons;

/// <summary>
///     Custom class to check changes on specific files, as FileSystemWatcher is not working as I need it.
/// </summary>
public class AsyncFileWatcher
{
    private readonly ConcurrentQueue<GameInfo> _queueFiles = new();
    private readonly Timer? _timer;
    private readonly DllUpdater _updater;

    public AsyncFileWatcher(DllUpdater updater)
    {
        _updater = updater;
        _timer = new Timer(Check, null, Timeout.Infinite, Timeout.Infinite);
    }

    public event EventHandler? FilesChanged;

    public void Start()
    {
        _timer!.Change(0, 100);
    }

    public void AddFile(GameInfo info)
    {
        _queueFiles.Enqueue(info);
    }

    public void RemoveFile(GameInfo info)
    {
        var files = _queueFiles.ToList();
        var updated = files.Remove(info);

        if (updated)
        {
            _queueFiles.Clear();
            foreach (var file in files)
            {
                _queueFiles.Enqueue(file);
            }
        }
    }

    private async void Check(object? state)
    {
        if (_queueFiles.Count == 0)
        {
            return;
        }

        if (_queueFiles.TryDequeue(out var info))
        {
            var (bChanged, _) = await info.GatherInstalledVersions();
            if (bChanged)
            {
                FilesChanged?.Invoke(this, EventArgs.Empty);
            }

            _queueFiles.Enqueue(info);
        }
    }
}