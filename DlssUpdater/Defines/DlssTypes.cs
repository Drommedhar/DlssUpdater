namespace DlssUpdater.Defines;

public static class DlssTypes
{
    public enum DllType
    {
        Dlss,
        DlssD,
        DlssG
    }

    public static string? GetUrl(DllType dllType)
    {
        return dllType switch
        {
            DllType.Dlss => "https://www.techpowerup.com/download/nvidia-dlss-dll/",
            DllType.DlssD => "https://www.techpowerup.com/download/nvidia-dlss-3-ray-reconstruction-dll/",
            DllType.DlssG => "https://www.techpowerup.com/download/nvidia-dlss-3-frame-generation-dll/",
            _ => null
        };
    }

    public static string GetName(DllType type)
    {
        return type switch
        {
            DllType.Dlss => "DLSS",
            DllType.DlssD => "DLSS Ray Reconstruction",
            DllType.DlssG => "DLSS Frame Gen",
            _ => ""
        };
    }

    public static string GetShortName(DllType type)
    {
        return type switch
        {
            DllType.Dlss => "Main",
            DllType.DlssD => "Ray Reconstruction",
            DllType.DlssG => "Frame Gen",
            _ => ""
        };
    }

    public static string GetDllName(DllType type)
    {
        return type switch
        {
            DllType.Dlss => "nvngx_dlss.dll",
            DllType.DlssD => "nvngx_dlssd.dll",
            DllType.DlssG => "nvngx_dlssg.dll",
            _ => ""
        };
    }
}