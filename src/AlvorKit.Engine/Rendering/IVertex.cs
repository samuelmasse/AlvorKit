namespace AlvorKit.Engine;

/// <summary>Static vertex-layout contract consumed by <see cref="RenderProgram{T}"/>.</summary>
public interface IVertex
{
    /// <summary>Enables and describes this vertex type's attributes for the currently bound vertex array.</summary>
    static abstract void SetAttributes(GlLayer gl);
}
