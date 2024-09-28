using System.ComponentModel;
using DlssUpdater.GameLibrary.Steam;
using DLSSUpdater.Defines;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary;

public enum LibraryType
{
    Manual,

    Steam,
    Ubisoft,
    GOG,
    // TODO: More launchers
}

public interface ILibrary
{
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
            _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(LibraryType))
        };
    }
}