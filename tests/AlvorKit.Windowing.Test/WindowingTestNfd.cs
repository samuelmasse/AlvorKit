namespace AlvorKit.Windowing.Test;

internal sealed class WindowingTestNfd : NfdNoop, IDisposable
{
    private readonly nint error;
    private int pathIndex;

    public WindowingTestNfd() => error = Marshal.StringToCoTaskMemUTF8("test native error");

    public NfdResult InitResult { get; set; } = NfdResult.Okay;
    public NfdResult DialogResult { get; set; } = NfdResult.Okay;
    public string SinglePath { get; set; } = "C:/selected.rom";
    public string[] Paths { get; set; } = ["C:/first.rom", "C:/second.rom"];
    public string LastOperation { get; private set; } = string.Empty;
    public string? LastDefaultPath { get; private set; }
    public string? LastDefaultName { get; private set; }
    public FileDialogFilter[] LastFilters { get; private set; } = [];
    public NfdWindowHandle LastParent { get; private set; }
    public nuint LastVersion { get; private set; }
    public int InitCount { get; private set; }
    public int QuitCount { get; private set; }
    public int ClearErrorCount { get; private set; }
    public int FreePathCount { get; private set; }
    public int FreePathSetPathCount { get; private set; }
    public int FreePathSetCount { get; private set; }
    public int FreeEnumeratorCount { get; private set; }
    public int InitThreadId { get; private set; }
    public int DialogThreadId { get; private set; }
    public int QuitThreadId { get; private set; }
    public ApartmentState InitApartmentState { get; private set; }

    public override NfdResult Init()
    {
        InitCount++;
        InitThreadId = Environment.CurrentManagedThreadId;
        InitApartmentState = Thread.CurrentThread.GetApartmentState();
        return InitResult;
    }

    public override void Quit()
    {
        QuitCount++;
        QuitThreadId = Environment.CurrentManagedThreadId;
    }

    public override void ClearError() => ClearErrorCount++;

    public unsafe override NfdResult OpenDialogU8WithImpl(
        nuint version,
        out nint outPath,
        NfdOpenDialogUtf8Args* args)
    {
        LastOperation = "open";
        Capture(version, args->FilterList, args->FilterCount, args->DefaultPath, 0, args->ParentWindow);
        return Single(out outPath);
    }

    public unsafe override NfdResult OpenDialogMultipleU8WithImpl(
        nuint version,
        out nint outPaths,
        NfdOpenDialogUtf8Args* args)
    {
        LastOperation = "open-multiple";
        Capture(version, args->FilterList, args->FilterCount, args->DefaultPath, 0, args->ParentWindow);
        return Multiple(out outPaths);
    }

    public unsafe override NfdResult SaveDialogU8WithImpl(
        nuint version,
        out nint outPath,
        NfdSaveDialogUtf8Args* args)
    {
        LastOperation = "save";
        Capture(version, args->FilterList, args->FilterCount, args->DefaultPath, args->DefaultName, args->ParentWindow);
        return Single(out outPath);
    }

    public unsafe override NfdResult PickFolderU8WithImpl(
        nuint version,
        out nint outPath,
        NfdPickFolderUtf8Args* args)
    {
        LastOperation = "folder";
        Capture(version, null, 0, args->DefaultPath, 0, args->ParentWindow);
        return Single(out outPath);
    }

    public unsafe override NfdResult PickFolderMultipleU8WithImpl(
        nuint version,
        out nint outPaths,
        NfdPickFolderUtf8Args* args)
    {
        LastOperation = "folder-multiple";
        Capture(version, null, 0, args->DefaultPath, 0, args->ParentWindow);
        return Multiple(out outPaths);
    }

    public override void FreePathU8(nint filePath)
    {
        FreePathCount++;
        Marshal.FreeCoTaskMem(filePath);
    }

    public override nint GetError() => error;

    public override NfdResult PathSetGetEnum(nint pathSet, out NfdPathSetEnumerator outEnumerator)
    {
        pathIndex = 0;
        outEnumerator = default;
        return NfdResult.Okay;
    }

    public unsafe override NfdResult PathSetEnumNextU8(NfdPathSetEnumerator* enumerator, out nint outPath)
    {
        outPath = pathIndex < Paths.Length ? Marshal.StringToCoTaskMemUTF8(Paths[pathIndex++]) : 0;
        return NfdResult.Okay;
    }

    public override void PathSetFreePathU8(nint filePath)
    {
        FreePathSetPathCount++;
        Marshal.FreeCoTaskMem(filePath);
    }

    public unsafe override void PathSetFreeEnum(NfdPathSetEnumerator* enumerator) => FreeEnumeratorCount++;

    public override void PathSetFree(nint pathSet) => FreePathSetCount++;

    public void Dispose() => Marshal.FreeCoTaskMem(error);

    private NfdResult Single(out nint path)
    {
        path = DialogResult == NfdResult.Okay ? Marshal.StringToCoTaskMemUTF8(SinglePath) : 0;
        return DialogResult;
    }

    private NfdResult Multiple(out nint paths)
    {
        paths = DialogResult == NfdResult.Okay ? 1 : 0;
        return DialogResult;
    }

    private unsafe void Capture(
        nuint version,
        NfdUtf8FilterItem* filters,
        uint filterCount,
        nint defaultPath,
        nint defaultName,
        NfdWindowHandle parent)
    {
        DialogThreadId = Environment.CurrentManagedThreadId;
        LastVersion = version;
        LastDefaultPath = Marshal.PtrToStringUTF8(defaultPath);
        LastDefaultName = Marshal.PtrToStringUTF8(defaultName);
        LastParent = parent;
        LastFilters = new FileDialogFilter[filterCount];
        for (var i = 0; i < filterCount; i++)
        {
            LastFilters[i] = new(
                Marshal.PtrToStringUTF8(filters[i].Name)!,
                Marshal.PtrToStringUTF8(filters[i].Spec)!);
        }
    }
}
