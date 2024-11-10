using Microsoft.Win32;

namespace DLSSUpdater.Helpers;

public static class RegistryHelper
{
    public static object? GetRegistryValue(string key, string value)
    {
        object? retVal = null;

        retVal ??= ReadRegistryValue(key, value, RegistryHive.LocalMachine, RegistryView.Registry32);
        retVal ??= ReadRegistryValue(key, value, RegistryHive.LocalMachine, RegistryView.Registry64);
        retVal ??= ReadRegistryValue(key, value, RegistryHive.CurrentUser, RegistryView.Registry32);
        retVal ??= ReadRegistryValue(key, value, RegistryHive.CurrentUser, RegistryView.Registry64);

        return retVal;
    }

    private static object? ReadRegistryValue(string key, string value, RegistryHive hive, RegistryView view)
    {
        using var hklm = RegistryKey.OpenBaseKey(hive, view);
        using var regKey = hklm.OpenSubKey(key);
        var ret = regKey?.GetValue(value) as string;
        return ret;
    }
}