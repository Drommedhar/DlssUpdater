namespace DlssUpdater.Singletons.AntiCheatChecker;

[Flags]
public enum AntiCheatProvider
{
    None = 0,
    EasyAntiCheat = 1,
    BattlEye = 2,

    All = EasyAntiCheat | BattlEye,
    // TODO: More
}

public interface IAntiCheatProvider
{
    public AntiCheatProvider ProviderType { get; }

    /// <summary>
    ///     Checks a folder for a specific anti cheat being present.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <returns>False if the anti cheat was not found, true otherwise.</returns>
    bool Check(string directory);
}