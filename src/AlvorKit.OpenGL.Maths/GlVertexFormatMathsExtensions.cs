namespace AlvorKit.OpenGL;

/// <summary>Provides typed maths-vector vertex attribute declarations.</summary>
public static class GlVertexFormatMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glVertexAttribPointer</c> using the vector layout.</summary>
    public static void VertexAttribPointer<TVector>(this Gl gl, uint index, bool normalized, int stride, nint pointer)
        where TVector : unmanaged
    {
        var count = GlVertexFormatInfo<TVector>.GetNormalPointer(out var type);
        gl.VertexAttribPointer(index, count, type, normalized, stride, pointer);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glVertexAttribIPointer</c> using the vector layout.</summary>
    public static void VertexAttribIPointer<TVector>(this Gl gl, uint index, int stride, nint pointer)
        where TVector : unmanaged
    {
        var count = GlVertexFormatInfo<TVector>.GetInteger(out var type);
        gl.VertexAttribIPointer(index, count, type, stride, pointer);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glVertexAttribLPointer</c> using the vector layout.</summary>
    public static void VertexAttribLPointer<TVector>(this Gl gl, uint index, int stride, nint pointer)
        where TVector : unmanaged
    {
        var count = GlVertexFormatInfo<TVector>.GetLong(out var type);
        gl.VertexAttribLPointer(index, count, type, stride, pointer);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glVertexAttribFormat</c> using the vector layout.</summary>
    public static void VertexAttribFormat<TVector>(this Gl gl, uint attributeIndex, bool normalized, uint relativeOffset)
        where TVector : unmanaged
    {
        var count = GlVertexFormatInfo<TVector>.GetNormalFormat(out var type);
        gl.VertexAttribFormat(attributeIndex, count, type, normalized, relativeOffset);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glVertexAttribIFormat</c> using the vector layout.</summary>
    public static void VertexAttribIFormat<TVector>(this Gl gl, uint attributeIndex, uint relativeOffset)
        where TVector : unmanaged
    {
        var count = GlVertexFormatInfo<TVector>.GetInteger(out var type);
        gl.VertexAttribIFormat(attributeIndex, count, type, relativeOffset);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glVertexAttribLFormat</c> using the vector layout.</summary>
    public static void VertexAttribLFormat<TVector>(this Gl gl, uint attributeIndex, uint relativeOffset)
        where TVector : unmanaged
    {
        var count = GlVertexFormatInfo<TVector>.GetLong(out var type);
        gl.VertexAttribLFormat(attributeIndex, count, type, relativeOffset);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glVertexArrayAttribFormat</c> using the vector layout.</summary>
    public static void VertexArrayAttribFormat<TVector>(this Gl gl, GlVertexArrayHandle vertexArray,
        uint attributeIndex, bool normalized, uint relativeOffset) where TVector : unmanaged
    {
        var count = GlVertexFormatInfo<TVector>.GetNormalFormat(out var type);
        gl.VertexArrayAttribFormat(vertexArray, attributeIndex, count, type, normalized, relativeOffset);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glVertexArrayAttribIFormat</c> using the vector layout.</summary>
    public static void VertexArrayAttribIFormat<TVector>(this Gl gl, GlVertexArrayHandle vertexArray,
        uint attributeIndex, uint relativeOffset) where TVector : unmanaged
    {
        var count = GlVertexFormatInfo<TVector>.GetInteger(out var type);
        gl.VertexArrayAttribIFormat(vertexArray, attributeIndex, count, type, relativeOffset);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glVertexArrayAttribLFormat</c> using the vector layout.</summary>
    public static void VertexArrayAttribLFormat<TVector>(this Gl gl, GlVertexArrayHandle vertexArray,
        uint attributeIndex, uint relativeOffset) where TVector : unmanaged
    {
        var count = GlVertexFormatInfo<TVector>.GetLong(out var type);
        gl.VertexArrayAttribLFormat(vertexArray, attributeIndex, count, type, relativeOffset);
    }
}
