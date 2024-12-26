using DLSSUpdater.Helpers;
using Microsoft.Win32;

namespace DlssUpdater.Helpers;

public static class EnumHelper
{
    public static T SetFlag<T>(this Enum value, T flag, bool set)
    {
        var underlyingType = Enum.GetUnderlyingType(value.GetType());

        // note: AsInt mean: math integer vs enum (not the c# int type)
        dynamic valueAsInt = Convert.ChangeType(value, underlyingType);
        dynamic flagAsInt = Convert.ChangeType(flag, underlyingType)!;
        if (set)
        {
            valueAsInt |= flagAsInt;
        }
        else
        {
            valueAsInt &= ~flagAsInt;
        }

        return (T)valueAsInt;
    }

    public static T GetAs<T>(string? value)
    {
        if(value is null)
        {
            return default;
        }
        return (T)Enum.Parse(typeof(T), value);
    }
}