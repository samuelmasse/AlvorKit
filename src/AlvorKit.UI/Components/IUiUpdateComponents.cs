namespace AlvorKit;

[Components(SkipBuilder = true)]
public interface IUiUpdateComponents
{
    /// <summary>Callback invoked on normal ticks and when ahead updates are requested.</summary>
    UiCallback<Action?> OnUpdateFV { get; set; }

    /// <summary>Pending extra update calls consumed before layout; normal tick updates run separately.</summary>
    UiValue<int> AheadUpdateCountFV { get; set; }
}
