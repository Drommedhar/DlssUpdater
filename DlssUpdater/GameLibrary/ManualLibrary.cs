using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary;

public class ManualLibrary : ILibrary
{
    public ManualLibrary(NLog.Logger logger)
    {

    }

    public async Task<List<GameInfo>> GatherGamesAsync()
    {
        List<GameInfo> ret = [];
        // TODO: Do something here like loading stuff or something
        return ret;
    }
}