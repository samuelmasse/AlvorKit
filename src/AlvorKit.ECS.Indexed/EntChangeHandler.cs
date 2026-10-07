namespace AlvorKit;

/// <summary>Observes a committed change in value or presence.</summary>
public delegate void EntChangeHandler<T>(EntMutIdx ent, in EntChange<T> change);
