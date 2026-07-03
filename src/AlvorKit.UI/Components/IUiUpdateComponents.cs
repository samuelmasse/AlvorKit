namespace AlvorKit.UI;

[Components(SkipBuilder = true)]
public interface IUiUpdateComponents
{
    /// <summary>Callback invoked during the update phase.</summary>
    UiCallback<Action?> OnUpdateFV { get; set; }
}
