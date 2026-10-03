namespace AlvorKit;

/// <summary>Bootstraps the Blend editor-shell demo app scope from the engine root scope.</summary>
[Root]
public class RootLoadState(RootState state, RootScope scope) : State
{
    public override void Load()
    {
        var app = scope.Scope<AppScope>();
        app.Add(new BlendUi(scope.Get<RootBlend>(), scope.Get<RootGl>()));
        state.Current = app.New<EditorShellState>();
    }
}
