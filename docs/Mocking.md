# Mocking

`AlvorKit.Mocking` provides strict and loose test doubles, partial mocks,
ref-safe setup callbacks, invocation verification, cross-mock ordering, and an
Interception adapter for code that cannot be intercepted through an ordinary
proxy.

The supported execution paths are:

- interface and overridable class members through an ordinary mock;
- an existing instance through a partial mock; and
- selected calls in a profiler-enabled test process through
  `AlvorKit.Interception`, including concrete nonvirtual calls and receiver-free
  operations such as static calls, construction, constructor bodies, field
  access, and live struct receivers.

Receiver-free interception is always scoped to a `MockSession`. Concrete and
receiver-free interception require a CoreCLR process that loads the Interception
profiler at startup; NativeAOT is not supported for those paths.

## Quick start

Reference both the instrumentation-neutral control plane and the backend used
by the test process. For an ordinary JIT test project:

```xml
<ItemGroup>
    <PackageReference Include="AlvorKit.Mocking"
        Version="$(AlvorKitVersion)" />
    <PackageReference Include="AlvorKit.Mocking.Dynamic"
        Version="$(AlvorKitVersion)" />
</ItemGroup>
```

Keep the two package versions aligned. A project that references only
`AlvorKit.Mocking` can compile against the public control plane, but creating a
mock or using an intercepted site fails with an actionable error until exactly
one runtime backend is selected.

The ordinary API uses executable call expressions for both setup and
verification:

```csharp
using AlvorKit;

MockDynamic.Enable();

public interface IWorker
{
    bool TryWork(Job job);
    int Score(Job job);
    void Publish(string message);
}

public sealed record Job(int Priority);

IWorker worker = Mock.Create<IWorker>(); // Strict by default.
var published = new List<string>();

Mock.When(() => worker.TryWork(Arg.Any<Job>()))
    .ReturnSequence(false, true);
Mock.When(() => worker.Score(Arg.Any<Job>()))
    .Answer(call => call.Argument<Job>(0).Priority * 10);
Mock.When(() => worker.Publish(Arg.Any<string>()))
    .Do(call => published.Add(call.Argument<string>(0)));

_ = worker.TryWork(new Job(1));
_ = worker.TryWork(new Job(2));
int score = worker.Score(new Job(4));
worker.Publish("done");

Mock.Verify(() => worker.TryWork(Arg.Any<Job>()))
    .Exactly(2);
Mock.Verify(() => worker.Score(
        Arg.Match<Job>(job => job.Priority >= 4)))
    .Once();
Mock.Verify(() => worker.Publish("done"))
    .Once();
Mock.VerifyNoOtherCalls(worker);
```

`ReturnSequence` requires at least one value. It returns values in order and
repeats its last value after the sequence is exhausted.

Setup capture and verification capture do not call the configured behavior and
do not add invocation-history entries.

Constructed generic methods use their normal closed call expression; no
preparation or registration API is required:

```csharp
Mock.When(() => mapper.Map(Arg.Any<int>()))
    .Return(42);
Mock.When(() => mapper.Map(Arg.Any<string>()))
    .Return("mapped");

Mock.Verify(() => mapper.Map(Arg.Any<int>()))
    .Once();
```

Each constructed method has its own setup and verification identity.

## Strict, loose, and partial behavior

Choose the fallback policy when creating the target:

```csharp
IWorker strict = Mock.Create<IWorker>();
IWorker loose = Mock.CreateLoose<IWorker>();

object runtimeTyped = Mock.Create(
    typeof(IWorker),
    MockBehavior.Loose);

Worker existing = new();
Worker partial = Mock.Partial(existing);
```

- A strict mock throws `MockException` when an intercepted call has no matching
  setup.
- A loose mock returns the default value for an unmatched call.
- A partial mock preserves the supplied object and calls the original
  implementation when no setup matches.

Interface and overridable class members use the ordinary proxy path. Calls to
concrete nonvirtual members need to originate in an explicitly selected
Interception caller; see [Operation interception](#operation-interception).

For receiver-free operations, unmatched calls in an active session run the
original operation and are recorded. Add `.Strict()` to a receiver-free setup
when the matching operation should fail instead, or `.Passthrough()` when an
explicit matching setup should call the original operation.

## Matchers and configured behavior

Ordinary arguments support exact values and predicate-based matching:

```csharp
Mock.When(() => worker.TryWork(Arg.Any<Job>()))
    .Return(true);

Mock.When(() => worker.Score(
        Arg.Match<Job>(job => job.Priority > 10)))
    .Return(100);
```

The main terminal setup operations are:

- `.Return(value)` and `.ReturnSequence(values)` for ordinary return values;
- `.ReturnFactory(factory)` for a value produced on each call;
- `.Answer(callback)` for a callback that returns the result;
- `.Do(callback)` for a void callback;
- `.Throw(exception)` for a configured failure;
- `.Passthrough()` for supported receiver-free operations; and
- `.Strict()` for supported receiver-free operations.

`MockCall` exposes ordinary arguments by their declared parameter index:

```csharp
Mock.When(() => worker.Score(Arg.Any<Job>()))
    .Answer(call =>
    {
        Job job = call.Argument<Job>(0);
        return job.Priority * 10;
    });
```

The callback also exposes the intercepted object and exact method metadata:

```csharp
Mock.When(() => worker.Score(Arg.Any<Job>()))
    .Answer(call =>
    {
        Debug.Assert(ReferenceEquals(call.Instance, worker));
        Debug.Assert(call.Method.Name == nameof(IWorker.Score));
        return call.Argument<Job>(0).Priority;
    });
```

For ordinary `ref` and `out` parameters, use `SetReference` with the declared
parameter index:

```csharp
Mock.When(() => catalog.TryFind(
        Arg.Any<string>(),
        out _))
    .Answer(call =>
    {
        call.SetReference(1, expectedJob);
        return true;
    });
```

`MockCall` is deliberately an ordinary, heap-safe callback view. Use typed
callbacks for spans, other ref structs, or exact `in`/`ref`/`out` signatures.

## Ref-safe setup

An argument index supplied to a ref-safe matcher is the parameter's declared
index, not an index among only the ref-safe parameters:

```csharp
Mock.When(() => target.Count(
        Arg.Any<ReadOnlySpan<byte>>(0)))
    .Answer((ReadOnlySpan<byte> bytes) => bytes.Length);

Mock.When(() => target.Copy(
        Arg.ReadOnlySpanEqual(0, expected),
        Arg.Any<Span<byte>>(1)))
    .Do((
        ReadOnlySpan<byte> source,
        Span<byte> destination) =>
    {
        source.CopyTo(destination);
    });
```

The indexed ref-safe matchers include:

- `Arg.Any<T>(parameterIndex)` and
  `Arg.Match<T>(parameterIndex, predicate)`;
- `Arg.AnyRef<T>(parameterIndex)` and
  `Arg.Match<T>(parameterIndex, predicate)` for `ref` arguments; and
- `Arg.ReadOnlySpanEqual(parameterIndex, expected)` and
  `Arg.SpanEqual(parameterIndex, expected)`.

Natural lambdas preserve an exact `in`, `ref`, or `out` signature:

```csharp
Mock.When(() => target.Transform(
        Arg.Any<ReadOnlySpan<int>>(0),
        ref Arg.AnyRef<Span<int>>(1),
        out _))
    .Do((
        scoped in ReadOnlySpan<int> source,
        scoped ref Span<int> destination,
        scoped out int written) =>
    {
        source.CopyTo(destination);
        written = source.Length;
    });
```

The callback must match the captured method signature. If overload inference is
ambiguous, cast the lambda to a named delegate with that exact signature.

Three-input typed callbacks use the same declared parameter order:

```csharp
Mock.When(() => target.Combine(
        Arg.Any<ReadOnlySpan<byte>>(0),
        Arg.Any<ReadOnlySpan<byte>>(1),
        Arg.Any<int>()))
    .Answer((
        ReadOnlySpan<byte> left,
        ReadOnlySpan<byte> right,
        int seed) =>
            left.Length + right.Length + seed);
```

`.Throw(exception)` is available on both ordinary and ref-safe clauses. The
exception is thrown by the intercepted call, not while the setup expression is
captured.

### Ref-safe history

Borrowed values cannot be retained directly in ordinary invocation history.
Without a projector, their history entry is reported as unavailable rather
than boxed or copied implicitly. Add an explicit projector when verification
diagnostics need a stable value:

```csharp
Mock.When(() => target.Count(
        Arg.Any<ReadOnlySpan<byte>>(0)))
    .SnapshotArgument(
        0,
        (ReadOnlySpan<byte> bytes) => bytes.ToArray())
    .Answer((ReadOnlySpan<byte> bytes) => bytes.Length);
```

Use `.SnapshotArgumentOnExit(...)` when the stable value must be captured after
the callback and any reference writeback. Projectors make allocation and
lifetime choices explicit.

### Ref-struct and managed-reference returns

`ReturnFactory` can preserve a borrowed ref-struct return:

```csharp
Mock.When(target.ReadOnlyBuffer)
    .ReturnFactory(owner.ReadOnlyBuffer);
```

The owner used by the factory must remain alive, and the returned view must not
outlive its storage. For `Span<T>` and `ReadOnlySpan<T>`, `ReturnOwned` instead
copies the setup input once and returns views over storage owned by the mock:

```csharp
Mock.When(target.ReadOnlyBuffer)
    .ReturnOwned(new[] { 2, 3, 5 });
```

Managed-reference returns keep mutable and readonly forms distinct:

```csharp
Mock.WhenRef(target.MutableValue)
    .ReturnRef(owner.MutableValue);

Mock.WhenRefReadonly(target.ReadOnlyValue)
    .ReturnRef(owner.ReadOnlyValue);
```

`ReturnRef(value)` is also available when the mock should own stable storage
for a configured value.

`ReturnOwned` returns mutable views over the same setup-owned storage for
`Span<T>`. Mutations made through one returned view are therefore visible
through later returned views. Use `ReadOnlySpan<T>` when callers must not
mutate that owned storage.

## Verification and history

Verification quantifiers are applied after capturing the expected call:

```csharp
Mock.Verify(() => worker.TryWork(Arg.Any<Job>()))
    .Exactly(3);
Mock.Verify(() => worker.Publish(Arg.Any<string>()))
    .AtLeast(1);
Mock.Verify(() => worker.Score(Arg.Any<Job>()))
    .AtMost(4);
Mock.Verify(() => worker.TryWork(
        Arg.Match<Job>(job => job.Priority < 0)))
    .Never();
```

`.Once()` is the common exact-one form. Successful verification marks the
matching invocations as verified. `Mock.VerifyNoOtherCalls(target)` then fails
if that target still has unverified invocations.

`Mock.ClearInvocations(target)` starts a new history epoch for that target but
keeps its setups. It does not clear other mocks.

The public surface intentionally exposes history through verification,
checkpoints, and exact sequence checks. It does not currently expose a
read-only general-purpose invocation-history browser.

## Sessions, checkpoints, and order

A session supplies one logical timeline across ordinary mocks and all
receiver-free operations executed while that session is current:

```csharp
using var session = Mock.Session();

MockCheckpoint before = session.Checkpoint();
jobs.Claim(job);
movement.Begin(cell);
terrain.Mine(cell);
MockCheckpoint through = session.Checkpoint();

Mock.Verify(() => jobs.Claim(job))
    .Between(before, through)
    .Once();

session.VerifySequence(
    () => jobs.Claim(job),
    () => movement.Begin(cell),
    () => terrain.Mine(cell));
```

A checkpoint window is lower-exclusive and upper-inclusive: calls after
`before` and through `through` are considered. Both checkpoints must belong to
the same session and must be in chronological order.

`VerifySequence` compares the exact invocation sequence in its window. Extra,
missing, or differently ordered calls fail the check. A successful sequence
check marks the matched calls as verified; a failed one marks none.

Sessions are ambient through `ExecutionContext`, so they flow through ordinary
`await` and `Task.Run` usage. `session.Run(...)` temporarily makes a session
current when execution-context flow has been suppressed. Nested sessions are
isolated and must be disposed in last-in, first-out order.

```csharp
using var session = Mock.Session();

ExecutionContext.SuppressFlow();
try
{
    Task.Run(() =>
        session.Run(() => worker.Publish("explicit")))
        .GetAwaiter()
        .GetResult();
}
finally
{
    ExecutionContext.RestoreFlow();
}
```

The timeline is a monotonic logical entry order. It is suitable for
cross-mock ordering assertions and is not a wall-clock timestamp.

## Async behavior

Task-returning and value-task-returning setups use the same `Answer` and
`ReturnFactory` APIs:

```csharp
Mock.When(() => target.CountAsync(
        Arg.Any<ReadOnlySpan<byte>>(0)))
    .Answer((ReadOnlySpan<byte> bytes) =>
        CountCopiedAsync(bytes.ToArray()));

static async Task<int> CountCopiedAsync(byte[] bytes)
{
    await Task.Yield();
    return bytes.Length;
}
```

The typed callback itself runs synchronously at the intercepted boundary. A
span or other borrowed value must be consumed or copied before the callback
returns; it cannot cross an `await`. Async-void callbacks are rejected because
their failure and completion cannot be observed safely.

For a declared `Task`, `Task<T>`, `ValueTask`, or `ValueTask<T>` return, the
invocation first records the synchronous return. Its same history entry is
later augmented with succeeded, faulted, or canceled async completion. Async
completion does not add a second timeline entry. A `Task` hidden behind a
declared `object` result is not treated as the method's async completion.

## Events

Subscribe normally and raise the captured event through the mock:

```csharp
worker.Completed += OnCompleted;

Mock.Raise(
    () => worker.Completed += null!,
    worker,
    EventArgs.Empty);
```

The arguments after the event expression are passed to the subscribed
handlers. Event setup capture and `Mock.Raise` do not advance a session's
invocation timeline.

## Operation interception

Concrete nonvirtual and receiver-free operations use the
`AlvorKit.Mocking.Interception` adapter over `AlvorKit.Interception`. The
selected CoreCLR test process loads the native profiler at startup, then the
managed preparation layer validates the original loaded IL, publishes exact
typed Mocking wrappers, and activates the caller rewrite.

The callee does not need to belong to the test assembly. Selection belongs to
the caller method and original IL offset, and the profiler allowlist limits
which modules may be rewritten. Missing or stale method bodies, signature
mismatches, ambiguous operations, and unsupported IL fail before activation.

Use the repository's child-process launcher so profiler variables never leak
into the parent test runner:

```powershell
dotnet run --project scripts/AlvorKit.Script.TestInterception -- `
  --test-project tests/Example.Mocking.Interception.Test/Example.Mocking.Interception.Test.csproj `
  --configuration Release
```

See [AlvorKit Interception](Interception.md) for startup, platform, allowlist,
loaded-body, ReJIT, inliner, and diagnostics contracts.

## Receiver-free operations

Receiver-free setup, verification, and call-site capture require a current
`MockSession`. With no current session, an intercepted receiver-free site
bypasses the mocking runtime and runs the original operation without recording
history.
Parallel sessions own independent receiver-free setups.

The expressions passed to these APIs execute the selected intercepted call path.
They identify both the logical operation and its owned caller context.

### Static methods and properties

Use ordinary `When` and `Verify` syntax inside a session:

```csharp
using var session = Mock.Session();

Mock.When(() => CallSites.ReadClock())
    .Return(fixedTime);
Mock.When(() => CallSites.Reset())
    .Passthrough();
Mock.When(() => CallSites.Fail())
    .Strict();

DateTimeOffset actual = CallSites.ReadClock();

Mock.Verify(() => CallSites.ReadClock())
    .Once();
```

The selected method may be a wrapper around a static method or property
access. Only the selected caller operation is rewritten through ReJIT.

### Exact call sites

When the same operation appears at more than one selected site, capture and
apply an exact site:

```csharp
MockCallSite first = Mock.Site(
    () => CallSites.ReadClock());

Mock.When(() => CallSites.ReadClock())
    .AtSite(first)
    .Return(fixedTime);

Mock.Verify(() => CallSites.ReadClock())
    .AtSite(first)
    .Once();
```

`Mock.Site` must execute exactly one supported intercepted operation.
`.AtSite(...)`
can then distinguish otherwise identical operations in different caller
locations.

### Construction

`WhenNew` intercepts an owned `newobj` site:

```csharp
using var session = Mock.Session();
var substitute = new Buffer(256);

Mock.WhenNew(() => CallSites.CreateBuffer(
        Arg.Any<int>()))
    .Substitute(substitute);

Mock.WhenNew(() => CallSites.CreateBuffer(64))
    .SubstituteFactory(
        (Func<int, Buffer>)(capacity =>
            new Buffer(capacity + 1)));

Buffer actual = CallSites.CreateBuffer(32);

Mock.VerifyNew(() => CallSites.CreateBuffer(32))
    .Once();
```

`Substitute` and `SubstituteFactory` skip the original allocation and
constructor at that `newobj` operation. A factory must return a non-null value
assignable to the constructed type. Construction clauses also support
`.Throw(...)`, `.Passthrough()`, `.Strict()`, and `.AtSite(...)`.

### Constructor bodies

Constructor-body interception is definition-wide for the selected constructor
body and happens after the mandatory base or delegating initializer:

```csharp
using var session = Mock.Session();

Mock.WhenConstructorBody(
        () => CallSites.CreateBuffer(Arg.Any<int>()))
    .Observe(
        (Action<Buffer, int>)((buffer, capacity) =>
        {
            observed = buffer;
            observedCapacity = capacity;
        }));

Mock.VerifyConstructorBody(
        () => CallSites.CreateBuffer(32))
    .Once();
```

`.Observe(...)` runs the callback and then the original constructor remainder.
`.Replace(...)` keeps the allocated object and completed mandatory initializer
but replaces the constructor remainder. `.Passthrough()`, `.Throw(...)`, and
`.Strict()` are also available.

Constructor-body clauses currently have no `.AtSite(...)` modifier. Use
`WhenNew(...).AtSite(...)` when the desired distinction is the allocation site
rather than the constructor definition.

### Field reads and writes

Describe the exact field once, then configure typed reads and writes:

```csharp
MockField<int> version =
    Mock.Field<Worker, int>("globalVersion");
MockField<Job> currentJob =
    Mock.Field<Worker, Job>("currentJob");

using var session = Mock.Session();

Mock.WhenFieldRead(version)
    .Transform(
        (scoped in int value) => value + 1);

Mock.WhenFieldWrite(
        worker,
        currentJob,
        () => Arg.Any<Job>())
    .Observe(
        (scoped in Job value) =>
            observed = value);

Mock.VerifyFieldRead(version)
    .Once();
Mock.VerifyFieldWrite(
        worker,
        currentJob,
        () => Arg.Any<Job>())
    .Once();
```

`Mock.Field<TDeclaring, TValue>(name)` validates the declaring and value types.
`Mock.Field<TValue>(fieldInfo)` creates the same typed descriptor from
reflection metadata. Static forms omit the receiver; instance forms take the
exact receiver.

```csharp
FieldInfo metadata = typeof(Worker).GetField(
    "currentJob",
    BindingFlags.Instance | BindingFlags.NonPublic)!;
MockField<Job> reflected = Mock.Field<Job>(metadata);
```

A read can `.Return(...)`, `.ReturnFactory(...)`, `.Observe(...)`,
`.Transform(...)`, `.Throw(...)`, `.Passthrough()`, or `.Strict()`. A write can
`.Observe(...)`, `.Transform(...)`, `.Throw(...)`, `.Passthrough()`, or
`.Strict()`. A write transform changes the value before storage; a read
transform changes the value after loading. Read and write clauses and
verifications support `.AtSite(...)`.

Field observation and transformation delegates preserve the field's exact
value type and receive it as `scoped in`. Do not retain that borrowed callback
value.

## Struct receiver semantics

A struct value has copy semantics:

- assignment, by-value parameter passing, returns, and boxing can create a new
  copy;
- there is no stable managed receiver identity or address to follow across
  those copies; and
- a mutation applies only to the specific live receiver presented at the
  intercepted operation.

The public struct scope models that reality with three selection modes:

- type-wide matching;
- a live-value predicate evaluated for each receiver copy at each call; and
- an exact intercepted call site.

It intentionally does not offer reference-style “this particular struct
instance” identity. Entry and exit receiver snapshots require explicit
projectors, and receiver mutation callbacks apply to the live receiver for that
one operation.

Struct interception requires an Interception-selected caller. Configure the live
receiver through an exact `scoped ref` capture:

```csharp
using MockSession session = Mock.Session();

Mock.Struct<GridCursor>()
    .Matching(
        static (scoped in GridCursor cursor) =>
            cursor.Layer == 3)
    .When<int>(
        static (scoped ref GridCursor cursor) =>
            cursor.Move(Arg.Any<int>()))
    .SnapshotThisOnEntry(
        static (scoped in GridCursor cursor) =>
            cursor.Layer)
    .MutateThisOnExit(
        static (scoped ref GridCursor cursor) =>
            cursor.Layer++)
    .Passthrough();
```

Use `Mock.Struct<T>()` for type-wide behavior, `.Matching(...)` for a predicate
reevaluated against each live entry copy, or `.AtSite(...)` for one exact
intercepted call site. Mutable receivers preserve caller-visible storage; readonly
receivers reject mutation setup. Assignment and boxing remain copies, and no
temporary receiver address is retained as identity.

Void and value-returning struct members have parallel setup and verification
forms:

```csharp
Mock.Struct<GridCursor>()
    .When(static (scoped ref GridCursor cursor) =>
        cursor.Reset())
    .Do((MockStructCall<GridCursor>)(
        static (scoped ref GridCursor cursor) =>
            cursor.Layer = 0));

Mock.Struct<GridCursor>()
    .AtSite(moveSite)
    .When<int>(
        static (scoped ref GridCursor cursor) =>
            cursor.Move(Arg.Any<int>()))
    .Return(7);

Mock.Struct<GridCursor>()
    .Verify(static (scoped ref GridCursor cursor) =>
        cursor.Reset())
    .Between(before, through)
    .Once();
```

Value-returning struct clauses also support `.ReturnFactory(...)`,
`.Answer(...)`, `.Throw(...)`, `.Passthrough()`, and `.Strict()`. Void clauses
use `.Do(...)` instead of `.Answer(...)`. An unmatched receiver-free struct
call runs the original operation; `.Strict()` is the explicit failure policy.

## Deployment posture

The instrumentation-neutral `AlvorKit.Mocking` control plane selects proxy and
operation capabilities independently. `AlvorKit.Mocking.Dynamic` owns proxy and
callback execution; `AlvorKit.Mocking.Interception` owns concrete and
receiver-free operation execution. Neither is referenced by core, and selecting
a conflicting provider after one is active fails deterministically.

### JIT and Dynamic

JIT suites reference `AlvorKit.Mocking.Dynamic` and call
`MockDynamic.Enable()` once before creating mocks. Repeated calls are
idempotent. Dynamic owns proxy and callback runtime emission.

### CoreCLR Interception

Concrete and receiver-free suites also reference
`AlvorKit.Mocking.Interception` and call `MockInterception.Enable()` before
binding operation wrappers. The selected test process must load the
Interception profiler at startup. Repeated calls are idempotent.

ReJIT changes future managed calls and known inliners; an invocation already
active on old native code finishes on that old version. Interception cannot
instrument NativeAOT code, reverse earlier side effects, or coexist silently
with another profiler.

The retired build-time rewriting and generated NativeAOT pipelines are not
supported deployment paths.

## Diagnostics and test design

Prefer the smallest interception mechanism that expresses the test:

1. Use an interface mock when the collaborator already has an interface.
2. Use a class mock for overridable members.
3. Use a partial mock when unmatched calls should execute on an existing
   object.
4. Select a small caller through CoreCLR Interception for concrete nonvirtual
   or receiver-free behavior.

Use a hand-written fake when one reusable in-memory implementation communicates
the domain more clearly than many per-test setups. Use an integration fixture
when the behavior under test is the real boundary itself—serialization,
database behavior, native ABI, process startup, or framework wiring. A mock is
best when the important assertion is a collaborator call, its arguments,
result, failure, count, or order.

Keep allowlists narrow, use `.AtSite(...)` only when member-level matching is
ambiguous, and verify observable collaboration rather than internal
implementation detail. Add explicit snapshot projectors only when a stable
diagnostic value is useful.

Benchmark representative workloads before adding a hot-path callback, snapshot
allocation, or broad interception surface to a performance-sensitive suite.
