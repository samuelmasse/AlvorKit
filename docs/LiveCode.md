# LiveCode Development Host

LiveCode lets an explicitly enabled AlvorKit development process expose its
tracked injector scope graph and three complementary interaction surfaces:

- submitted C# constructed and run inside one exact active injector scope;
- discoverable, versioned structured bridges run directly on the game thread.
- frozen-only C# run from a dedicated thread when the frame heartbeat stalls.

`AlvorKit.Engine.LiveCode` automatically contributes an `alvorsense` bridge to
ordinary `RootLoop` games. It accepts the same input, gesture, frame, state, and
screenshot commands as AlvorSense without requiring an agent to generate C#.
Games can register narrower domain bridges for frequent operations while
retaining arbitrary C# as the escape hatch.

`AlvorKit.Engine.SourceUpdate` can register a `source-update` bridge in the same
control plane. Its external coordinator compiles a normal edit to an original
project file and submits verified metadata deltas at the game safe-frame
boundary.

LiveCode is arbitrary in-process code execution. It is intended only for trusted
local development. Do not reference or enable the host from a packed game,
server deployment, or process that handles untrusted code.

## Architecture

LiveCode has four separate responsibilities:

- `AlvorKit.Injection` continues to construct and cache dependencies.
- `AlvorKit.Injection.Graph` creates an explicit lifetime graph above injector
  scopes. It assigns stable IDs, records parents and siblings, and releases
  ended scope objects.
- `AlvorKit.LiveCode` owns discovery, the authenticated loopback protocol,
  reference reporting, collectible command loading, the bridge registry, and a
  game-thread queue. By default, it also owns a dormant dedicated inspection
  thread and an allocation-free frame heartbeat.
- `AlvorKit.Script.LiveCode` discovers targets and compiles submitted source
  with Roslyn outside the game process.

`AlvorKit.Engine.LiveCode` supplies the optional `RootLiveCode` composition for
games using `RootLoop`. It pumps queued work at the window loop's safe
pre-update dispatch boundary, before an engine update is active.

`AlvorKit.Engine.SourceUpdate` is a separate development-only composition. It
registers its versioned bridge in the same registry and applies a queued delta
at the same safe-frame boundary.

The target binds only to `127.0.0.1` on an ephemeral port by default. It writes a
random capability token into a per-user discovery manifest and removes that
manifest during normal shutdown.

## Enable A RootLoop Game

Reference `AlvorKit.Engine.LiveCode` only from the development executable.
Reference `AlvorKit.Injection.Graph` from the game packages that create or end
tracked scopes. Reference `AlvorKit.LiveCode.Generator` from each development
executable as an analyzer so LiveCode submissions inherit that executable's
resolved project-wide imports:

```xml
<ProjectReference Include="..\AlvorKit.LiveCode.Generator\AlvorKit.LiveCode.Generator.csproj"
    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

A development boot state can enable the graph and host before it creates the
application scope:

```csharp
[Root]
public class RootLoadDevelopmentState(
    Injector injector,
    RootScope root,
    RootScripts scripts) : State
{
    public override void Load()
    {
        var scopes = new RootLiveCode(
            injector,
            root,
            scripts,
            new("MyGame")).Enable();

        var app = scopes.Scope<AppScope>(root, "Development app");
        app.Get<AppStart>().Run();
    }
}
```

`RootLiveCode.Enable` registers the graph as an unmarked root-injector
dependency and registers its bridge registry alongside it. Services in child
scopes can therefore request `InjectorScopeGraph` or
`LiveCodeBridgeRegistry` normally.

## Register A Predefined Bridge

A bridge advertises a stable name, positive version, JSON request schema,
mutation flag, and required lease:

```csharp
public sealed class ColonyBridge(Colonies colonies) : ILiveCodeBridge
{
    public LiveCodeBridgeDescriptor Descriptor { get; } = new(
        "colony",
        1,
        "Pulse one colony by exact name.",
        true,
        LiveCodeBridgeLease.None,
        JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string" },
                strength = new { type = "number" }
            },
            required = new[] { "name" }
        }));

    public void Run(LiveCodeBridgeContext output, JsonElement request)
    {
        var colony = colonies.Find(request.GetProperty("name").GetString()!);
        var strength = request.TryGetProperty("strength", out var value)
            ? value.GetSingle()
            : 1f;
        colony.Pulse(strength);
        output.Value("colony", colony.Name);
        output.Value("strength", strength);
    }
}
```

Register it through the `RootLiveCode` instance. Registration is thread-safe
and bridge discovery reflects the current registry:

```csharp
var liveCode = new RootLiveCode(injector, root, scripts, options);
var scopes = liveCode.Enable();
liveCode.Bridges.Register(new ColonyBridge(colonies));
```

Bridge artifacts such as screenshots are returned as bytes to the client. The
game process does not interpret caller-side output paths.

## Own Scope Lifetimes

Create every lifetime that should be visible and executable through the graph:

```csharp
var colony = scopes.Scope<ColonyScope>(
    module,
    "Mountainhome");
```

Several siblings with the same concrete scope type remain distinct:

```text
RootScope
└── AppScope
    └── ModuleScope
        ├── ColonyScope "Mountainhome"
        ├── ColonyScope "Scenario preview"
        └── ColonyScope "Remote client"
```

Use `Run` for temporary scopes whose work is synchronous:

```csharp
scopes.Run<ColonyLoaderScope>(
    colony,
    loader => loader.Get<ColonyLoader>().Run(),
    "Initial colony load");
```

End long-lived scopes explicitly after their tracked children have ended:

```csharp
scopes.End(
    colony,
    ending =>
    {
        var loader = ending.Scope<ColonyLoaderScope>();
        loader.Get<ColonyFrontendUnloader>().Run();
        loader.Get<ColonyUnloader>().Run();
    });
```

The node becomes `Ending` before teardown starts, so LiveCode rejects new work.
It becomes `Ended` afterward, and the graph drops its strong reference while
retaining diagnostic tombstone metadata. A parent cannot end while a tracked
child remains active.

Calling the injector's raw `Scope<T>()` bypasses this lifetime graph. Reserve
raw scopes for intentionally untracked implementation details, or use
`InjectorScopeGraph.Run` for visible temporary scopes.

## Write A Command

A submission declares exactly one top-level class implementing
`ILiveCodeCommand`. Mark it with the attribute belonging to its target scope:

```csharp
[Colony]
public sealed class InspectColony(
    ColonyClock clock,
    ColonyDwarves dwarves,
    ColonyJobs jobs) : ILiveCodeCommand
{
    public void Run(LiveCodeContext output)
    {
        output.WriteLine($"Observed tick {clock.Tick}.");
        output.Value("dwarves", dwarves.Count);
        output.Value("queuedJobs", jobs.Count);
    }
}
```

`LiveCodeContext` keeps command output separate from process-wide console
streams. Commands run synchronously. They may inspect or deliberately mutate
their injected services.

The host uses `scope.New(entryType)`, not `Get`, so the temporary command itself
is not cached. Its assembly is loaded into a collectible
`AssemblyLoadContext`. A command can still prevent unloading if it deliberately
stores itself, subscribes to a long-lived event, registers objects, or starts
background work.

## Use The CLI

For agent-led debugging, especially while AlvorSense is driving the same game,
first read [`AgentLiveDevelopment.md`](AgentLiveDevelopment.md). Initialize a
`tmp/live/<workspace-id>/` workspace and pass `--workspace` to session-specific
commands. This pins the exact process identity, records requests and results,
and confines C# submissions to numbered files beneath the workspace's `lc/` or
`lp/` directory. The commands below remain valid without recording for direct
human use.

List running targets:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- list
```

Inspect the exact graph:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- graph `
    --session MyGame
```

Discover predefined bridges and their request schemas:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- bridges `
    --session MyGame
```

Invoke a game bridge with JSON from standard input:

```powershell
@'
{
  "name": "Mountainhome",
  "strength": 2.4
}
'@ | dotnet run --project scripts\AlvorKit.Script.LiveCode -- bridge `
    --session MyGame `
    --name colony `
    --version 1
```

Drive the live window through the built-in AlvorSense bridge:

```powershell
@'
key Tab down
update 0.016
key Tab up
update 0.016
move 620 410
mouse Left down
update 0.016
mouse Left up
update 0.016
screenshot out\live-window.png
'@ | dotnet run --project scripts\AlvorKit.Script.LiveCode -- puppet `
    --session MyGame
```

`puppet` runs the entire batch under a short exclusive input reservation. It
clears pre-existing held input, suppresses and quarantines native keyboard,
pointer, close, and text callbacks, executes the ordered synthetic frames,
releases all synthetic held inputs, and resumes native input at a clean poll
boundary. Resize and move callbacks remain live so drawable-size state cannot
become stale.

Screenshots are read after game drawing and before buffer swap. PNG bytes cross
the authenticated loopback connection and the CLI writes them using the path
from the `screenshot` command.

Execute a source file by stable numeric scope ID:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session MyGame `
    --scope 7 `
    --file out\inspect.cs
```

An exact diagnostic label also works when it is unambiguous:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- exec `
    --session MyGame `
    --scope "Mountainhome" `
    --file out\inspect.cs
```

When `--file` is omitted, the CLI reads source from standard input.

The CLI asks the running target for its managed dependency closure, loaded
extension assemblies, and build-generated global imports. It compiles against
that exact manifest and sends the portable assembly and symbols over the
authenticated local connection. `LiveCodeHostOptions.GlobalUsings` remains
available for exceptional submission-only imports. Compile errors never enter
the game process.

### Inspect A Frozen Game Loop

`FrozenInspection` is enabled by default with a two-second freeze threshold.
Its dedicated managed thread sleeps while the game is healthy, and
`LiveCodeHost.Pump` records one allocation-free heartbeat at every normal
frame boundary. Set `LiveCodeHostOptions.FrozenInspection` to null to disable
the lane.

Read its status without waiting for the game thread:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- frozen status `
    --session MyGame
```

When `isFrozen` is false, the frozen lane rejects execution with
`GameRunning`. After the heartbeat exceeds `FreezeThreshold`, submit the same
ordinary `ILiveCodeCommand` used by `exec`:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- frozen exec `
    --session MyGame `
    --scope "Mountainhome" `
    --file out\inspect-frozen.cs
```

There is no alternate command contract or instance model. The dedicated thread
resolves the exact graph scope and calls `scope.New(entryType)`, so constructor
injection behaves exactly like normal LiveCode. The result includes heartbeat
snapshots from immediately before and after execution plus the managed
inspector thread ID.

This mode assumes the CLR and injector are healthy and only the game-frame
thread stopped progressing. If the freeze holds the injector lock, blocks the
CLR, suspends every managed thread, or prevents JIT/GC activity, the submitted
command can also block. Frozen inspection intentionally does not pretend to
solve those hard-freeze cases.

## Source-file updates

An editable target launched through AlvorSense can expose Source Update through
the same LiveCode host:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- source start `
    --workspace MyWorkspace
dotnet run --project scripts\AlvorKit.Script.LiveCode -- source apply `
    --workspace MyWorkspace --source Game\Service.cs --diff service.diff
dotnet run --project scripts\AlvorKit.Script.LiveCode -- source status `
    --workspace MyWorkspace
```

See [`AgentLiveDevelopment.md`](AgentLiveDevelopment.md) for immutable launch,
diff, compiler-generation, evidence, and cleanup rules.

## Execution Semantics

Network handling and compilation do not touch game state. Normal C# and bridge
requests wait in one ordered concurrent queue until `LiveCodeHost.Pump` runs.
`RootLiveCode` pumps at the window loop's pre-update dispatch boundary; custom
loops call `Pump` at their own safe boundary. Frozen C# uses a separate
single-reader queue and dedicated thread, and is accepted only after at least
one recorded frame followed by a stale heartbeat.

Immediately before loading the command, the game thread resolves the requested
scope ID through `InjectorScopeGraph`. An ended scope produces `ScopeEnded`; the
host never substitutes another scope with the same type or label.

Commands and bridges cannot be safely force-cancelled. A client timeout can
stop waiting, but work that blocks either execution thread may require
restarting the development process. `LiveCodeHostOptions` can independently
disable arbitrary C# (`EnableCodeExecution`) or predefined bridges
(`EnableBridges`), while a null `FrozenInspection` keeps the extra thread and
protocol surface completely disabled. All surfaces remain trusted development
tools rather than sandboxes.

## Demo

`demos/AlvorKit.Engine.LiveCode.Demo` is a real `RootLoop` game with an
interactive, rendered mycelial observatory. Three animated bioluminescent
colonies are three simultaneous child scopes orbiting a central sun, and the
side panel draws their live injection graph. Mouse and keyboard input directly
manipulate the scoped state.

Checked-in submissions can visibly rewrite one exact colony's morphology,
palette, population, and motion; create and end a nested diagnostic scope;
recompose all colonies; create a fourth sibling executor; and retire another
scope while preserving its graph tombstone. The demo can also deliberately
freeze its game thread, inspect an exact colony from the dedicated lane, and
release the original thread without restarting.

See the demo README for the short walkthrough.
