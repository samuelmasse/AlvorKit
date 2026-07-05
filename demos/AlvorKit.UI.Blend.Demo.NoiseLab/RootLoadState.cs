namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>Bootstraps the Noise Lab app scope from the engine root scope.</summary>
[Root]
public class RootLoadState(RootState state, RootScope scope) : State
{
    public override void Load() =>
        state.Current = scope.Scope<AppScope>().New<AppState>();
}
