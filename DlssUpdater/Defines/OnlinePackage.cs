using static DlssUpdater.Defines.DlssTypes;

namespace DlssUpdater.Defines;

public class OnlinePackage
{
    public DllType DllType { get; set; }
    public string Version { get; set; }
    public string MD5 { get; set; }
    public string DownloadId { get; set; }

    public override string ToString()
    {
        return Version;
    }
}