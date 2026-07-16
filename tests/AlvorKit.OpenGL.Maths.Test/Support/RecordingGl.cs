namespace AlvorKit.OpenGL.Maths.Test;

internal sealed class RecordingGl : GlNoop
{
    public int CallCount { get; private set; }
    public Vec4 LastColor { get; private set; }
    public Vec4b LastMask { get; private set; }
    public Vec4i LastRegion { get; private set; }
    public Vec4i LastSourceRegion { get; private set; }
    public Vec4i LastDestinationRegion { get; private set; }
    public Vec3i LastOffset { get; private set; }
    public Vec3i LastSize { get; private set; }
    public float[] LastFloats { get; private set; } = [];
    public bool LastTranspose { get; private set; }
    public int LastLocation { get; private set; }
    public int LastComponentCount { get; private set; }
    public GlVertexAttribPointerType LastPointerType { get; private set; }
    public GlVertexAttribType LastFormatType { get; private set; }
    public GlVertexAttribIType LastIntegerType { get; private set; }
    public GlVertexAttribLType LastLongType { get; private set; }

    public override void ClearColor(float red, float green, float blue, float alpha)
    {
        CallCount++;
        LastColor = (red, green, blue, alpha);
    }

    public override void ColorMask(bool red, bool green, bool blue, bool alpha)
    {
        CallCount++;
        LastMask = (red, green, blue, alpha);
    }

    public override void Viewport(int x, int y, int width, int height)
    {
        CallCount++;
        LastRegion = (x, y, width, height);
    }

    public override void ScissorArrayv(uint first, int count, ReadOnlySpan<int> v)
    {
        CallCount++;
        LastRegion = new(v[..4]);
    }

    public override void Uniform3f(int location, float v0, float v1, float v2)
    {
        CallCount++;
        LastLocation = location;
        LastFloats = [v0, v1, v2];
    }

    public override void Uniform3fv(int location, ReadOnlySpan<float> value)
    {
        CallCount++;
        LastLocation = location;
        LastFloats = value.ToArray();
    }

    public override void ProgramUniform3f(GlProgramHandle program, int location, float v0, float v1, float v2)
    {
        CallCount++;
        LastLocation = location;
        LastFloats = [v0, v1, v2];
    }

    public override void UniformMatrix4fv(int location, bool transpose, ReadOnlySpan<float> value)
    {
        CallCount++;
        LastLocation = location;
        LastTranspose = transpose;
        LastFloats = value.ToArray();
    }

    public override void TexSubImage3D(GlTextureTarget target, int level, int xoffset, int yoffset, int zoffset,
        int width, int height, int depth, GlPixelFormat format, GlPixelType type, nint pixels)
    {
        CallCount++;
        LastOffset = (xoffset, yoffset, zoffset);
        LastSize = (width, height, depth);
    }

    public override void ReadPixels(int x, int y, int width, int height,
        GlPixelFormat format, GlPixelType type, nint pixels)
    {
        CallCount++;
        LastRegion = (x, y, width, height);
    }

    public override void RenderbufferStorage(GlRenderbufferTarget target, GlInternalFormat internalformat,
        int width, int height)
    {
        CallCount++;
        LastRegion = (0, 0, width, height);
    }

    public override void BlitFramebuffer(
        int sourceX0,
        int sourceY0,
        int sourceX1,
        int sourceY1,
        int destinationX0,
        int destinationY0,
        int destinationX1,
        int destinationY1,
        GlClearBufferMask mask,
        GlBlitFramebufferFilter filter)
    {
        CallCount++;
        LastSourceRegion = (sourceX0, sourceY0, sourceX1, sourceY1);
        LastDestinationRegion = (destinationX0, destinationY0, destinationX1, destinationY1);
    }

    public override void VertexAttribPointer(uint index, int size, GlVertexAttribPointerType type,
        bool normalized, int stride, nint pointer)
    {
        CallCount++;
        LastComponentCount = size;
        LastPointerType = type;
    }

    public override void VertexAttribIPointer(uint index, int size, GlVertexAttribIType type, int stride, nint pointer)
    {
        CallCount++;
        LastComponentCount = size;
        LastIntegerType = type;
    }

    public override void VertexAttribLPointer(uint index, int size, GlVertexAttribLType type, int stride, nint pointer)
    {
        CallCount++;
        LastComponentCount = size;
        LastLongType = type;
    }

    public override void VertexAttribFormat(
        uint attribindex,
        int size,
        GlVertexAttribType type,
        bool normalized,
        uint relativeoffset)
    {
        CallCount++;
        LastComponentCount = size;
        LastFormatType = type;
    }

    public override void VertexAttribIFormat(uint attribindex, int size, GlVertexAttribIType type, uint relativeoffset)
    {
        CallCount++;
        LastComponentCount = size;
        LastIntegerType = type;
    }

    public override void VertexAttribLFormat(uint attribindex, int size, GlVertexAttribLType type, uint relativeoffset)
    {
        CallCount++;
        LastComponentCount = size;
        LastLongType = type;
    }
}
