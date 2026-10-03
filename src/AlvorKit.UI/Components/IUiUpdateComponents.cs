namespace AlvorKit;

[Components(SkipBuilder = true)]
public interface IUiUpdateComponents
{
    /// <summary>Callback invoked once per normal tick, after layout and input dispatch.</summary>
    UiCallback<Action?> OnUpdateFV { get; set; }

    /// <summary>Prepares data and children before first visible layout, reactivation, and requested refreshes.</summary>
    UiCallback<Action?> OnRefreshFV { get; set; }

    /// <summary>Pending refresh calls consumed before layout, coalescing with the first or periodic refresh.</summary>
    UiValue<int> RefreshCountFV { get; set; }

    /// <summary>Time between visible refreshes in UI update time; zero disables periodic refreshes.</summary>
    UiProp<TimeSpan> RefreshIntervalFV { get; set; }
}
