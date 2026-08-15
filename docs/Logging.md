# Logging

`AlvorKit.Logging` provides the application log shared by AlvorKit games,
dedicated servers, and other custom hosts. The producer API is buffered per
thread so normal log calls do not wait for console output.

## Standard Games

`RootLoop.RunGlfw` creates one `LogRuntime`, starts it before loading the boot
state, and registers its `Log` above the engine root scope. Root and nested game
scopes can therefore request `Log` directly:

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

Projects that mention `Log` directly should reference
`src/AlvorKit.Logging/AlvorKit.Logging.csproj`. The package does not depend on
graphics, UI, windowing, or the engine loop, so pure, protocol, backend, and
server projects may use it without changing their architectural role.

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

Hosts that do not use `RootLoop` own the runtime explicitly:

```csharp
using AlvorKit;

using var logging = new LogRuntime();
logging.Start();

new Injector()
    .With(logging.Log)
    .Scope<AppScope>()
    .Run(scope => scope.Get<AppServer>().Run());
```

Disposal stops the worker and synchronously flushes entries submitted before
shutdown. `Start` and `Stop` are idempotent in their current state, and
`Dispose` may be called repeatedly.

Pass a `TextWriter` to capture or redirect output. ANSI level colors default to
enabled only for `Console.Out` and can be controlled through
`LogRuntime.UseColor`.

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
