# File Dialogs

AlvorKit file dialogs present the operating system's native chooser while keeping NFDe, GLFW native accessors, and OS window handles out of game code.

`RootLoop.RunGlfw` creates the dialog host after GLFW and adds `RootFileDialogs` to the root scope. The native session initializes on the first dialog request, so applications that never open a chooser do not require a desktop portal or session bus during startup.

Dialog calls are synchronous and should originate on the window thread. Windows dispatches the NFDe operation to a dedicated COM STA while pumping the caller's window messages; macOS and Linux execute the native operation directly on the calling thread.

## Operations

`RootFileDialogs` and `FileDialogs` provide:

- `OpenFile`: select one existing file.
- `OpenFiles`: select existing files.
- `SaveFile`: select a destination path and optional default file name.
- `PickFolder`: select one folder.
- `PickFolders`: select folders.

Every operation returns `null` when the user cancels. Single-selection operations otherwise return one UTF-8 path; multiple-selection operations return a `string[]`. NFDe failures throw one `InvalidOperationException` containing the native error text.

## Filters

File filters pair a friendly name with comma-separated extensions that omit dots and wildcards:

```csharp
FileDialogFilter[] filters =
[
    new("NES ROM", "nes"),
    new("Archives", "zip,7z")
];

var path = dialogs.OpenFile(filters);
if (path is not null)
    OpenRom(path);
```

Pass an empty span or collection to show all files. NFDe also supplies its platform all-files fallback.

The caller decides what to do around the modal operation. An emulator menu can pause audio before opening a chooser and resume it afterwards; the windowing layer does not mutate game state.

## Linux

AlvorKit's NFDe native package uses the xdg-desktop-portal backend through D-Bus. Opening a dialog requires a desktop session with a working portal implementation. Current AlvorKit Linux GLFW binaries use X11, allowing NFDe 1.3.0 to parent the portal dialog to the game window.

## Demo

After the NFDe native and binding packages are available, run:

```powershell
dotnet run --project demos\AlvorKit.Windowing.Demo.FileDialogs
```

The demo opens a normal GLFW window. Press:

- `O` to open one file.
- `M` to open multiple files.
- `S` to choose a save path.
- `F` to pick one folder.
- `P` to pick multiple folders.
- `Esc` to exit.

Pass `-- --open` to open the single-file chooser immediately, which is useful for an OS-level smoke test.

Use AlvorEye for automated interaction with the chooser because it is an external OS window. Unit and AlvorSense tests should inject an `IFileDialogHost` fake instead of opening a real dialog.
