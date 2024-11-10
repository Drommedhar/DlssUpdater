using DLSSUpdater.Defines;
using NLog;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary;

public class ManualLibrary : ILibrary
{
    public ManualLibrary(LibraryConfig config, Logger logger)
    {
    }

    public event EventHandler<Tuple<int, int, LibraryType>> LoadingProgress;

    public LibraryType GetLibraryType()
    {
        return LibraryType.Manual;
    }

    public void GetInstallationDirectory()
    {
    }

    public async Task<List<GameInfo>> GatherGamesAsync()
    {
        List<GameInfo> ret = [];
        // TODO: Do something here like loading stuff or something
        return ret;
    }
}