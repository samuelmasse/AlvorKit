namespace AlvorKit;

/// <summary>Records MSBuild's actual reads, including missing conditional imports and external dependencies.</summary>
internal class SolutionEvaluationFiles(SolutionWatchInputs inputs) : Microsoft.Build.FileSystem.MSBuildFileSystemBase
{
    /// <summary>Subscribes before MSBuild opens XML or another evaluation input.</summary>
    public override TextReader ReadFile(string path) { inputs.File(path); return base.ReadFile(path); }

    /// <summary>Subscribes before streamed project and import reads.</summary>
    public override Stream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
    {
        inputs.File(path);
        return base.GetFileStream(path, mode, access, share);
    }

    /// <summary>Subscribes before text-valued evaluation reads.</summary>
    public override string ReadFileAllText(string path) { inputs.File(path); return base.ReadFileAllText(path); }

    /// <summary>Subscribes before binary-valued evaluation reads.</summary>
    public override byte[] ReadFileAllBytes(string path) { inputs.File(path); return base.ReadFileAllBytes(path); }

    /// <summary>Observes import candidates before MSBuild's XML cache opens them outside the evaluation filesystem.</summary>
    public override bool FileExists(string path) { inputs.File(path); return base.FileExists(path); }

    /// <summary>Observes conditional directory existence, including absent generated project directories.</summary>
    public override bool DirectoryExists(string path) { inputs.Exists(path); return base.DirectoryExists(path); }

    /// <summary>Observes conditional candidates, which may also be imported through MSBuild's separate XML cache.</summary>
    public override bool FileOrDirectoryExists(string path) { inputs.File(path); return base.FileOrDirectoryExists(path); }

    /// <summary>Observes metadata consulted during evaluation.</summary>
    public override FileAttributes GetAttributes(string path) { inputs.File(path); return base.GetAttributes(path); }

    /// <summary>Observes timestamps consulted during evaluation.</summary>
    public override DateTime GetLastWriteTimeUtc(string path) { inputs.File(path); return base.GetLastWriteTimeUtc(path); }

    /// <summary>Observes wildcard imports and items, including the creation of their first matching file.</summary>
    public override IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option)
    {
        inputs.Glob(path, pattern, option == SearchOption.AllDirectories, false);
        return base.EnumerateFiles(path, pattern, option);
    }

    /// <summary>Observes directory membership used by recursive globs.</summary>
    public override IEnumerable<string> EnumerateDirectories(string path, string pattern, SearchOption option)
    {
        inputs.Glob(path, pattern, option == SearchOption.AllDirectories, false);
        return base.EnumerateDirectories(path, pattern, option);
    }

    /// <summary>Observes mixed directory queries performed by the evaluator.</summary>
    public override IEnumerable<string> EnumerateFileSystemEntries(string path, string pattern, SearchOption option)
    {
        inputs.Glob(path, pattern, option == SearchOption.AllDirectories, false);
        return base.EnumerateFileSystemEntries(path, pattern, option);
    }
}
