using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using DlssUpdater.Defines;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using DLSSUpdater.Singletons;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.Singletons;

public class GameContainer
{
    public event EventHandler? GamesChanged;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters =
        {
            new GameConvert()
        }
    };

    public readonly List<ILibrary> Libraries = [];

    private readonly Settings _settings;
    private readonly AntiCheatChecker.AntiCheatChecker _antiCheatChecker;
    private readonly NLog.Logger _logger;
    private readonly AsyncFileWatcher _watcher;

    public GameContainer(Settings settings, AntiCheatChecker.AntiCheatChecker antiCheatChecker, NLog.Logger logger, AsyncFileWatcher watcher)
    {
        _settings = settings;
        _antiCheatChecker = antiCheatChecker;
        _logger = logger;
        _watcher = watcher;

        UpdateLibraries();

        Games.CollectionChanged += Games_CollectionChanged;
    }

    public SortableObservableCollection<GameInfo> Games { get; } = new()
    {
        SortingSelector = g => g.GameName
    };

    public void UpdateLibraries()
    {
        Libraries.Clear();
        foreach (var library in _settings.Libraries)
        {
            Libraries.Add(ILibrary.Create(library, _logger));
        }
    }

    public async Task LoadGamesAsync()
    {
        _logger.Debug("Starting gathering games");
        Stopwatch stopwatch = Stopwatch.StartNew();

        await loadGamesAsync();

        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        _logger.Debug($"Game Loading done. Took {elapsedMs} ms");
    }

    public void RemoveGame(GameInfo game)
    {
        _watcher.RemoveFile(game);
        Games.Remove(game);
    }

    public void DoUpdate()
    {
        GamesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SaveGames()
    {
        DirectoryHelper.EnsureDirectoryExists(_settings.Directories.SettingsPath);
        var json = JsonSerializer.Serialize(Games, _jsonOptions);
        File.WriteAllText(Path.Combine(_settings.Directories.SettingsPath, Settings.Constants.GamesFile), json);
    }

    public void RescanAntiCheat()
    {
        _antiCheatChecker.Init();
        foreach (var game in Games)
        {
            game.AntiCheat = _antiCheatChecker.Check(game.GamePath);
            game.SetAntiCheatImage();
        }
    }

    public bool IsUpdateAvailable()
    {
        foreach(var game in Games)
        {
            if (!game.IsHidden && game.UpdateVisible == Visibility.Visible)
                return true;
        }
        return false;
        //return Games.Any(g => !g.IsHidden && g.UpdateVisible == Visibility.Visible);
    }

    private async Task loadGamesAsync()
    {
        var gameDataPath = Path.Combine(_settings.Directories.SettingsPath, Settings.Constants.GamesFile);

        if (File.Exists(gameDataPath))
        {
            var jsonData = File.ReadAllText(gameDataPath);
            var data = JsonSerializer.Deserialize<ObservableCollection<GameInfo>>(jsonData, _jsonOptions)!;
            Games.Clear();
            foreach (var item in data)
            {
                Games.Add(item);
                _watcher.AddFile(item);
            }
        }

        foreach (var lib in Libraries)
        {
            if (!_settings.Libraries.FirstOrDefault(l => l.LibraryType == lib.GetLibraryType())?.IsChecked ?? false)
            {
                continue;
            }

            var libGames = await lib.GatherGamesAsync();
            foreach (var item in libGames)
            {
                var index = Games.IndexOf(g => g.GamePath == item.GamePath);
                if (index != -1)
                {
                    var id = Games[index].UniqueId;
                    var isHidden = Games[index].IsHidden;
                    Games[index] = new(item);
                    Games[index].UniqueId = id;
                    Games[index].IsHidden = isHidden;
                    await Games[index].GatherInstalledVersions();
                }
                else
                {
                    var info = new GameInfo(item);
                    await info.GatherInstalledVersions();
                    Games.Add(info);
                }
                _watcher.AddFile(item);
            }
        }

        SaveGames();
        DoUpdate();
    }

    public async Task ReloadLibraryGames(LibraryType type)
    {
        // Create a list of games to remove
        var gamesToRemove = Games.Where(game => game.LibraryType == type).ToList();

        // Remove the games from the ObservableCollection
        foreach (var game in gamesToRemove)
        {
            Games.Remove(game);
            _watcher.RemoveFile(game);
        }

        if (!_settings.Libraries.FirstOrDefault(l => l.LibraryType == type)?.IsChecked ?? false)
        {
            SaveGames();
            return;
        }

        // Reload games from libraries of the specified type
        foreach (var lib in Libraries.Where(l => l.GetLibraryType() == type))
        {
            var libGames = await lib.GatherGamesAsync();
            foreach (var item in libGames.Where(item => !Games.Any(g => g.GamePath == item.GamePath)))
            {
                Games.Add(item);
                _watcher.AddFile(item);
            }
        }

        SaveGames();
    }

    private void Games_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        GamesChanged?.Invoke(this, e);
    }
}