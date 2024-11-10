using System.Text.RegularExpressions;

namespace DLSSUpdater.Helpers;

public partial class SafeVersion : IComparable<SafeVersion>, IEquatable<SafeVersion?>, ICloneable, IComparable
{
    public int Major => _versionParts.Count > 0 ? _versionParts[0] : 0;
    public int Minor => _versionParts.Count > 1 ? _versionParts[1] : 0;
    public int Build => _versionParts.Count > 2 ? _versionParts[2] : 0;
    public int Revision => _versionParts.Count > 3 ? _versionParts[3] : 0;
    
    private readonly List<int> _versionParts = [];

    public SafeVersion(string? version)
    {
        if (string.IsNullOrEmpty(version))
        {
            return;
        }
        
        // We need to parse all elements that might be there
        // Use regex to extract digits and group into version format
        _versionParts = VersionRegex().Matches(version)
            .Select(m => int.Parse(m.Value))
            .ToList();
    }

    public SafeVersion(Version? version)
    {
        if (version == null)
        {
            return;
        }

        FillParts(version.Major);
        FillParts(version.Minor);
        FillParts(version.Build);
        FillParts(version.Revision);
    }
    
    private SafeVersion(SafeVersion version)
    {
        foreach (var part in version._versionParts)
        {
            _versionParts.Add(part);            
        }
    }

    public void AddVersionPart(int part)
    {
        _versionParts.Add(part);
    }
    
    public int CompareTo(SafeVersion? other)
    {
        return other == null ? 1 : ((Version)this).CompareTo((Version)other);
    }
    
    public int CompareTo(object? obj)
    {
        return CompareTo(obj as SafeVersion);
    }

    public bool Equals(SafeVersion? other)
    {
        return other != null && ((Version)this).Equals((Version)other);
    }

    public object Clone()
    {
        return new SafeVersion(this);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SafeVersion);
    }
    
    public static bool operator ==(SafeVersion? v1, SafeVersion? v2)
    {
        // Test "right" first to allow branch elimination when inlined for null checks (== null)
        // so it can become a simple test
        if (v2 is null)
        {
            return v1 is null;
        }

        // Quick reference equality test prior to calling the virtual Equality
        return ReferenceEquals(v2, v1) || v2.Equals(v1);
    }

    public static bool operator !=(SafeVersion? v1, SafeVersion? v2) => !(v1 == v2);

    public static bool operator <(SafeVersion? v1, SafeVersion? v2)
    {
        if (v1 is null)
        {
            return v2 is not null;
        }

        return v1.CompareTo(v2) < 0;
    }

    public static bool operator <=(SafeVersion? v1, SafeVersion? v2)
    {
        if (v1 is null)
        {
            return true;
        }

        return v1.CompareTo(v2) <= 0;
    }

    public static bool operator >(SafeVersion? v1, SafeVersion? v2) => v2 < v1;

    public static bool operator >=(SafeVersion? v1, SafeVersion? v2) => v2 <= v1;

    public override int GetHashCode()
    {
       return HashCode.Combine(_versionParts); 
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Build}.{Revision}";
    }

    public static implicit operator Version(SafeVersion version)
    {
        return version._versionParts.Count switch
        {
            2 => new Version(version.Major, version.Minor),
            3 => new Version(version.Major, version.Minor, version.Build),
            4 => new Version(version.Major, version.Minor, version.Build, version.Revision),
            _ => new Version()
        };
    }

    private void FillParts(int part)
    {
        if (part == -1)
        {
            return;
        }
        
        _versionParts.Add(part);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex VersionRegex();
}