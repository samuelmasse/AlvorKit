# GL Object Ownership

`GlLayer` is hierarchical: layers form a tree, and every GL object belongs to the layer node that
created it. Disposing a node deletes its children's objects and then its own, in one sweep. This
turns GPU resource lifetime into the same decision as service lifetime in
[GameScopeOrganization.md](GameScopeOrganization.md): pick the scope, and the cleanup follows.

## The Layer Tree

`RootGl` (`AlvorKit.Engine`) is the root node over the GL backend. A game declares one facade per
dependency-injection scope whose services own GPU objects. Each facade is a single line:

```csharp
[Game]
public class GameGl(RootGl gl) : GlLayer(gl);

[World]
public class WorldGl(GameGl gl) : GlLayer(gl);

[Level]
public class LevelGl(WorldGl gl) : GlLayer(gl);
```

A node is a full `GlLayer`: raw GL, `Unbind*`/`Reset*`, the typed memory helpers — everything.
Every existing API that accepts a `GlLayer` accepts any node. Behind that surface, two kinds of
state behave differently:

- **Context state is shared by the whole tree.** Binds, single-assignment state, enable caps, and
  memory accounting model the one underlying GL context. Binding through `LevelGl` and unbinding
  through `RootGl` is one coherent story to the strict validation, any node may *use* any object,
  and calls cost the same at every depth — a node calls the backend directly, not through its
  parents.
- **Ownership is per node.** `Gen*`/`Create*` calls record the new object on the node they were
  called on. Ownership decides exactly one thing: what dies when that node is disposed.

## Lifetime-Tied Objects Need No Dispose Code

> If an object's lifetime is tied to an application layer, do **not** give it an explicit dispose
> path. Create it through that layer's node and let the layer's single `Dispose()` reclaim the
> whole scope in one sweep.

When a service owns GPU objects that live exactly as long as its scope:

- do **not** implement `IDisposable` on the service,
- do **not** write `DeleteBuffer`/`DeleteTexture` teardown calls in it,
- do **not** enumerate objects in an unloader.

A level's mesh storage allocates large vertex buffers and never mentions deletion:

```csharp
[Level]
public class LevelChunkMeshes(LevelGl gl)
{
    private readonly GlBufferHandle vertices = gl.GenBuffer();
}
```

Because the buffer was created through `LevelGl`, it is level-owned. The unloader for the whole
world is one call, and it is the only teardown code the frontend needs:

```csharp
[WorldLoader]
public class WorldUnloader(WorldGl gl)
{
    public void Run() => gl.Dispose();
}
```

`Dispose()` on a node first disposes its child nodes, newest first, then deletes the node's own
objects in reverse dependency order. Disposal is idempotent, and a disposed child detaches from
its parent's `Children`.

Explicit `Delete*` calls remain correct for exactly one situation: an object that dies **mid**
scope — a streamed texture being replaced, a buffer regenerated while the level stays loaded. An
object that dies *with* its scope makes explicit deletion redundant at best and a double-delete
hazard at worst.

## Choosing The Owner

Inject the facade of the scope that matches the objects' lifetime; that one constructor parameter
*is* the lifetime management.

- A texture atlas that outlives every world belongs on the game node.
- Per-world geometry pools belong on the world or level node.
- Debug objects that live for the whole session belong on `RootGl`.

If no scope names the moment an object dies, that is a design question about the scope layout —
not a reason to hand the service an `IDisposable`.

## Deletion And Disposal Rules

- **Children delete upward, never sideways or downward.** An explicit `Delete*` resolves against
  the calling node first, then its ancestors, so a child can clean up objects its scope inherited.
  Deleting an object owned by a sibling or descendant throws the not-tracked exception naming the
  resource.
- **Objects must be unbound before their owner is disposed.** A child node's disposal routes
  through the strict validated deletes; an object still bound at that moment throws with the
  handle named. The ordinary per-frame discipline — renderers unbind what they bound — already
  satisfies this.
- **The root is the context.** `RootGl.Dispose()` is whole-context teardown and drains straight to
  the backend. Calling it at shutdown is optional; the GL context dies with the window.
- **One GL thread.** Nodes add no locking; hierarchy calls follow the same GL-thread rule as every
  other layer call.
