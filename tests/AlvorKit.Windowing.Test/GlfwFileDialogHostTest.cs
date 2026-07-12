namespace AlvorKit.Windowing.Test;

/// <summary>Tests the managed NFDe ownership and result contract without displaying OS dialogs.</summary>
[TestClass]
public sealed class GlfwFileDialogHostTest
{
    private static readonly NfdWindowHandle Parent = new() { Type = (nuint)NfdWindowHandleType.Windows, Handle = 42 };

    /// <summary>Single-file selection marshals arguments, returns the copied path, and frees native ownership.</summary>
    [TestMethod]
    public void OpenFile_Success_CopiesPathAndArguments()
    {
        using var nfd = new WindowingTestNfd();
        using var host = new GlfwFileDialogHost(nfd, Parent);

        var path = host.OpenFile([new("NES ROM", "nes")], "C:/roms");

        Assert.AreEqual("C:/selected.rom", path);
        Assert.AreEqual("open", nfd.LastOperation);
        Assert.AreEqual("C:/roms", nfd.LastDefaultPath);
        CollectionAssert.AreEqual(new[] { new FileDialogFilter("NES ROM", "nes") }, nfd.LastFilters);
        Assert.AreEqual(Parent, nfd.LastParent);
        Assert.AreEqual((nuint)NfdEnum.InterfaceVersion, nfd.LastVersion);
        Assert.AreEqual(1, nfd.FreePathCount);
    }

    /// <summary>Cancellation is represented by null without native path cleanup.</summary>
    [TestMethod]
    public void OpenFile_Cancel_ReturnsNull()
    {
        using var nfd = new WindowingTestNfd { DialogResult = NfdResult.Cancel };
        using var host = new GlfwFileDialogHost(nfd, Parent);

        Assert.IsNull(host.OpenFile([]));
        Assert.AreEqual(0, nfd.FreePathCount);
    }

    /// <summary>Native failures become one concise managed exception with the NFDe error.</summary>
    [TestMethod]
    public void OpenFile_Error_ThrowsNativeMessage()
    {
        using var nfd = new WindowingTestNfd { DialogResult = NfdResult.Error };
        using var host = new GlfwFileDialogHost(nfd, Parent);

        var error = Assert.ThrowsExactly<InvalidOperationException>(() => host.OpenFile([]));

        StringAssert.Contains(error.Message, "test native error");
        Assert.AreEqual(1, nfd.ClearErrorCount);
    }

    /// <summary>Multiple-file selection copies every enumerated path and releases all path-set ownership.</summary>
    [TestMethod]
    public void OpenFiles_Success_CopiesAndFreesPathSet()
    {
        using var nfd = new WindowingTestNfd();
        using var host = new GlfwFileDialogHost(nfd, Parent);

        var paths = host.OpenFiles([new("ROM", "nes,zip")]);

        CollectionAssert.AreEqual(nfd.Paths, paths);
        Assert.AreEqual(2, nfd.FreePathSetPathCount);
        Assert.AreEqual(1, nfd.FreeEnumeratorCount);
        Assert.AreEqual(1, nfd.FreePathSetCount);
    }

    /// <summary>Save selection forwards the default directory, name, filter, and parent.</summary>
    [TestMethod]
    public void SaveFile_Success_ForwardsSaveArguments()
    {
        using var nfd = new WindowingTestNfd();
        using var host = new GlfwFileDialogHost(nfd, Parent);

        host.SaveFile([new("Text", "txt")], "C:/notes", "draft.txt");

        Assert.AreEqual("save", nfd.LastOperation);
        Assert.AreEqual("C:/notes", nfd.LastDefaultPath);
        Assert.AreEqual("draft.txt", nfd.LastDefaultName);
    }

    /// <summary>Folder operations use their single and multiple NFDe entry points.</summary>
    [TestMethod]
    public void PickFolders_UsesMatchingEntryPoints()
    {
        using var nfd = new WindowingTestNfd();
        using var host = new GlfwFileDialogHost(nfd, Parent);

        Assert.AreEqual("C:/selected.rom", host.PickFolder("C:/"));
        Assert.AreEqual("folder", nfd.LastOperation);
        CollectionAssert.AreEqual(nfd.Paths, host.PickFolders("C:/"));
        Assert.AreEqual("folder-multiple", nfd.LastOperation);
    }

    /// <summary>Initialization is deferred until first use, errors are cleared, and successful disposal quits once.</summary>
    [TestMethod]
    public void Lifetime_InitializesLazilyAndQuitsExactlyOnce()
    {
        using var failed = new WindowingTestNfd { InitResult = NfdResult.Error };
        using var failedHost = new GlfwFileDialogHost(failed, Parent);
        Assert.AreEqual(0, failed.InitCount);
        Assert.ThrowsExactly<InvalidOperationException>(() => failedHost.OpenFile([]));
        Assert.AreEqual(1, failed.ClearErrorCount);
        Assert.AreEqual(0, failed.QuitCount);

        using var idleNfd = new WindowingTestNfd();
        var idleHost = new GlfwFileDialogHost(idleNfd, Parent);
        idleHost.Dispose();
        Assert.AreEqual(0, idleNfd.InitCount);
        Assert.AreEqual(0, idleNfd.QuitCount);

        using var nfd = new WindowingTestNfd();
        var host = new GlfwFileDialogHost(nfd, Parent);
        host.PickFolder();
        host.Dispose();
        host.Dispose();
        Assert.AreEqual(1, nfd.InitCount);
        Assert.AreEqual(1, nfd.QuitCount);
    }

    /// <summary>Windows dialog work, initialization, and teardown stay on the dedicated STA thread.</summary>
    [TestMethod]
    public void WindowsStaHost_RunsNativeSessionOnDedicatedApartment()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Windows STA behavior requires Windows.");
            return;
        }

        var callerThreadId = Environment.CurrentManagedThreadId;
        using var nfd = new WindowingTestNfd();
        var host = new WindowsStaFileDialogHost(Parent, () => nfd);

        var path = host.OpenFile([]);
        host.Dispose();

        Assert.AreEqual("C:/selected.rom", path);
        Assert.AreNotEqual(callerThreadId, nfd.InitThreadId);
        Assert.AreEqual(nfd.InitThreadId, nfd.DialogThreadId);
        Assert.AreEqual(nfd.InitThreadId, nfd.QuitThreadId);
        Assert.AreEqual(ApartmentState.STA, nfd.InitApartmentState);
    }

    /// <summary>Parent construction tags nonzero platform handles and leaves zero handles unset.</summary>
    [TestMethod]
    public void Parent_Create_TagsOnlyNonzeroHandles()
    {
        Assert.AreEqual(default, GlfwFileDialogParent.Create(NfdWindowHandleType.Cocoa, 0));
        Assert.AreEqual(
            new NfdWindowHandle { Type = (nuint)NfdWindowHandleType.X11, Handle = 75 },
            GlfwFileDialogParent.Create(NfdWindowHandleType.X11, 75));
    }

    /// <summary>Empty native filter fields are rejected before invoking NFDe's undefined behavior.</summary>
    [TestMethod]
    public void OpenFile_EmptyFilter_ThrowsArgumentException()
    {
        using var nfd = new WindowingTestNfd();
        using var host = new GlfwFileDialogHost(nfd, Parent);

        Assert.ThrowsExactly<ArgumentException>(() => host.OpenFile([new("ROM", "")]));
    }
}
