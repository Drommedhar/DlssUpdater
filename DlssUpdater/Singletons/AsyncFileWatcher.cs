
using DlssUpdater.Defines;
using DlssUpdater.Singletons;

namespace DLSSUpdater.Singletons
{
    /// <summary>
    /// Custom class to check changes on specific files, as FileSystemWatcher is not working as I need it.
    /// </summary>
    public class AsyncFileWatcher
    {
        public event EventHandler? FilesChanged;

        private Queue<GameInfo> _queueFiles = new();
        private object _lock = new();
        private Timer? _timer;

        private readonly DllUpdater _updater;

        public AsyncFileWatcher(DllUpdater updater)
        {
            _updater = updater;
            _timer = new(Check, null, 0, 100);
        }

        public void AddFile(GameInfo info)
        {
            lock (_lock)
            {
                _queueFiles.Enqueue(info);
            }
        }

        public void RemoveFile(GameInfo info) 
        {
            lock (_lock)
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
        }

        private async void Check(object? state)
        {
            GameInfo? info = null;
            lock (_lock)
            {
                if(_queueFiles.Count == 0)
                {
                    return;
                }
                info = _queueFiles.Dequeue();
            }

            var bChanged = await info.GatherInstalledVersions();
            if (bChanged)
            {
                FilesChanged?.Invoke(this, EventArgs.Empty);
            }

            lock (_lock) 
            {
                _queueFiles.Enqueue(info);
            }
        }
    }
}
