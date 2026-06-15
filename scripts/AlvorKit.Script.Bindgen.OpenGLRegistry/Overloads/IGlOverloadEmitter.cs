namespace AlvorKit.Script.Bindgen;

/// <summary>Emitter contract for one family of generated OpenGL convenience overloads.</summary>
internal interface IGlOverloadEmitter
{
    /// <summary>Appends overloads for a command when that emitter recognizes a safe pattern.</summary>
    void Append(StringBuilder output, GlCommand command);
}
