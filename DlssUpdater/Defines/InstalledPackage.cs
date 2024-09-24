namespace DlssUpdater.Defines;

public class InstalledPackage
{
    public string Version { get; set; }
    public string VersionDetailed { get; set; }
    public string Path { get; set; }

    public override string ToString()
    {
        return Version;
    }
}