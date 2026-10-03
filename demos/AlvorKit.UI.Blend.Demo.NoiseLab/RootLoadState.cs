namespace AlvorKit;

/// <summary>Bootstraps the Noise Lab app scope from the engine root scope.</summary>
[Root]
public class RootLoadState(RootState state, RootScope scope) : State
{
    public override void Load()
    {
        var app = scope.Scope<AppScope>();
        // Decimal readouts need the 12px chip font to keep the period glyph visible.
        app.Add(new BlendUi(
            scope.Get<RootBlend>(), scope.Get<RootGl>(), BlendPalette.Default, new BlendMetrics { ChipFontSize = 12 }));
        state.Current = app.New<AppState>();
    }
}
