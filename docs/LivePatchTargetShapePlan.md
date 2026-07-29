# Superseded: LivePatch Target-Shaped Source

This design has been superseded by
[`SourceDiffHotReloadPlan.md`](SourceDiffHotReloadPlan.md).

The replacement plan updates the original loaded method from an ordinary diff
to its real C# source file. The existing explicit-handler LivePatch is retained
only during migration and is removed after Source Update passes its acceptance
gates.
