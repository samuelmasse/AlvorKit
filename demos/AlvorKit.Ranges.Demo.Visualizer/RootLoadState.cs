namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Bootstraps the demo app scope from the engine root scope.</summary>
[Root]
public class RootLoadState(RootState state, RootScope scope) : State
{
    public override void Load() =>
        state.Current = scope.Scope<AppScope>().New<AppState>();
}
