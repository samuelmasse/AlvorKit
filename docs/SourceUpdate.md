# Source Update

Source Update applies a normal C# method edit to the original method definition
in a running development process.

The agent edits the real project `.cs` file and submits an immutable unified
diff. A retained external Roslyn coordinator loads the exact project and
immutable launch PE/PDB, emits public Edit-and-Continue metadata, IL, and PDB
deltas, and submits them to the target at the LiveCode safe-frame boundary. The
target calls `MetadataUpdater.ApplyUpdate` for the original loaded assembly.

## What changes

If this method is edited:

```csharp
public sealed class WeatherService(
    Forecast forecast,
    Clock clock)
{
    private int samples;
    private readonly float smoothing = 0.2f;

    public float Update(float value)
    {
        samples++;
        return forecast.Blend(value, smoothing, clock.Time);
    }
}
```

the new body is compiled in `WeatherService`, just as it is during a normal
build. `samples`, `smoothing`, `forecast`, and `clock` resolve to the existing
fields and captured constructor state through their ordinary metadata tokens.
Nothing is redeclared or mapped. Access is direct compiled IL, with the same
member-access behavior as source compiled in the original file.

The update is global for that `MethodDef`: every existing and future
`WeatherService` instance executes the new body. The runtime type, reflection
identity, stack frames, method attributes, and call sites remain those of the
original method.

## Target composition

The development executable enables LiveCode first, then registers Source
Update:

```csharp
var liveCode = new RootLiveCode(
    injector,
    root,
    scripts,
    new("weather-demo"));
_ = liveCode.Enable();

_ = new RootSourceUpdate(
    liveCode.Bridges,
    SourceUpdateHostOptions.FromEnvironment(
        typeof(Program).Assembly)).Enable();
```

`SourceUpdateHostOptions.FromEnvironment` succeeds only for an immutable
AlvorSense editable launch.

## Supported edit boundary

Version 1 permits exactly one body change to one existing ordinary method per
generation. It does not add methods, fields, properties, constructors, types,
attributes, interfaces, or base types. It also rejects:

- signature or generic-shape changes;
- newly captured primary-constructor parameters;
- async, iterator, unsafe, dynamic, lambda, anonymous-object, and local-function
  shapes; and
- any compiler delta that adds metadata definitions or updates an unexpected
  method/type token.

Use a normal rebuild and process restart for an unsupported shape.

## Launch and apply

```powershell
dotnet run --project scripts\AlvorKit.Script.AlvorSense -- start `
  --id weather `
  --editable-project path\to\Weather.Game.csproj

dotnet run --project scripts\AlvorKit.Script.LiveCode -- workspace init `
  --id weather --purpose "Tune forecast" `
  --session weather-demo --alvorsense weather

dotnet run --project scripts\AlvorKit.Script.LiveCode -- source start `
  --workspace weather

dotnet run --project scripts\AlvorKit.Script.LiveCode -- source apply `
  --workspace weather `
  --source path\to\WeatherService.cs `
  --diff path\to\weather.diff
```

`source apply` returns after the target reserves and queues the operation. Run an
AlvorSense update, then require `source status` to report `applied`.

Each success advances one forward generation. Restoring old source text is
another forward edit. The process must restart to return to its original loaded
generation.

See [`AgentLiveDevelopment.md`](AgentLiveDevelopment.md) for the complete
workspace, evidence, visual-verification, and cleanup workflow.
