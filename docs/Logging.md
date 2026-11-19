# Logging

`AlvorKit.Logging` provides the application log shared by AlvorKit games,
dedicated servers, and other custom hosts. The producer API is buffered per
thread so normal log calls do not wait for console output.

## Standard Games

`RootLoop.RunGlfw` starts a `RootHost` before initializing GLFW.
The host owns one `LogRuntime` and registers its `Log` with the injector.
Services under the engine root can request `Log` through their constructors.

```csharp
using AlvorKit;

[App]
public class AppLoader(Log log)
{
    public void Run() => log.Info("Loading app");
}
```

The engine keeps logging alive through state and script unloading, then drains
every published entry before shutting the runtime down.

`AlvorKit.Engine.Root` supplies `AlvorKit.Logging` transitively. Projects
outside the engine dependency tree can reference `AlvorKit.Logging` directly.

Application events and diagnostics should use the injected `Log`, including
messages emitted by states, loaders, simulation services, development hosts,
and engine-loop demos. Do not create a game-local logger or write those
messages directly through `Console`, `Debug`, or `Trace`.

Direct `Console` or `TextWriter` output remains appropriate when the text is
the command's data rather than an application event: generated help, compiler-
style diagnostics, machine-readable protocol output, console-only walkthroughs,
benchmark/report tables, interactive terminal painting, and deterministic
trace exports. Dedicated capture files may likewise keep their domain format;
failures while opening or writing such a capture should be reported through
`Log`.

## Dedicated Servers And Custom Hosts

Dedicated server launchers can reference `AlvorKit.Engine.Root.Host` and start
a `RootHost` directly.

```csharp
using AlvorKit;

using var host = RootHost.Start();
var graph = host.Root.Get<InjectorScopeGraph>();
var app = graph.Scope<AppScope>(host.Root);
app.Get<AppServer>().Run();
```

Disposing the host stops its logging worker and flushes pending entries.
Tools that manage a standalone `LogRuntime` can pass a `TextWriter` to its
constructor to capture output. ANSI colors default to enabled only for
`Console.Out` and can be set through `UseColor`. Its `Start`, `Stop`, and
`Dispose` methods support repeated calls in their respective states.

## Levels And Formatting

`Log.Level` is safe to read while other threads write entries. Its default is
`LogLevel.All`. Setting it to `Off` suppresses severity-prefixed entries;
`Raw` output always remains enabled.

Each severity method accepts plain strings, exceptions, values, and composite
format strings with up to eight typed arguments. Accepted entries contain a UTC
timestamp, level, caller filename, and caller line. Exception details follow
the message on the next line.

Formatting uses reusable thread-owned storage from `AlvorKit.Text`; it has no
mutable process-wide formatter registry. Strings, string builders, read-only
character memory, built-in span-formattable values, and AlvorKit maths types
format without boxing or steady-state allocation. Any custom value that
implements `ISpanFormattable` receives the same path automatically. A value
that only exposes allocating `ToString` or `IFormattable` APIs necessarily uses
that fallback instead.

Producer buffers grow only when a thread first logs, exhausts its current
buffer, or submits an unusually large message. Filtered calls return before
opening a formatter. Console collection and rendering run on the runtime worker
rather than game threads.
