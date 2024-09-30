using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DlssUpdater.GameLibrary;
using DlssUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using DLSSUpdater.Singletons;
using static DlssUpdater.Defines.DlssTypes;

namespace DlssUpdater.Defines;

public partial class GameInfo : ObservableObject, IEquatable<GameInfo>
{
    public string UniqueId { get; set; }
    [ObservableProperty] [JsonIgnore] public ImageSource? _gameImage;

    [ObservableProperty] public string? _gameImageUri;

    [ObservableProperty] public string _gameName;

    [ObservableProperty] public string _gamePath;
    public bool IsHidden;
    [ObservableProperty] public Visibility _visible;

    [ObservableProperty] [JsonIgnore] public bool _hasAntiCheat;

    [ObservableProperty] [JsonIgnore] public GameInfo _self;

    [ObservableProperty] [JsonIgnore] public Visibility _textVisible;
    [ObservableProperty] [JsonIgnore] public Visibility _updateVisible;
    [ObservableProperty][JsonIgnore] public string _installedVersions;

    [JsonIgnore] public LibraryType LibraryType;
    [JsonIgnore] public readonly DllUpdater _updater;
    [JsonIgnore] public readonly NLog.Logger _logger;

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
        _logger = App.GetService<NLog.Logger>()!;
        GatherInstalledVersions().ConfigureAwait(true);
    }

    [JsonIgnore] public Dictionary<DllType, InstalledPackage> InstalledDlls { get; set; } = [];

    public async Task<bool> GatherInstalledVersions()
    {
        if (!Directory.Exists(GamePath)) return false;

        bool bChanged = false;
        var internalVersions = "";
        await Task.Run(() =>
        {
            bool bUpdateAvailable = false;
            foreach (var (dll, info) in InstalledDlls)
            {
                var allFiles = Directory.GetFiles(GamePath, GetDllName(dll), SearchOption.AllDirectories);
                _logger.Debug($"Found '{allFiles?.Length.ToString() ?? "0"} files' for {GetDllName(dll)} in {GameName}");
                if (allFiles is null || allFiles.Length == 0)
                {
                    continue; 
                }
                // TODO: Support for multiple instances of the same dll?

                // We only should have one entry
                info.Path = allFiles[^1];
                var fileInfo = FileVersionInfo.GetVersionInfo(info.Path);
                var newVersion = fileInfo.FileVersion?.Replace(',', '.');
                if (newVersion is not null && newVersion != info.Version)
                {
                    info.Version = fileInfo.FileVersion?.Replace(',', '.') ?? "0.0.0.0";
                    bChanged = true;
                }
                if (_updater.IsNewerVersionAvailable(dll, info))
                {
                    bUpdateAvailable = true;
                }

                if (!string.IsNullOrEmpty(internalVersions))
                {
                    internalVersions += "\n";
                }
                internalVersions += $"{GetShortName(dll)}: {info.Version}";
            }

            UpdateVisible = bUpdateAvailable ? Visibility.Visible : Visibility.Hidden;
        });

        InstalledVersions = internalVersions;

        return bChanged;
    }

    public bool HasInstalledDlls()
    {
        return InstalledDlls.Any(kvp => !string.IsNullOrEmpty(kvp.Value.Version));
    }

    public void SetGameImageUri(string imageUri)
    {
        GameImageUri = imageUri;
        try
        {
            if (!string.IsNullOrEmpty(GameImageUri))
            {
                GameImage = new BitmapImage(new Uri(GameImageUri));
                TextVisible = Visibility.Hidden;
            }
            else
            {
                TextVisible = Visibility.Visible;
            }
        }
        catch(FileNotFoundException ex)
        {
            _logger.Error($"Image not found: {ex}");
            TextVisible = Visibility.Visible;
        }
    }

    public bool Equals(GameInfo? other)
    {
        return UniqueId == other?.UniqueId;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;
        return Equals(obj as GameInfo);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueId);
    }
}

public class GameConvert : JsonConverter<GameInfo>
{
    public override GameInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var gameName = "";
        var gamePath = "";
        var libraryType = "";
        var uniqueId = "";
        string? gameImageUri = null;
        var isHidden = false;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                LibraryType type = string.IsNullOrEmpty(libraryType) ? LibraryType.Manual : (LibraryType)Enum.Parse(typeof(LibraryType), libraryType);
                var info = new GameInfo(gameName!, gamePath!, type);
                if(type == LibraryType.Manual && string.IsNullOrEmpty(uniqueId))
                {
                    uniqueId = Guid.NewGuid().ToString();
                }
                info.UniqueId = uniqueId!;
                info.IsHidden = isHidden;
                if (!string.IsNullOrEmpty(gameImageUri)) info.SetGameImageUri(gameImageUri);
                return info;
            }

            var propName = reader.GetString();
            reader.Read();
            if (propName == "GameName")
                gameName = reader.GetString();
            else if (propName == "GamePath")
                gamePath = reader.GetString();
            else if (propName == "GameImageUri")
                gameImageUri = reader.GetString();
            else if(propName == "LibraryType")
                libraryType = reader.GetString();
            else if(propName == "UniqueId")
                uniqueId = reader.GetString();
            else if(propName == "IsHidden")
                isHidden = reader.GetBoolean();
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, GameInfo value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString((string)nameof(GameInfo.GameName), (string?)value.GameName);
        writer.WriteString((string)nameof(GameInfo.GamePath), (string?)value.GamePath);
        writer.WriteString((string)nameof(GameInfo.GameImageUri), value.GameImageUri);
        writer.WriteString((string)nameof(GameInfo.LibraryType), value.LibraryType.ToString());
        writer.WriteString((string)nameof(GameInfo.UniqueId), value.UniqueId);
        writer.WriteBoolean((string)nameof(GameInfo.IsHidden), value.IsHidden);
        writer.WriteEndObject();
    }
}