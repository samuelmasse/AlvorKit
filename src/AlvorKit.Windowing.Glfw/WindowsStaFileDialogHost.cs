namespace AlvorKit.Windowing;

/// <summary>Runs Windows COM file dialogs on one dedicated single-threaded apartment.</summary>
[SupportedOSPlatform("windows")]
internal sealed class WindowsStaFileDialogHost : IFileDialogHost, IDisposable
{
    private const uint PeekMessageRemove = 1;
    private const uint QueueStatusAllInput = 0x04FF;
    private const uint WaitInputAvailable = 4;
    private readonly BlockingCollection<Action> requests = [];
    private readonly Func<Nfd> createNfd;
    private readonly ManualResetEventSlim initialized = new();
    private readonly NfdWindowHandle parent;
    private readonly Thread thread;
    private GlfwFileDialogHost? host;
    private Exception? initializationError;

    /// <summary>Starts the apartment and waits until its request loop is ready.</summary>
    public WindowsStaFileDialogHost(NfdWindowHandle parent) : this(parent, () => new NfdBackend()) { }

    /// <summary>Starts the apartment with an injected NFDe API provider.</summary>
    internal WindowsStaFileDialogHost(NfdWindowHandle parent, Func<Nfd> createNfd)
    {
        this.parent = parent;
        this.createNfd = createNfd;
        thread = new(Run) { IsBackground = true, Name = "AlvorKit NFDe" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        initialized.Wait();
        if (initializationError is not null)
            ExceptionDispatchInfo.Capture(initializationError).Throw();
    }

    /// <inheritdoc />
    public string? OpenFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null)
    {
        var copiedFilters = filters.ToArray();
        return Invoke(dialogs => dialogs.OpenFile(copiedFilters, defaultPath));
    }

    /// <inheritdoc />
    public string[]? OpenFiles(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null)
    {
        var copiedFilters = filters.ToArray();
        return Invoke(dialogs => dialogs.OpenFiles(copiedFilters, defaultPath));
    }

    /// <inheritdoc />
    public string? SaveFile(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath = null, string? defaultName = null)
    {
        var copiedFilters = filters.ToArray();
        return Invoke(dialogs => dialogs.SaveFile(copiedFilters, defaultPath, defaultName));
    }

    /// <inheritdoc />
    public string? PickFolder(string? defaultPath = null) => Invoke(dialogs => dialogs.PickFolder(defaultPath));

    /// <inheritdoc />
    public string[]? PickFolders(string? defaultPath = null) => Invoke(dialogs => dialogs.PickFolders(defaultPath));

    /// <summary>Stops accepting requests and releases NFDe on its apartment thread.</summary>
    public void Dispose()
    {
        requests.CompleteAdding();
        thread.Join();
        requests.Dispose();
        initialized.Dispose();
    }

    /// <summary>Creates the apartment-owned host and processes requests until disposal.</summary>
    private void Run()
    {
        try
        {
            host = new(createNfd(), parent);
        }
        catch (Exception error)
        {
            initializationError = error;
            initialized.Set();
            return;
        }

        initialized.Set();
        foreach (var request in requests.GetConsumingEnumerable())
            request();
        host.Dispose();
    }

    /// <summary>Runs one synchronous operation and preserves its original exception.</summary>
    private TResult Invoke<TResult>(Func<GlfwFileDialogHost, TResult> operation)
    {
        var completion = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        requests.Add(() =>
        {
            try
            {
                completion.SetResult(operation(host!));
            }
            catch (Exception error)
            {
                completion.SetException(error);
            }
        });

        while (!completion.Task.IsCompleted)
        {
            MsgWaitForMultipleObjectsEx(0, 0, 10, QueueStatusAllInput, WaitInputAvailable);
            while (PeekMessage(out var message, 0, 0, 0, PeekMessageRemove))
            {
                TranslateMessage(in message);
                DispatchMessage(in message);
            }
        }

        return completion.Task.GetAwaiter().GetResult();
    }

    [DllImport("user32.dll", EntryPoint = "MsgWaitForMultipleObjectsEx")]
    private static extern uint MsgWaitForMultipleObjectsEx(uint count, nint handles, uint milliseconds, uint wakeMask, uint flags);

    [DllImport("user32.dll", EntryPoint = "PeekMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PeekMessage(out NativeMessage message, nint window, uint minimum, uint maximum, uint remove);

    [DllImport("user32.dll", EntryPoint = "TranslateMessage")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TranslateMessage(in NativeMessage message);

    [DllImport("user32.dll", EntryPoint = "DispatchMessageW")]
    private static extern nint DispatchMessage(in NativeMessage message);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMessage
    {
        internal nint Window;
        internal uint Value;
        internal nuint WParam;
        internal nint LParam;
        internal uint Time;
        internal int PointX;
        internal int PointY;
        internal uint Private;
    }
}
