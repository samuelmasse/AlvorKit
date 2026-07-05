namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>
    /// Gets the zero-based active texture unit index required by strict texture bind operations.
    /// </summary>
    /// <param name="function">The GL function that requires an active texture unit.</param>
    /// <returns>The active texture unit index.</returns>
    private uint GetActiveTextureIndex(string function) =>
        state.activeTexture.Value is { } unit
            ? (uint)((int)unit - (int)GlTextureUnit.Texture0)
            : throw new GlMissingPrerequisiteException(function, "no active texture unit is set; call glActiveTexture first.");
}
