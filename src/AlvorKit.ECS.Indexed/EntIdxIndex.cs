namespace AlvorKit;

/// <summary>Registers one index's inputs and one removal callback per Clear or individual Dispose.</summary>
public class EntIdxIndex
{
    /// <summary>Borrowed registration owner; inputs must be added before allocation.</summary>
    private readonly EntIdxContext context;
    /// <summary>Cold registration set used to reject repeated inputs for this index.</summary>
    private readonly HashSet<Type> components = [];

    /// <summary>Binds a registration builder to the context that owns its removal callback.</summary>
    internal EntIdxIndex(EntIdxContext context) => this.context = context;

    /// <summary>Maintains an index on every Set and present Unset before reactions.</summary>
    public EntIdxIndex OnWrite<T, N>(EntWriteHandler update) where N : IComponent
    {
        ValidateInput<T, N>();
        context.GetPlan<T, N>().AddWrite(update, true);
        return this;
    }

    /// <summary>Maintains an index on value or presence changes before reactions.</summary>
    public EntIdxIndex OnChange<T, N>(EntChangeHandler<T> update) where N : IComponent
    {
        ValidateInput<T, N>();
        context.GetPlan<T, N>().AddChange(update, true);
        return this;
    }

    /// <summary>Validates the component contract and rejects duplicate index inputs.</summary>
    private void ValidateInput<T, N>() where N : IComponent
    {
        context.ValidateRegistration<T, N>();

        if (!components.Add(typeof(N)))
            throw new EntIdxRegistrationException($"Component {typeof(N).FullName} is already watched by this index.");
    }
}
