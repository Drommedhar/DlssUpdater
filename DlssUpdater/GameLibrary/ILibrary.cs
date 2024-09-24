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

    static ILibrary Create(LibraryType type)
    {
        return type switch
        {
            LibraryType.Manual => new ManualLibrary(),
            LibraryType.Steam => new SteamLibrary(),
            LibraryType.Ubisoft => new UbisoftConnectLibrary(),
            _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(LibraryType))
        };
    }
}