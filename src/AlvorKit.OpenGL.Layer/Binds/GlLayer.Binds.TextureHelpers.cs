namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <summary>
    /// Clears every texture target binding for a texture unit.
    /// </summary>
    /// <param name="function">The GL function that requested the unbind.</param>
    /// <param name="unit">The texture unit whose target bindings should be cleared.</param>
    private void RequireAnyTextureUnitBinding(string function, uint unit)
    {
        if (!TryFindTextureUnitBinding(unit, out _))
            throw new GlNotBoundException(function, "attempted to unbind, but nothing is bound.");
    }

    /// <summary>
    /// Clears every texture target binding for a unit after the backend unbind succeeded.
    /// </summary>
    /// <param name="unit">The texture unit whose target bindings should be cleared.</param>
    private void ResetTextureUnitBindingsKnownBound(uint unit)
    {
        while (TryFindTextureUnitBinding(unit, out var key))
            textureBinds.UnbindKnownBound(key);
    }

    /// <summary>
    /// Finds one texture binding recorded for a texture unit without allocating a key snapshot.
    /// </summary>
    /// <param name="unit">The texture unit to inspect.</param>
    /// <param name="key">The matching texture binding key, if found.</param>
    /// <returns><see langword="true"/> when a binding for <paramref name="unit"/> was found.</returns>
    private bool TryFindTextureUnitBinding(uint unit, out (uint Unit, GlTextureTarget Target) key)
    {
        var bindings = textureBinds.GetEnumerator();
        while (bindings.MoveNext())
        {
            var binding = bindings.Current;
            if (binding.Key.Item1 != unit)
                continue;
            key = binding.Key;
            return true;
        }
        key = default;
        return false;
    }
}
