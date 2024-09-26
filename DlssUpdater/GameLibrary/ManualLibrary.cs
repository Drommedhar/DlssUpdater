using DLSSUpdater.Defines;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary;

public class ManualLibrary : ILibrary
{
    public ManualLibrary(LibraryConfig config, NLog.Logger logger)
    {

    }

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