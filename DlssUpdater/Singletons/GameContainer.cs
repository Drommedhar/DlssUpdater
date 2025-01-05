using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Shapes;
using DlssUpdater.Defines;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using DLSSUpdater.Helpers;
using DLSSUpdater.Singletons;
using Microsoft.Win32;
using NLog;
using Octokit;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.Singletons;

public class GameContainer
{
    private readonly AntiCheatChecker.AntiCheatChecker _antiCheatChecker;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters =
        {
            new GameConvert()
        }
    };

    private readonly Logger _logger;

    private readonly Settings _settings;
    private readonly AsyncFileWatcher _watcher;

    public readonly List<ILibrary> Libraries = [];

    public GameContainer(Settings settings, AntiCheatChecker.AntiCheatChecker antiCheatChecker, Logger logger,
        AsyncFileWatcher watcher)
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

    public event EventHandler<Tuple<int, int, LibraryType>> LoadingProgress;
    public event EventHandler<string>? InfoMessage;
    public event EventHandler? GamesChanged;

    public void UpdateLibraries()
    {
        Libraries.Clear();
        foreach (var library in _settings.Libraries)
        {
            var lib = ILibrary.Create(library, _logger);
            lib.LoadingProgress += Lib_LoadingProgress;
            Libraries.Add(lib);
        }
    }

    private void Lib_LoadingProgress(object? sender, Tuple<int, int, LibraryType> e)
    {
        LoadingProgress?.Invoke(sender, e);
    }

    public async Task LoadGamesAsync()
    {
        _logger.Debug("Starting gathering games");
        var stopwatch = Stopwatch.StartNew();

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
        foreach (var game in Games)
        {
            var path = GetGameRegistryPath(game.GameName);
            RegistryHelper.WriteRegistryValue(path, nameof(game.UniqueId), game.UniqueId, RegistryHive.CurrentUser, RegistryView.Registry64);
            RegistryHelper.WriteRegistryValue(path, nameof(game.GamePath), game.GamePath, RegistryHive.CurrentUser, RegistryView.Registry64);
            RegistryHelper.WriteRegistryValue(path, nameof(game.LibraryType), game.LibraryType, RegistryHive.CurrentUser, RegistryView.Registry64);
            RegistryHelper.WriteRegistryValue(path, nameof(game.IsHidden), game.IsHidden, RegistryHive.CurrentUser, RegistryView.Registry64);
            if (game.GameImageUri is not null)
            {
                RegistryHelper.WriteRegistryValue(path, nameof(game.GameImageUri), game.GameImageUri, RegistryHive.CurrentUser, RegistryView.Registry64);
            }
        }
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
        foreach (var game in Games)
        {
            if (!game.IsHidden && game.UpdateVisible == Visibility.Visible)
            {
                return true;
            }
        }

        return false;
        //return Games.Any(g => !g.IsHidden && g.UpdateVisible == Visibility.Visible);
    }

    private async Task loadGamesAsync()
    {
        var gameNames = RegistryHelper.ReadRegistrySubKeys(Settings.Constants.RegistryPath + @$"\Games", RegistryHive.CurrentUser, RegistryView.Registry64);
        Games.Clear();
        foreach (var gameName in gameNames)
        {
            var path = GetGameRegistryPath(gameName);
            var gamePath = RegistryHelper.ReadRegistryValue(path, nameof(GameInfo.GamePath), RegistryHive.CurrentUser, RegistryView.Registry64) as string;
            var uniqueId = RegistryHelper.ReadRegistryValue(path, nameof(GameInfo.UniqueId), RegistryHive.CurrentUser, RegistryView.Registry64) as string;
            var libType = (LibraryType)Enum.Parse(typeof(LibraryType), RegistryHelper.ReadRegistryValue(path, nameof(GameInfo.LibraryType), RegistryHive.CurrentUser, RegistryView.Registry64)! as string);
            var gameImagePath = RegistryHelper.ReadRegistryValue(path, nameof(GameInfo.GameImageUri), RegistryHive.CurrentUser, RegistryView.Registry64) as string;
            var isHidden = bool.Parse(RegistryHelper.ReadRegistryValue(path, nameof(GameInfo.IsHidden), RegistryHive.CurrentUser, RegistryView.Registry64) as string ?? "False");
            if (gamePath is null)
            {
                continue;
            }

            var game = new GameInfo(gameName, gamePath, libType);
            game.UniqueId = uniqueId!;
            game.IsHidden = isHidden;
            game.GameImageUri = gameImagePath;
            game.GenerateGameImage();
            Games.Add(game);
            _watcher.AddFile(game);
        }

        List<GameInfo> totalGames = [];
        foreach (var lib in Libraries)
        {
            if (!_settings.Libraries.FirstOrDefault(l => l.LibraryType == lib.GetLibraryType())?.IsChecked ??
                false)
            {
                continue;
            }

            var libGames = await lib.GatherGamesAsync();
            totalGames.AddRange(libGames);
        }

        totalGames = totalGames.GroupBy(g => g.GamePath)
            .Select(g => g.First())
            .ToList();

        // We need to check our local list against the library games and remove
        // the ones that are no longer reported
        List<GameInfo> gamesToDelete = [];
        foreach (var item in Games)
        {
            if (item.LibraryType == LibraryType.Manual)
            {
                continue;
            }

            var game = totalGames.FirstOrDefault(g => g.UniqueId == item.UniqueId);
            if (game is null)
            {
                gamesToDelete.Add(item);
            }
        }

        foreach (var item in gamesToDelete)
        {
            Games.Remove(item);
        }

        var amount = totalGames.Count;
        var current = 0;
        foreach (var item in totalGames)
        {
            current++;
            InfoMessage?.Invoke(this, $"Parsing games {current}/{amount}");
            await CopyGameData(item);
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
                await CopyGameData(item);
            }
        }

        SaveGames();
    }

    private async Task CopyGameData(GameInfo? item)
    {
        
        var index = -1;
        if (item.LibraryType != LibraryType.Manual)
        // For library games, we check against the id, not the path
        {
            index = Games.IndexOf(g => g.UniqueId == item.UniqueId);
        }
        else
        {
            index = Games.IndexOf(g => g.GamePath == item.GamePath);
        }


        if (index != -1)
        {
            var id = Games[index].UniqueId;
            var isHidden = Games[index].IsHidden;
            Games[index] = new GameInfo(item);
            Games[index].UniqueId = id;
            Games[index].IsHidden = isHidden;
            await Games[index].GatherInstalledVersions();
            Games[index].Update();
        }
        else
        {
            var info = new GameInfo(item);
            await info.GatherInstalledVersions();
            info.Update();
            Games.Add(info);
        }

        _watcher.AddFile(item);
    }

    private void Games_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        GamesChanged?.Invoke(this, e);
    }

    private static string GetGameRegistryPath(string gameName)
    {
        return Settings.Constants.RegistryPath + @$"\Games\{gameName}";
    }
}