namespace AlvorKit.Script.Bindgen;

/// <summary>Maps OpenGL registry scalar aliases to generated C# scalar types.</summary>
internal static class GlRegistryValueTypes
{
    /// <summary>Registry scalar aliases by C name; pointer forms are modeled separately as raw nint.</summary>
    public static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<string, string>
    {
        ["void"] = "void",
        ["GLvoid"] = "void",
        ["GLboolean"] = "bool",
        ["GLbyte"] = "sbyte",
        ["GLubyte"] = "byte",
        ["GLchar"] = "byte",
        ["GLshort"] = "short",
        ["GLushort"] = "ushort",
        ["GLint"] = "int",
        ["GLuint"] = "uint",
        ["GLsizei"] = "int",
        ["GLenum"] = "uint",
        ["GLbitfield"] = "uint",
        ["GLfloat"] = "float",
        ["GLclampf"] = "float",
        ["GLdouble"] = "double",
        ["GLclampd"] = "double",
        ["GLint64"] = "long",
        ["GLuint64"] = "ulong",
        ["GLintptr"] = "nint",
        ["GLsizeiptr"] = "nint",
        ["GLsync"] = "nint",
        ["GLDEBUGPROC"] = "nint"
    };
}
