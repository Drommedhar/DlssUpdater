using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DlssUpdater.GameLibrary;
using DlssUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using DLSSUpdater.Defines.UI.Pages;
using static DlssUpdater.Defines.DlssTypes;

namespace DlssUpdater.Defines;

public partial class GameInfo : ObservableObject, IEquatable<GameInfo>
{
    public string UniqueId { get; set; }
    [ObservableProperty] [JsonIgnore] public ImageSource? _gameImage;
    [ObservableProperty] [JsonIgnore] public ImageSource? _libraryImage;
    [ObservableProperty][JsonIgnore] public ImageSource? _antiCheatImage;
    [ObservableProperty][JsonIgnore] public ImageSource? _hideImage;
    [ObservableProperty][JsonIgnore] public Visibility _hasAntiCheat = Visibility.Collapsed;

    [ObservableProperty] public string? _gameImageUri;

    [ObservableProperty] public string _gameName;

    [ObservableProperty] public string _gamePath;
    private bool _isHidden;
    public bool IsHidden { get => _isHidden; set
        {
            if (value != _isHidden)
            {
                _isHidden = value;
                setHideImage();
            }
        }
    }
    [ObservableProperty] public Visibility _removeVisible;

    [JsonIgnore] public AntiCheatProvider AntiCheat = AntiCheatProvider.None;

    [ObservableProperty] [JsonIgnore] public GameInfo _self;

    [ObservableProperty] [JsonIgnore] public Visibility _updateVisible;
    [ObservableProperty][JsonIgnore] public string _installedVersionDlss;
    [ObservableProperty][JsonIgnore] public string _installedVersionDlssD;
    [ObservableProperty][JsonIgnore] public string _installedVersionDlssG;
    public Dictionary<DllType, bool> DefaultDlls { get; set; } = [];
    [JsonIgnore] public Dictionary<DllType, InstalledPackage> InstalledDlls { get; set; } = [];

    [JsonIgnore] public LibraryType LibraryType;
    [JsonIgnore] public readonly DllUpdater _updater;
    [JsonIgnore] public readonly NLog.Logger _logger;
    [JsonIgnore] public readonly LibraryPage _libPage;

    public GameInfo(GameInfo gameInfo)
    {
        GameName = gameInfo.GameName;
        GamePath = gameInfo.GamePath;
        LibraryType = gameInfo.LibraryType;
        _removeVisible = LibraryType == LibraryType.Manual ? Visibility.Visible : Visibility.Collapsed;
        UniqueId = gameInfo.UniqueId;
        IsHidden = gameInfo.IsHidden;
        GameImageUri = gameInfo.GameImageUri;
        foreach(var kvp in gameInfo.InstalledDlls)
        {
            InstalledDlls.Add(kvp.Key, kvp.Value);
        }
        _updater = gameInfo._updater;
        _logger = gameInfo._logger;
        _libPage = gameInfo._libPage;
        InstalledVersionDlss = gameInfo.InstalledVersionDlss;
        InstalledVersionDlssD = gameInfo.InstalledVersionDlssD;
        InstalledVersionDlssG = gameInfo._installedVersionDlssG;
        GenerateGameImage();
        setLibraryImage();
        setHideImage();
        SetAntiCheatImage();

        Self = this;
    }

    public GameInfo(string gameName, string gamePath, LibraryType type)
    {
        GameName = gameName;
        GamePath = gamePath;
        LibraryType = type;
        _removeVisible = LibraryType == LibraryType.Manual ? Visibility.Visible : Visibility.Collapsed;
        InstalledVersionDlss = "N/A";
        InstalledVersionDlssD = "N/A";
        InstalledVersionDlssG = "N/A";
        UniqueId = Guid.NewGuid().ToString();
        setLibraryImage();
        setHideImage();

        foreach (DllType dllType in Enum.GetValues(typeof(DllType))) InstalledDlls.Add(dllType, new InstalledPackage());

        Self = this;
        _updater = App.GetService<DllUpdater>()!;
        _logger = App.GetService<NLog.Logger>()!;
        _libPage = App.GetService<LibraryPage>()!;
    }

    public void Update()
    {
        setLibraryImage();
        setHideImage();
        AntiCheat = App.GetService<AntiCheatChecker>()!.Check(GamePath);
        SetAntiCheatImage();
    }

    private void setHideImage()
    {
        HideImage = _isHidden switch
        {
            true => new BitmapImage(new Uri("pack://application:,,,/Icons/unhide.png")),
            false => new BitmapImage(new Uri("pack://application:,,,/Icons/hide.png")),
        };
    }

    private void setLibraryImage()
    {
        LibraryImage = LibraryType switch
        {
            LibraryType.Manual => new BitmapImage(new Uri("pack://application:,,,/Icons/folder.png")),
            LibraryType.Steam => new BitmapImage(new Uri("pack://application:,,,/Icons/steam.png")),
            LibraryType.Ubisoft => new BitmapImage(new Uri("pack://application:,,,/Icons/ubi.png")),
            LibraryType.EpicGames => new BitmapImage(new Uri("pack://application:,,,/Icons/epic.png")),
            LibraryType.GOG => new BitmapImage(new Uri("pack://application:,,,/Icons/gog.png")),
            LibraryType.Xbox => new BitmapImage(new Uri("pack://application:,,,/Icons/xbox.png")),
            _ => null,
        };
    }

    public void SetAntiCheatImage()
    {
        AntiCheatImage = AntiCheat switch
        {
            AntiCheatProvider.EasyAntiCheat => new BitmapImage(new Uri("pack://application:,,,/Icons/eac.png")),
            AntiCheatProvider.BattlEye => new BitmapImage(new Uri("pack://application:,,,/Icons/battleye.png")),
            _ => null,
        };

        HasAntiCheat = AntiCheatImage != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public async Task<(bool, Exception?)> GatherInstalledVersions()
    {
        if (!Directory.Exists(GamePath)) return (false, null);

        bool bChanged = false;
        try
        {
            await Task.Run(() =>
            {
                bool bUpdateAvailable = false;
                foreach (var (dll, info) in InstalledDlls)
                {
                    var allFiles = Directory.GetFiles(GamePath, GetDllName(dll), SearchOption.AllDirectories);
                    //_logger.Debug($"Found '{allFiles?.Length.ToString() ?? "0"} files' for {GetDllName(dll)} in {GameName}");
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

                    switch (dll)
                    {
                        case DllType.Dlss: InstalledVersionDlss = info.Version; break;
                        case DllType.DlssD: InstalledVersionDlssD = info.Version; break;
                        case DllType.DlssG: InstalledVersionDlssG = info.Version; break;
                    }
                }

                UpdateVisible = bUpdateAvailable ? Visibility.Visible : Visibility.Hidden;

            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error($"GameInfo.GatherInstalledVersions failed with: {ex}");
            InstalledDlls.Clear();
            return (false, ex);
        }        

        if(bChanged)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { _libPage.UpdateNotificationInfo(); }));
        }

        return (bChanged, null);
    }

    public bool HasInstalledDlls()
    {
        return InstalledDlls.Any(kvp => !string.IsNullOrEmpty(kvp.Value.Version));
    }

    public void SetGameImageUri(string imageUri)
    {
        GameImageUri = imageUri;
    }

    public void GenerateGameImage()
    {
        try
        {
            if (!string.IsNullOrEmpty(GameImageUri))
            {
                GameImage = new BitmapImage(new Uri(GameImageUri));
            }
        }
        catch (FileNotFoundException ex)
        {
            _logger.Error($"Image not found: {ex}");
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
                if (!string.IsNullOrEmpty(gameImageUri))
                {
                    info.SetGameImageUri(gameImageUri);
                    info.GenerateGameImage();
                }
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