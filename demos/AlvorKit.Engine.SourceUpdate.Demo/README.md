# AlvorKit Engine Source Update Demo

Launch this project through AlvorSense with `--editable-project`. The target
starts from an immutable Debug PE/PDB copy and explicitly registers Source
Update.

`PulseService.Step` is an ordinary method in the original project file. It uses
three private fields and three primary-constructor captures. The state creates
two instances before any update. Change only that method body, save a unified
diff in the live workspace, and submit it with `source apply`. Both existing
instances then execute the new method definition because the runtime updates
the original `MethodDef`; no handler, reflection access, field redeclaration, or
call-site redirection is involved.

The acceptance edit changes:

```csharp
return new("ORIGINAL METHOD", palette.Original * (0.55f + energy * 0.45f), energy, updates);
```

to:

```csharp
return new("UPDATED METHOD", palette.Updated * (0.35f + energy * 0.65f), energy, updates);
```
