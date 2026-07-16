namespace AlvorKit.OpenGL;

internal static class GlVertexFormatInfo<TVector> where TVector : unmanaged
{
    private static readonly Type VectorType = typeof(TVector);
    private static readonly int componentCount = CreateComponentCount();
    private static readonly GlVertexAttribPointerType normalPointerType = CreateNormalPointerType();
    private static readonly GlVertexAttribIType integerType = CreateIntegerType();
    private static readonly bool supportsNormal = IsSupportedNormal();
    private static readonly bool supportsInteger = IsSupportedInteger();
    private static readonly bool supportsLong = IsDouble();

    public static int GetNormalPointer(out GlVertexAttribPointerType type)
    {
        if (!supportsNormal)
            ThrowUnsupported("glVertexAttribPointer");

        type = normalPointerType;
        return componentCount;
    }

    public static int GetNormalFormat(out GlVertexAttribType type)
    {
        if (!supportsNormal)
            ThrowUnsupported("glVertexAttribFormat");

        type = (GlVertexAttribType)(uint)normalPointerType;
        return componentCount;
    }

    public static int GetInteger(out GlVertexAttribIType type)
    {
        if (!supportsInteger)
            ThrowUnsupported("integer vertex attributes");

        type = integerType;
        return componentCount;
    }

    public static int GetLong(out GlVertexAttribLType type)
    {
        if (!supportsLong)
            ThrowUnsupported("long vertex attributes");

        type = GlVertexAttribLType.Double;
        return componentCount;
    }

    private static int CreateComponentCount()
    {
        if (Is<Vec2, Vec2d, Vec2h>() || Is<Vec2i8, Vec2u8, Vec2i16>() || Is<Vec2u16, Vec2i, Vec2u>())
            return 2;
        if (Is<Vec3, Vec3d, Vec3h>() || Is<Vec3i8, Vec3u8, Vec3i16>() || Is<Vec3u16, Vec3i, Vec3u>())
            return 3;
        if (Is<Vec4, Vec4d, Vec4h>() || Is<Vec4i8, Vec4u8, Vec4i16>() || Is<Vec4u16, Vec4i, Vec4u>())
            return 4;
        return 0;
    }

    private static GlVertexAttribPointerType CreateNormalPointerType()
    {
        if (Is<Vec2, Vec3, Vec4>())
            return GlVertexAttribPointerType.Float;
        if (Is<Vec2d, Vec3d, Vec4d>())
            return GlVertexAttribPointerType.Double;
        if (Is<Vec2h, Vec3h, Vec4h>())
            return GlVertexAttribPointerType.HalfFloat;
        if (Is<Vec2i8, Vec3i8, Vec4i8>())
            return GlVertexAttribPointerType.Byte;
        if (Is<Vec2u8, Vec3u8, Vec4u8>())
            return GlVertexAttribPointerType.UnsignedByte;
        if (Is<Vec2i16, Vec3i16, Vec4i16>())
            return GlVertexAttribPointerType.Short;
        if (Is<Vec2u16, Vec3u16, Vec4u16>())
            return GlVertexAttribPointerType.UnsignedShort;
        if (Is<Vec2i, Vec3i, Vec4i>())
            return GlVertexAttribPointerType.Int;
        if (Is<Vec2u, Vec3u, Vec4u>())
            return GlVertexAttribPointerType.UnsignedInt;
        return default;
    }

    private static GlVertexAttribIType CreateIntegerType()
    {
        if (Is<Vec2i8, Vec3i8, Vec4i8>())
            return GlVertexAttribIType.Byte;
        if (Is<Vec2u8, Vec3u8, Vec4u8>())
            return GlVertexAttribIType.UnsignedByte;
        if (Is<Vec2i16, Vec3i16, Vec4i16>())
            return GlVertexAttribIType.Short;
        if (Is<Vec2u16, Vec3u16, Vec4u16>())
            return GlVertexAttribIType.UnsignedShort;
        if (Is<Vec2i, Vec3i, Vec4i>())
            return GlVertexAttribIType.Int;
        if (Is<Vec2u, Vec3u, Vec4u>())
            return GlVertexAttribIType.UnsignedInt;
        return default;
    }

    private static bool IsSupportedNormal() => componentCount != 0;

    private static bool IsSupportedInteger() =>
        Is<Vec2i8, Vec3i8, Vec4i8>() || Is<Vec2u8, Vec3u8, Vec4u8>() ||
        Is<Vec2i16, Vec3i16, Vec4i16>() || Is<Vec2u16, Vec3u16, Vec4u16>() ||
        Is<Vec2i, Vec3i, Vec4i>() || Is<Vec2u, Vec3u, Vec4u>();

    private static bool IsDouble() => Is<Vec2d, Vec3d, Vec4d>();

    private static bool Is<T2, T3, T4>() =>
        VectorType == typeof(T2) || VectorType == typeof(T3) || VectorType == typeof(T4);

    [DoesNotReturn]
    private static void ThrowUnsupported(string api) =>
        throw new NotSupportedException($"{typeof(TVector).Name} is not supported by {api}.");
}
