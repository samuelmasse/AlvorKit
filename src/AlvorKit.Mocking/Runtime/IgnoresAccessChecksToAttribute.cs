namespace System.Runtime.CompilerServices;

/// <summary>
/// Allows a generated companion assembly to call the mocking runtime's internal dispatch seam.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
internal sealed class IgnoresAccessChecksToAttribute(string assemblyName) : Attribute
{
    /// <summary>
    /// Gets the exempted assembly name.
    /// </summary>
    public string AssemblyName { get; } = assemblyName;
}
