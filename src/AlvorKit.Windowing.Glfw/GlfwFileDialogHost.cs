namespace AlvorKit.Windowing;

/// <summary>GLFW-parented Native File Dialog Extended implementation.</summary>
public sealed class GlfwFileDialogHost : IFileDialogHost, IDisposable
{
    private const nuint InterfaceVersion = (nuint)NfdEnum.InterfaceVersion;
    private readonly Nfd? nfd;
    private readonly NfdWindowHandle parent;
    private IFileDialogHost? windowsHost;
    private bool initialized;
    private bool disposed;

    /// <summary>Creates a lazily initialized NFDe host associated with an existing GLFW window.</summary>
    public GlfwFileDialogHost(Glfw glfw, GlfwWindow window)
    {
        parent = GlfwFileDialogParent.Get(glfw, window);
        if (!OperatingSystem.IsWindows())
            nfd = new NfdBackend();
    }

    /// <summary>Creates a lazily initialized host with an injected NFDe API.</summary>
    internal GlfwFileDialogHost(Nfd nfd, NfdWindowHandle parent)
    {
        this.nfd = nfd;
        this.parent = parent;
    }

    /// <inheritdoc />
    public unsafe string? OpenFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null)
    {
        if (WindowsHost() is { } host)
            return host.OpenFile(filters, defaultPath);

        EnsureInitialized();
        using var memory = new NfdeDialogMemory(filters, defaultPath);
        fixed (NfdUtf8FilterItem* nativeFilters = memory.Filters)
        {
            var args = OpenArguments(nativeFilters, memory);
            var result = nfd!.OpenDialogU8WithImpl(InterfaceVersion, out var path, &args);
            return CompletePath(result, path);
        }
    }

    /// <inheritdoc />
    public unsafe string[]? OpenFiles(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null)
    {
        if (WindowsHost() is { } host)
            return host.OpenFiles(filters, defaultPath);

        EnsureInitialized();
        using var memory = new NfdeDialogMemory(filters, defaultPath);
        fixed (NfdUtf8FilterItem* nativeFilters = memory.Filters)
        {
            var args = OpenArguments(nativeFilters, memory);
            var result = nfd!.OpenDialogMultipleU8WithImpl(InterfaceVersion, out var paths, &args);
            return CompletePathSet(result, paths);
        }
    }

    /// <inheritdoc />
    public unsafe string? SaveFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null, string? defaultName = null)
    {
        if (WindowsHost() is { } host)
            return host.SaveFile(filters, defaultPath, defaultName);

        EnsureInitialized();
        using var memory = new NfdeDialogMemory(filters, defaultPath, defaultName);
        fixed (NfdUtf8FilterItem* nativeFilters = memory.Filters)
        {
            var args = new NfdSaveDialogUtf8Args
            {
                FilterList = nativeFilters,
                FilterCount = checked((uint)memory.Filters.Length),
                DefaultPath = memory.DefaultPath,
                DefaultName = memory.DefaultName,
                ParentWindow = parent
            };
            var result = nfd!.SaveDialogU8WithImpl(InterfaceVersion, out var path, &args);
            return CompletePath(result, path);
        }
    }

    /// <inheritdoc />
    public unsafe string? PickFolder(string? defaultPath = null)
    {
        if (WindowsHost() is { } host)
            return host.PickFolder(defaultPath);

        EnsureInitialized();
        using var memory = new NfdeDialogMemory([], defaultPath);
        var args = new NfdPickFolderUtf8Args { DefaultPath = memory.DefaultPath, ParentWindow = parent };
        var result = nfd!.PickFolderU8WithImpl(InterfaceVersion, out var path, &args);
        return CompletePath(result, path);
    }

    /// <inheritdoc />
    public unsafe string[]? PickFolders(string? defaultPath = null)
    {
        if (WindowsHost() is { } host)
            return host.PickFolders(defaultPath);

        EnsureInitialized();
        using var memory = new NfdeDialogMemory([], defaultPath);
        var args = new NfdPickFolderUtf8Args { DefaultPath = memory.DefaultPath, ParentWindow = parent };
        var result = nfd!.PickFolderMultipleU8WithImpl(InterfaceVersion, out var paths, &args);
        return CompletePathSet(result, paths);
    }

    /// <summary>Calls <c>NFD_Quit</c> once when the native session was initialized.</summary>
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        if (windowsHost is IDisposable disposable)
            disposable.Dispose();
        else if (initialized)
            nfd!.Quit();
    }

    /// <summary>Creates the Windows STA host on the first dialog request.</summary>
    private IFileDialogHost? WindowsHost()
    {
        if (!OperatingSystem.IsWindows() || nfd is not null)
            return null;

        return windowsHost ??= new WindowsStaFileDialogHost(parent);
    }

    /// <summary>Starts the native session on the first dialog request.</summary>
    private void EnsureInitialized()
    {
        if (initialized)
            return;
        if (nfd!.Init() != NfdResult.Okay)
            throw Failure("initialization");
        initialized = true;
    }

    /// <summary>Creates shared open-dialog arguments over the pinned filter records.</summary>
    private unsafe NfdOpenDialogUtf8Args OpenArguments(NfdUtf8FilterItem* filters, NfdeDialogMemory memory) =>
        new()
        {
            FilterList = filters,
            FilterCount = checked((uint)memory.Filters.Length),
            DefaultPath = memory.DefaultPath,
            ParentWindow = parent
        };

    /// <summary>Translates one NFDe result and releases a successful path.</summary>
    private string? CompletePath(NfdResult result, nint path)
    {
        if (result == NfdResult.Cancel)
            return null;
        if (result != NfdResult.Okay)
            throw Failure("dialog");

        try
        {
            return Marshal.PtrToStringUTF8(path) ?? throw new InvalidOperationException("NFDe returned an empty path pointer.");
        }
        finally
        {
            nfd!.FreePathU8(path);
        }
    }

    /// <summary>Translates one NFDe path-set result and releases the path set.</summary>
    private string[]? CompletePathSet(NfdResult result, nint paths)
    {
        if (result == NfdResult.Cancel)
            return null;
        if (result != NfdResult.Okay)
            throw Failure("dialog");

        try
        {
            return ReadPaths(paths);
        }
        finally
        {
            nfd!.PathSetFree(paths);
        }
    }

    /// <summary>Copies every UTF-8 path from an NFDe path-set enumerator.</summary>
    private unsafe string[] ReadPaths(nint paths)
    {
        if (nfd!.PathSetGetEnum(paths, out var enumerator) != NfdResult.Okay)
            throw Failure("path enumeration");

        try
        {
            var result = new List<string>();
            while (true)
            {
                if (nfd.PathSetEnumNextU8(&enumerator, out var path) != NfdResult.Okay)
                    throw Failure("path enumeration");
                if (path == 0)
                    return [.. result];

                try
                {
                    result.Add(Marshal.PtrToStringUTF8(path) ?? throw new InvalidOperationException("NFDe returned an empty path pointer."));
                }
                finally
                {
                    nfd.PathSetFreePathU8(path);
                }
            }
        }
        finally
        {
            nfd.PathSetFreeEnum(&enumerator);
        }
    }

    /// <summary>Builds the single managed error raised at the NFDe boundary.</summary>
    private InvalidOperationException Failure(string operation)
    {
        nfd!.GetError(out var message);
        nfd.ClearError();
        return new($"NFDe {operation} failed: {message ?? "unknown native error"}");
    }
}
