namespace DlssUpdater.Defines;

public class InstalledPackage
{
    public InstalledPackage()
    {
        Version = string.Empty;
        VersionDetailed = string.Empty;
        Path = string.Empty;
    }

    public string Version { get; set; }
    public string VersionDetailed { get; set; }
    public string Path { get; set; }

    public override string ToString()
    {
        return Version;
    }
}