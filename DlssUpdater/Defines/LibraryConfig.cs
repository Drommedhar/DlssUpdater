﻿using System.Text.Json;
using System.Text.Json.Serialization;
using DLSSUpdater.Defines;
using DlssUpdater.GameLibrary;

namespace DLSSUpdater.Defines
{
    public partial class LibraryConfig : ObservableObject
    {
        [ObservableProperty] private string _installPath;

        [ObservableProperty] private bool _isChecked;

        [ObservableProperty] private string _libraryName;

        [ObservableProperty] private bool _needsInstallPath;

        [ObservableProperty] [JsonIgnore] public LibraryConfig _self;

        public LibraryConfig(LibraryType type, string name)
        {
            LibraryType = type;
            _libraryName = name;
            _installPath = string.Empty;
            _isChecked = true;
            _needsInstallPath = true;
            Self = this;
        }

        public LibraryType LibraryType { get; set; }
    }
}

public class LibraryConvert : JsonConverter<LibraryConfig>
{
    public override LibraryConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var isChecked = false;
        var libraryName = "";
        var installPath = "";
        var needsInstallPath = true;
        var libraryType = LibraryType.Manual;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                var config = new LibraryConfig(libraryType, libraryName)
                {
                    IsChecked = isChecked,
                    InstallPath = installPath,
                    NeedsInstallPath = needsInstallPath
                };
                return config;
            }

            // TODO: More
            var propName = reader.GetString();
            reader.Read();
            if (propName == "IsChecked")
            {
                isChecked = reader.GetBoolean();
            }

            if (propName == "LibraryName")
            {
                libraryName = reader.GetString()!;
            }

            if (propName == "LibraryType")
            {
                libraryType = (LibraryType)Enum.Parse(typeof(LibraryType), reader.GetString()!);
            }

            if (propName == "InstallPath")
            {
                installPath = reader.GetString()!;
            }

            if (propName == "NeedsInstallPath")
            {
                needsInstallPath = reader.GetBoolean()!;
            }
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, LibraryConfig value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean(nameof(LibraryConfig.IsChecked), value.IsChecked);
        writer.WriteString(nameof(LibraryConfig.LibraryName), value.LibraryName);
        writer.WriteString(nameof(LibraryConfig.LibraryType), value.LibraryType.ToString());
        writer.WriteString(nameof(LibraryConfig.InstallPath), value.InstallPath);
        writer.WriteBoolean(nameof(LibraryConfig.NeedsInstallPath), value.NeedsInstallPath);
        writer.WriteEndObject();
    }
}