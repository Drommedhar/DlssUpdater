using System.IO;

namespace DlssUpdater.Helpers;

public static class DirectoryHelper
{
    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    public static Dictionary<TKey, TElement> SafeToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TElement>(comparer);

        if (source == null)
        {
            return dictionary;
        }

        foreach (TSource element in source)
        {
            if (!dictionary.ContainsKey(keySelector(element)))
            {
                dictionary[keySelector(element)] = elementSelector(element);
            }
        }

        return dictionary;
    }
}