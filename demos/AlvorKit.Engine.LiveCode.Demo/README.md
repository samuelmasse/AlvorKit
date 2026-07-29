# AlvorKit Engine LiveCode Demo

This is a real `RootLoop` engine project: a living mycelial observatory whose
three animated colonies are simultaneously active `ColonyScope` instances.
Their colors, motion, population, atmosphere, and graph relationships come from
dependencies inside those exact scopes.

Start the game:

```powershell
dotnet run --project demos\AlvorKit.Engine.LiveCode.Demo
```

The normal engine frame loop pumps LiveCode before state updates. The standard
`RootLoop.RunGlfw` startup path also makes the window controllable through
AlvorSense.

Inspect the current graph and advertised bridge schemas:

```powershell
dotnet run --project scripts\AlvorKit.Script.LiveCode -- graph `
    --session mycelial-observatory

dotnet run --project scripts\AlvorKit.Script.LiveCode -- bridges `
    --session mycelial-observatory
```

Checked-in files beneath `Submissions/` demonstrate scoped commands, frozen
inspection, and structured bridge interactions. Agent-authored work belongs
beneath `tmp/live/<workspace-id>/lc/` so the exact source and result are
recorded with the live workspace.

Useful interactions:

- Click or press `Tab` to select a colony scope.
- Drag or use the arrow keys to move the selected colony.
- Right-click or press `Space` to pulse the selected colony.
- Press `B` to bloom every colony and `L` to intensify their links.
- Press `F` to deliberately freeze the game loop for out-of-band inspection.

Use the dedicated
[`AlvorKit.Engine.SourceUpdate.Demo`](../AlvorKit.Engine.SourceUpdate.Demo/)
when testing edits to an original C# method in a running process.
