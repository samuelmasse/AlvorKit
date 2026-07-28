namespace AlvorKit.Mocking;

/// <summary>
/// Preserves an exact runtime type, including constructed, pointer, function-pointer, and by-reference forms.
/// </summary>
internal readonly record struct MockTypeIdentity
{
    private readonly Type runtimeType;

    /// <summary>
    /// Creates an identity from the runtime type that appears in the executable signature.
    /// </summary>
    internal MockTypeIdentity(Type runtimeType)
    {
        this.runtimeType = runtimeType;
    }

    /// <summary>
    /// Gets the represented runtime type.
    /// </summary>
    internal Type RuntimeType => runtimeType;

    /// <inheritdoc />
    public override string ToString() => runtimeType.AssemblyQualifiedName ?? runtimeType.ToString();
}
