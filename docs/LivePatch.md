# LivePatch

LivePatch lets trusted development tooling replace an ordinary C# method in an
already running optimized AlvorKit game. It combines CoreCLR ReJIT with the
authoritative injector scope graph, so the same method can behave differently
for two live colony scopes without modifying the call site.

It is an opt-in development add-on:

- `AlvorKit.Injection.Graph` tracks scope identity, parentage, lifetime, and
  weak instance provenance;
- `AlvorKit.Interception` owns exact method/trampoline/native contracts;
- `AlvorKit.LivePatch` owns selectors and patch leases; and
- `AlvorKit.Engine.LivePatch` composes LivePatch with `RootLoop` and the
  LiveCode bridge.

## Enable it

Enable `RootLiveCode`, then enable `RootLivePatch` with the same graph and
bridge registry:

```csharp
var liveCode = new RootLiveCode(
    injector,
    root,
    scripts,
    new("MyGame.Dev"));
var graph = liveCode.Enable();

if (RootLivePatch.IsProfilerConfigured)
{
    _ = new RootLivePatch(
        injector,
        root,
        scripts,
        graph,
        liveCode.Bridges).Enable();
}
```

Launch that exact Release executable with the profiler variables described in
[`Interception.md`](Interception.md). VS Code can launch the managed DLL
directly under `coreclr`, so breakpoints and LivePatch coexist in one process.
Launching the same Dev project normally from Visual Studio simply omits the
profiler and retains ordinary debugging.

## Select a receiver set

Every install has one explicit selector:

- `exact-instance`: one object reference;
- `exact-scope`: receivers owned by one scope;
- `descendants`: receivers owned by one scope or its active descendants; or
- `all`: every eligible receiver, and the only selector for static methods.

The graph records provenance when Injection constructs, adds, or binds an
object. It uses weak keys, so graphing does not keep application objects alive.
A scope enters `Ending` synchronously before teardown; matching patch
registrations stop dispatching at that boundary. When the last registration
for a method leaves, native original-IL restoration begins.

Overlapping selectors for the same method are rejected. Composition must be
explicit instead of depending on registration order.

## Write a handler

Agent-authored handlers are temporary experiments. Follow
[`AgentLiveDevelopment.md`](AgentLiveDevelopment.md), write each revision to a
numbered file beneath `tmp/live/<workspace-id>/lp/`, and pass `--workspace` to
install, replace, remove, and status commands. The workspace ledger prevents a
session from being declared clean while a patch is still active, removing, or
known to require a restart.

The handler has one `[LivePatchHandler]` method. For an instance target, its
first parameter is the receiver and the rest exactly match the target:

```csharp
using AlvorKit.Engine.LiveCode.Demo;
using AlvorKit.LivePatch;

public sealed class FasterOrbit(ColonySky sky)
{
    [LivePatchHandler]
    public void Run(ColonyGarden receiver, double delta)
    {
        receiver.SolarAngle += delta * 4.8;
        receiver.OrbitRadius = 168f;
        receiver.SporeCount = 72;
        sky.Weather = "agent-authored solar ribbons";
    }
}
```

Constructor dependencies are resolved from the selected executor scope. They
must be types already loaded by the game, which prevents Injection caches from
retaining the collectible submitted assembly.

Install it:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch install `
  --session mycelial-observatory `
  --scope "Moon Garden" `
  --selector exact-scope `
  --target "AlvorKit.Engine.LiveCode.Demo.ColonyGarden::Update" `
  --target-assembly AlvorKit.Engine.LiveCode.Demo `
  --name "Moon solar ribbons" `
  --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\FasterOrbit.cs
```

The CLI requests the live process's exact reference manifest, compiles outside
the game, and sends the assembly and symbols through the authenticated
loopback LiveCode bridge. Compilation failures never enter the game.

Inspect, atomically replace, and remove:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch list `
  --session mycelial-observatory

dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch status `
  --session mycelial-observatory --patch 1

dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch replace `
  --session mycelial-observatory --patch 1 `
  --file demos\AlvorKit.Engine.LiveCode.Demo\Submissions\ReverseOrbit.cs

dotnet run --project scripts\AlvorKit.Script.LiveCode -- patch remove `
  --session mycelial-observatory --patch 1
```

Replacement publishes a new managed handler atomically and does not request
another ReJIT. Removal stops managed dispatch immediately; restoring original
IL and existing inliners is asynchronous and observable.

## Failures and unloading

A generated exact trampoline catches a submitted handler exception, records
the first failure, disables future dispatch, returns the exact default for that
single failing invocation, and releases the handler. Subsequent calls execute
the original method. The session reports `Failed` while retaining the native
revert evidence and diagnostic text.

Submitted handlers live in collectible `AssemblyLoadContext` instances.
Replace, remove, scope end, or handler failure unloads the old context after
the last in-flight call. Status reports `retained`, `unloading`, or
`collected`.

## Security and v1 boundary

LivePatch runs trusted arbitrary code in the game process. It is not a
sandbox, is disabled unless explicitly composed and profiler-launched, and
must not be enabled in a packed or untrusted game.

Version 1 exposes replacement semantics. `Before`, `After`, and `Around`
remain gated on a separately proven original-call strategy; the current
implementation does not pretend that fallback-to-original is an invocable
continuation from inside a handler.
