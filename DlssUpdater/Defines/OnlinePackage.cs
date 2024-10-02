using static DlssUpdater.Defines.DlssTypes;

namespace DlssUpdater.Defines;

public class OnlinePackage
{
    public DllType DllType { get; set; }
    public string Version { get; set; }
    public string SHA256 { get; set; }
    public string DownloadId { get; set; }

    public OnlinePackage()
    {
        Version = string.Empty;
        SHA256 = string.Empty;
        DownloadId = string.Empty;
    }

    public override string ToString()
    {
        return Version;
    }
}