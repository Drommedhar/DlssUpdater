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

    public static object? ReadRegistryValue(string key, string value, RegistryHive hive, RegistryView view)
    {
        using var hklm = RegistryKey.OpenBaseKey(hive, view);
        using var regKey = hklm.OpenSubKey(key);
        var ret = regKey?.GetValue(value) as string;
        return ret;
    }

    public static List<string> ReadRegistrySubKeys(string key, RegistryHive hive, RegistryView view)
    {
        using var hklm = RegistryKey.OpenBaseKey(hive, view);
        using var regKey = hklm.OpenSubKey(key);
        return regKey?.GetSubKeyNames().ToList() ?? [];
    }

    public static void WriteRegistryValue(string key, string valueName, object valueValue, RegistryHive hive, RegistryView view)
    {
        using var hklm = RegistryKey.OpenBaseKey(hive, view);
        using var regKey = hklm.CreateSubKey(key);
        regKey?.SetValue(valueName, valueValue);
    }
}