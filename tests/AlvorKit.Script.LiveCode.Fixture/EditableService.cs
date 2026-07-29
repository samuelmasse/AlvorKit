namespace AlvorKit.Script.LiveCode.Fixture;

/// <summary>Stable dependency captured before Source Update changes an existing service method.</summary>
public sealed class EditableDependency(string identity)
{
    /// <summary>Gets the dependency identity used by the fixture method.</summary>
    public string Identity => identity;
}

/// <summary>Fixture with private state, a reference field, and captured and uncaptured primary parameters.</summary>
public sealed class EditableService(
    EditableDependency dependency,
    string uncaptured)
{
    private int value = 3;
    private readonly EditableDependency reference = dependency;

    /// <summary>Uses ordinary locals, private fields, and the captured dependency.</summary>
    public string Update(int delta)
    {
        var next = value + delta;
        value = next;
        return $"{reference.Identity}:{dependency.Identity}:{value}";
    }
}
