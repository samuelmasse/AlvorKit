namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Bootstraps the azure tentacle demo app scope from the engine root scope.</summary>
[Root]
public class RootLoadState(RootState state, RootScope scope) : State
{
    public override void Load() =>
        state.Current = scope.Scope<AppScope>().New<AppState>();
}
