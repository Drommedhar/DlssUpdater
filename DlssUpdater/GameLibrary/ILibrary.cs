using System.ComponentModel;
using DlssUpdater.GameLibrary.Steam;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary;

public enum LibraryType
{
    Manual,

    Steam,
    Ubisoft
    // TODO: More launchers
}

public interface ILibrary
{
    public Task<List<GameInfo>> GatherGamesAsync();

    static ILibrary Create(LibraryType type, NLog.Logger logger)
    {
        return type switch
        {
            LibraryType.Manual => new ManualLibrary(logger),
            LibraryType.Steam => new SteamLibrary(logger),
            LibraryType.Ubisoft => new UbisoftConnectLibrary(logger),
            _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(LibraryType))
        };
    }
}