namespace AlvorKit;

/// <summary>Reads an intact Ent during teardown; Indexed mutation of the target is prohibited.</summary>
public delegate void EntLifecycleHandler(EntMutIdx ent);
