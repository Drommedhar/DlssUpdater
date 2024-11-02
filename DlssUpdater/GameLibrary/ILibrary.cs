using System.ComponentModel;
using DlssUpdater.GameLibrary.Steam;
using DLSSUpdater.Defines;
using DLSSUpdater.GameLibrary;
using DLSSUpdater.GameLibrary.EpicGames;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary;

public enum LibraryType
{
    Manual,

    Steam,
    Ubisoft,
    EpicGames,
    GOG,
    Xbox,
    // TODO: More launchers
}

public interface ILibrary
{
    public event EventHandler<Tuple<int, int, LibraryType>>? LoadingProgress;
    public LibraryType GetLibraryType();
    public void GetInstallationDirectory();
    public Task<List<GameInfo>> GatherGamesAsync();

    static ILibrary Create(LibraryConfig config, NLog.Logger logger)
    {
        return config.LibraryType switch
        {
            LibraryType.Manual => new ManualLibrary(config, logger),
            LibraryType.Steam => new SteamLibrary(config, logger),
            LibraryType.Ubisoft => new UbisoftConnectLibrary(config, logger),
            LibraryType.GOG => new GOGLibrary(config, logger),
            LibraryType.EpicGames => new EpicGamesLibrary(config, logger),
            LibraryType.Xbox => new XboxLibrary(config, logger),
            _ => throw new InvalidEnumArgumentException(nameof(config.LibraryType), (int)config.LibraryType, typeof(LibraryType))
        };
    }

    static string GetName(LibraryType type)
    {
        return type switch
        {
            LibraryType.Manual => "Manual",
            LibraryType.Steam => "Steam",
            LibraryType.Ubisoft => "Ubisoft Connect",
            LibraryType.GOG => "GOG",
            LibraryType.EpicGames => "Epic Games",
            LibraryType.Xbox => "Xbox",
            _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(LibraryType))
        };
    }
}