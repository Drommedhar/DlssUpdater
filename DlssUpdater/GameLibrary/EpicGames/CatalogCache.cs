using System.Text.Json.Serialization;

namespace DLSSUpdater.GameLibrary.EpicGames;

internal class CatalogCache
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("keyImages")] public List<CachedGame> KeyImages { get; set; } = [];


    internal class CachedGame
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;

        [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    }
}