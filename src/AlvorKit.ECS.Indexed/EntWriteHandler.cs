namespace AlvorKit;

/// <summary>Observes a committed sparse write. Equal values and reused references still produce notifications.</summary>
public delegate void EntWriteHandler(EntMutIdx ent);
