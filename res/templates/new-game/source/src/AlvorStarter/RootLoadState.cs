namespace AlvorStarter;

/// <summary>Bootstraps the Alvor Starter app scope from the engine root scope.</summary>
[Root]
public class RootLoadState(RootState state, RootScope scope) : State
{
    /// <summary>Creates the app scope and enters the starter state.</summary>
    public override void Load()
    {
        var app = scope.Scope<AppScope>();
        app.Add(new BlendUi(scope.Get<RootBlend>(), scope.Get<RootGl>()));
        state.Current = app.New<AppStarterState>();
    }
}
