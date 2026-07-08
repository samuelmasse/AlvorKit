namespace AlvorKit.ECS;

public class EntDebugView(IEnt ent)
{
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

    [DebuggerDisplay("{Value}", Name = "{Name,nq}")]
    public readonly struct DebugViewComponent(string name, object? value)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string name = name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object? value = value;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Name => name;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object? Value => value;
    }

    [DebuggerDisplay("{Value}", Name = "{Name,nq}")]
    public readonly struct DebugViewComponentPrimitive(string name, object? value)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string name = name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object? value = value;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Name => name;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public object? Value => value;
    }
}
