namespace AlvorKit.ECS;

/// <summary>Debugger proxy that exposes the components currently attached to an entity.</summary>
public class EntDebugView(IEnt ent)
{
    /// <summary>Gets the entity components visible to the debugger.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object[] Components
    {
        get
        {
            var fields = EntReg.Fields.ToArray()
                .Where(x => x.Has(ent)).OrderBy(x => x.NameType().Name).ToArray();

            var comps = new object[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                string name = fields[i].NameType().Name;
                object? value = fields[i].Get(ent);

                if (value == null || value.GetType().IsPrimitive || value.GetType() == typeof(string))
                    comps[i] = new DebugViewComponentPrimitive(name, value);
                else comps[i] = new DebugViewComponent(name, value);

            }

            return comps;
        }
    }

    /// <summary>Represents one component in the debugger view.</summary>
    [DebuggerDisplay("{Value}", Name = "{Name,nq}")]
    public readonly struct DebugViewComponent(string name, object? value)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string name = name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object? value = value;

        /// <summary>Gets the component marker type name.</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Name => name;

        /// <summary>Gets the component value.</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object? Value => value;
    }

    /// <summary>Represents a primitive or string component shown directly in the debugger.</summary>
    [DebuggerDisplay("{Value}", Name = "{Name,nq}")]
    public readonly struct DebugViewComponentPrimitive(string name, object? value)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string name = name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object? value = value;

        /// <summary>Gets the component marker type name.</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Name => name;

        /// <summary>Gets the component value.</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public object? Value => value;
    }
}
