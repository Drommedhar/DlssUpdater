using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DlssUpdater.GameLibrary;
using DlssUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using static DlssUpdater.Defines.DlssTypes;

namespace DlssUpdater.Defines;

public partial class GameInfo : ObservableObject
{
    [ObservableProperty] [JsonIgnore] public ImageSource? _gameImage;

    [ObservableProperty] public string? _gameImageUri;

    [ObservableProperty] public string _gameName;

    [ObservableProperty] public string _gamePath;

    [ObservableProperty] [JsonIgnore] public bool _hasAntiCheat;

    [ObservableProperty] [JsonIgnore] public GameInfo _self;

    [ObservableProperty] [JsonIgnore] public Visibility _textVisible;
    [ObservableProperty][JsonIgnore] public Visibility _updateVisible;

    [JsonIgnore] public LibraryType LibraryType;
    [JsonIgnore] public readonly DllUpdater _updater;

    public GameInfo(string gameName, string gamePath, LibraryType type)
    {
        TextVisible = Visibility.Visible;
        GameName = gameName;
        GamePath = gamePath;
        LibraryType = type;

        foreach (DllType dllType in Enum.GetValues(typeof(DllType))) InstalledDlls.Add(dllType, new InstalledPackage());

        Self = this;
        HasAntiCheat = App.GetService<AntiCheatChecker>()!.Check(gamePath);
        _updater = App.GetService<DllUpdater>()!;
        GatherInstalledVersions().ConfigureAwait(true);
    }

    [JsonIgnore] public Dictionary<DllType, InstalledPackage> InstalledDlls { get; set; } = [];

    public async Task GatherInstalledVersions()
    {
        if (!Directory.Exists(GamePath)) return;

        await Task.Run(() =>
        {
            bool bUpdateAvailable = false;
            foreach (var (dll, info) in InstalledDlls)
            {
                var allFiles = Directory.GetFiles(GamePath, GetDllName(dll), SearchOption.AllDirectories);
                if (allFiles is null || allFiles.Length != 1) continue;

                // We only should have one entry
                info.Path = allFiles[0];
                var fileInfo = FileVersionInfo.GetVersionInfo(info.Path);
                info.Version = fileInfo.FileVersion!.Replace(',', '.');
                if (_updater.IsNewerVersionAvailable(dll, info)) 
                {
                    bUpdateAvailable = true;
                }
            }

            UpdateVisible = bUpdateAvailable ? Visibility.Visible : Visibility.Hidden;
        });
    }

    public bool HasInstalledDlls()
    {
        return InstalledDlls.Any(kvp => !string.IsNullOrEmpty(kvp.Value.Version));
    }

    public void SetGameImageUri(string imageUri)
    {
        GameImageUri = imageUri;
        if (!string.IsNullOrEmpty(GameImageUri)) GameImage = new BitmapImage(new Uri(GameImageUri));
    }
}

public class GameConvert : JsonConverter<GameInfo>
{
    public override GameInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var gameName = "";
        var gamePath = "";
        string? gameImageUri = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                var info = new GameInfo(gameName!, gamePath!, LibraryType.Manual);
                if (!string.IsNullOrEmpty(gameImageUri)) info.SetGameImageUri(gameImageUri);
                return info;
            }

            var propName = reader.GetString();
            reader.Read();
            if (propName == "GameName")
                gameName = reader.GetString();
            else if (propName == "GamePath")
                gamePath = reader.GetString();
            else if (propName == "GameImageUri") gameImageUri = reader.GetString();
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, GameInfo value, JsonSerializerOptions options)
    {
        if (value.LibraryType != LibraryType.Manual) return;

        writer.WriteStartObject();
        writer.WriteString((string)nameof(GameInfo.GameName), (string?)value.GameName);
        writer.WriteString((string)nameof(GameInfo.GamePath), (string?)value.GamePath);
        writer.WriteString((string)nameof(GameInfo.GameImageUri), value.GameImageUri);
        writer.WriteEndObject();
    }
}