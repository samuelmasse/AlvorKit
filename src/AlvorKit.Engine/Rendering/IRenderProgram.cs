namespace AlvorKit.Engine;

/// <summary>Common API for engine shader programs that expose vertex attribute setup.</summary>
public interface IRenderProgram
{
    /// <summary>Gets the tracked OpenGL program handle.</summary>
    GlProgramHandle Id { get; }

    /// <summary>Enables and describes the program's vertex attributes for the currently bound vertex array.</summary>
    void SetAttributes();
}
