namespace AlvorKit;

/// <summary>Maps supported runtime identifiers to Clang targets and native preprocessor conditions.</summary>
public static class BindingRuntimeTarget
{
    /// <summary>Returns the exact ABI used to parse a runtime's native declarations.</summary>
    public static string Triple(string runtime) => runtime switch
    {
        "win-x64" => "x86_64-pc-windows-msvc",
        "win-x86" => "i686-pc-windows-msvc",
        "win-arm64" => "aarch64-pc-windows-msvc",
        "linux-x64" => "x86_64-unknown-linux-gnu",
        "linux-arm64" => "aarch64-unknown-linux-gnu",
        "linux-arm" => "armv7-unknown-linux-gnueabihf",
        "osx-x64" => "x86_64-apple-darwin",
        "osx-arm64" => "aarch64-apple-darwin",
        _ => throw new ArgumentException($"Unsupported binding runtime: {runtime}", nameof(runtime))
    };

    /// <summary>Returns a compiler condition selecting one runtime's native layout table.</summary>
    public static string Condition(string runtime) => runtime switch
    {
        "win-x64" => "defined(_WIN32) && defined(_M_X64)",
        "win-x86" => "defined(_WIN32) && defined(_M_IX86)",
        "win-arm64" => "defined(_WIN32) && defined(_M_ARM64)",
        "linux-x64" => "defined(__linux__) && defined(__x86_64__)",
        "linux-arm64" => "defined(__linux__) && defined(__aarch64__)",
        "linux-arm" => "defined(__linux__) && defined(__arm__)",
        "osx-x64" => "defined(__APPLE__) && defined(__x86_64__)",
        "osx-arm64" => "defined(__APPLE__) && defined(__aarch64__)",
        _ => throw new ArgumentException($"Unsupported binding runtime: {runtime}", nameof(runtime))
    };
}
