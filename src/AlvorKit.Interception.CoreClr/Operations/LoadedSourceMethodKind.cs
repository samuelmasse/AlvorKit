namespace AlvorKit;

/// <summary>Identifies how a selected source method maps to an executable loaded body.</summary>
public enum LoadedSourceMethodKind
{
    /// <summary>The source MethodDef owns the executable body directly.</summary>
    Synchronous,

    /// <summary>An async source MethodDef maps to its generated state-machine <c>MoveNext</c>.</summary>
    Async,

    /// <summary>An iterator source MethodDef maps to its generated state-machine <c>MoveNext</c>.</summary>
    Iterator,

    /// <summary>An async-iterator source MethodDef maps to its generated <c>MoveNext</c>.</summary>
    AsyncIterator
}
