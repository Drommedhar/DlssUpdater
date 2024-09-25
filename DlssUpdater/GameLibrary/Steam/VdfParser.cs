using System.IO;

namespace DlssUpdater.GameLibrary.Steam;

public class VdfParser
{
    private readonly string _path;
    private string? _data;

    public VdfParser(string path)
    {
        _path = path;
    }

    public async Task<bool> Load()
    {
        if (!File.Exists(_path)) return false;

        _data = await File.ReadAllTextAsync(_path);
        return true;
    }

    public List<T> GetValuesForKey<T>(string key)
    {
        var genericType = typeof(T);
        List<T> values = [];

        var indices = findKeyIndices(key);
        foreach (var item in indices)
        {
            if (genericType == typeof(string))
            {
                // Only parse the next string we can find
                var value = getString(item + key.Length);
                if (value is null) continue;

                if (value is T convertedValue) values.Add(convertedValue);
            }

            if (genericType == typeof(List<string>))
            {
                // We know there needs to be some kind of object here
                var objectValues = getObject(item + key.Length);
                if (objectValues.Count == 0) continue;

                if (objectValues is T convertedValue) values.Add(convertedValue);
            }
        }

        return values;
    }

    private string? getString(int start)
    {
        var startIndex = _data!.IndexOf('\"', start + 1);
        var endIndex = _data!.IndexOf('\"', startIndex + 1);
        if (startIndex == -1 || endIndex == -1) return null;

        return _data!.Substring(startIndex + 1, endIndex - startIndex - 1);
    }

    private List<string> getObject(int start)
    {
        List<string> objects = [];

        var objectStart = _data!.IndexOf('{', start + 1);
        var objectEnd = _data!.IndexOf('}', start + 1);
        if (objectStart == -1 || objectEnd == -1) return objects;

        var startIndex = objectStart;
        int index;
        while ((index = _data!.IndexOf('\"', startIndex)) != -1 && index <= objectEnd)
        {
            // index now contains the start of the value, find the end
            var valueEndIndex = _data!.IndexOf('\"', index + 1);
            if (valueEndIndex == -1)
                // Should not happen, but better be safe
                break;

            var value = _data!.Substring(index + 1, valueEndIndex - index - 1);
            objects.Add(value.Trim());
            startIndex = valueEndIndex + 1;
        }

        return objects;
    }

    private List<int> findKeyIndices(string key)
    {
        List<int> keyIndidices = new();

        var startIndex = 0;
        int index;
        while ((index = _data!.IndexOf(key, startIndex)) != -1)
        {
            keyIndidices.Add(index);
            startIndex = index + 1;
        }

        return keyIndidices;
    }
}