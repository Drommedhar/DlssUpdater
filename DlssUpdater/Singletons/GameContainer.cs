using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using DlssUpdater.Defines;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.Singletons;

public class GameContainer
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters =
        {
            new GameConvert()
        }
    };

    private readonly List<ILibrary> _libraries = [];

    private readonly Settings _settings;
    private readonly AntiCheatChecker.AntiCheatChecker _antiCheatChecker;
    private readonly NLog.Logger _logger;

    public GameContainer(Settings settings, AntiCheatChecker.AntiCheatChecker antiCheatChecker, NLog.Logger logger)
    {
        _settings = settings;
        _antiCheatChecker = antiCheatChecker;
        _logger = logger;

        // TODO: Add settings for active libraries
        _libraries.Add(ILibrary.Create(LibraryType.Steam, _logger));
        _libraries.Add(ILibrary.Create(LibraryType.Ubisoft, _logger));
    }

    public SortableObservableCollection<GameInfo> Games { get; } = new()
    {
        SortingSelector = g => g.GameName
    };

    public async Task LoadGamesAsync()
    {
        await loadGamesAsync();
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
            game.HasAntiCheat = _antiCheatChecker.Check(game.GamePath);
        }
    }

    private async Task loadGamesAsync()
    {
        var gameDataPath = Path.Combine(_settings.Directories.SettingsPath, Settings.Constants.GamesFile);

        if (File.Exists(gameDataPath))
        {
            var jsonData = File.ReadAllText(gameDataPath);
            var data = JsonSerializer.Deserialize<ObservableCollection<GameInfo>>(jsonData, _jsonOptions)!;
            Games.Clear();
            foreach (var item in data) Games.Add(item);
        }

        foreach (var lib in _libraries)
        {
            var libGames = await lib.GatherGamesAsync();
            foreach (var item in libGames)
                if (!Games.Any(g => g.GamePath == item.GamePath))
                    Games.Add(item);
        }
    }
}