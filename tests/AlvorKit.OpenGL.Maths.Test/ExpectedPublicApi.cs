namespace AlvorKit.OpenGL.Maths.Test;

internal static class ExpectedPublicApi
{
    private static readonly string GlType = TypeName(typeof(Gl));

    public static HashSet<string> Create()
    {
        var api = new HashSet<string>(StringComparer.Ordinal);

        AddState(api);
        AddUniforms(api);
        AddMatrices(api);
        AddTextures(api);
        AddFramebuffers(api);
        Add(api, "DispatchCompute", TypeName(typeof(Vec3u)));
        AddVertexAttributes(api);

        return api;
    }

    public static string Format(MethodInfo method)
    {
        var genericArguments = method.IsGenericMethodDefinition
            ? $"<{string.Join(',', method.GetGenericArguments().Select(static argument => argument.Name))}>"
            : string.Empty;
        var parameters = string.Join(',', method.GetParameters().Select(Format));
        return $"{method.Name}{genericArguments}({parameters})";
    }

    private static void AddState(HashSet<string> api)
    {
        Add(api, "ClearColor", TypeName(typeof(Vec4)));
        Add(api, "BlendColor", TypeName(typeof(Vec4)));
        Add(api, "ColorMask", TypeName(typeof(Vec4b)));
        Add(api, "ColorMaski", TypeName(typeof(uint)), TypeName(typeof(Vec4b)));

        Add(api, "Viewport", TypeName(typeof(Vec2u)));
        Add(api, "Viewport", TypeName(typeof(Vec2i)), TypeName(typeof(Vec2u)));
        Add(api, "ViewportIndexedf", TypeName(typeof(uint)), TypeName(typeof(Vec4)));
        Add(api, "ViewportIndexedf", TypeName(typeof(uint)), TypeName(typeof(Vec2)), TypeName(typeof(Vec2)));
        Add(api, "ViewportArrayv", TypeName(typeof(uint)), ReadOnlySpan(typeof(Vec4)));

        Add(api, "Scissor", TypeName(typeof(Vec2u)));
        Add(api, "Scissor", TypeName(typeof(Vec2i)), TypeName(typeof(Vec2u)));
        Add(api, "ScissorIndexed", TypeName(typeof(uint)), TypeName(typeof(Vec2u)));
        Add(api, "ScissorIndexed", TypeName(typeof(uint)), TypeName(typeof(Vec2i)), TypeName(typeof(Vec2u)));
        Add(api, "ScissorArrayv", TypeName(typeof(uint)), ReadOnlySpan(typeof(Vec4i)));

        Add(api, "DepthRange", TypeName(typeof(Intervald)));
        Add(api, "DepthRangef", TypeName(typeof(Intervalf)));
        Add(api, "DepthRangeIndexed", TypeName(typeof(uint)), TypeName(typeof(Intervald)));
        Add(api, "DepthRangeArrayv", TypeName(typeof(uint)), ReadOnlySpan(typeof(Intervald)));
    }

    private static void AddUniforms(HashSet<string> api)
    {
        AddUniform(api, "2f", "2fv", typeof(Vec2));
        AddUniform(api, "3f", "3fv", typeof(Vec3));
        AddUniform(api, "4f", "4fv", typeof(Vec4));
        AddUniform(api, "4f", "4fv", typeof(Quat));

        AddUniform(api, "2d", "2dv", typeof(Vec2d));
        AddUniform(api, "3d", "3dv", typeof(Vec3d));
        AddUniform(api, "4d", "4dv", typeof(Vec4d));
        AddUniform(api, "4d", "4dv", typeof(Quatd));

        AddUniform(api, "2i", "2iv", typeof(Vec2i));
        AddUniform(api, "3i", "3iv", typeof(Vec3i));
        AddUniform(api, "4i", "4iv", typeof(Vec4i));

        AddUniform(api, "2ui", "2uiv", typeof(Vec2u));
        AddUniform(api, "3ui", "3uiv", typeof(Vec3u));
        AddUniform(api, "4ui", "4uiv", typeof(Vec4u));
    }

    private static void AddMatrices(HashSet<string> api)
    {
        AddMatrix(api, "2fv", typeof(Mat2));
        AddMatrix(api, "2x3fv", typeof(Mat2x3));
        AddMatrix(api, "2x4fv", typeof(Mat2x4));
        AddMatrix(api, "3x2fv", typeof(Mat3x2));
        AddMatrix(api, "3fv", typeof(Mat3));
        AddMatrix(api, "3x4fv", typeof(Mat3x4));
        AddMatrix(api, "4x2fv", typeof(Mat4x2));
        AddMatrix(api, "4x3fv", typeof(Mat4x3));
        AddMatrix(api, "4fv", typeof(Mat4));

        AddMatrix(api, "2dv", typeof(Mat2d));
        AddMatrix(api, "2x3dv", typeof(Mat2x3d));
        AddMatrix(api, "2x4dv", typeof(Mat2x4d));
        AddMatrix(api, "3x2dv", typeof(Mat3x2d));
        AddMatrix(api, "3dv", typeof(Mat3d));
        AddMatrix(api, "3x4dv", typeof(Mat3x4d));
        AddMatrix(api, "4x2dv", typeof(Mat4x2d));
        AddMatrix(api, "4x3dv", typeof(Mat4x3d));
        AddMatrix(api, "4dv", typeof(Mat4d));
    }

    private static void AddTextures(HashSet<string> api)
    {
        var textureTarget = TypeName(typeof(GlTextureTarget));
        var texture = TypeName(typeof(GlTextureHandle));
        var internalFormat = TypeName(typeof(GlInternalFormat));
        var sizedInternalFormat = TypeName(typeof(GlSizedInternalFormat));
        var pixelFormat = TypeName(typeof(GlPixelFormat));
        var pixelType = TypeName(typeof(GlPixelType));
        var copyTarget = TypeName(typeof(GlCopyImageSubDataTarget));
        var integer = TypeName(typeof(int));
        var unsigned = TypeName(typeof(uint));
        var pointer = TypeName(typeof(nint));
        var boolean = TypeName(typeof(bool));
        var vec2i = TypeName(typeof(Vec2i));
        var vec2u = TypeName(typeof(Vec2u));
        var vec3i = TypeName(typeof(Vec3i));
        var vec3u = TypeName(typeof(Vec3u));

        Add(api, "TexImage2D", textureTarget, integer, internalFormat, vec2u, integer, pixelFormat, pixelType, pointer);
        AddGeneric(api, "TexImage2D", "T", textureTarget, integer, internalFormat, vec2u, integer,
            pixelFormat, pixelType, ReadOnlySpan("T"));
        Add(api, "TexImage3D", textureTarget, integer, internalFormat, vec3u, integer, pixelFormat, pixelType, pointer);
        AddGeneric(api, "TexImage3D", "T", textureTarget, integer, internalFormat, vec3u, integer,
            pixelFormat, pixelType, ReadOnlySpan("T"));
        Add(api, "CompressedTexImage2D", textureTarget, integer, internalFormat, vec2u, integer, integer, pointer);
        Add(api, "CompressedTexImage3D", textureTarget, integer, internalFormat, vec3u, integer, integer, pointer);
        Add(api, "TexImage2DMultisample", textureTarget, integer, internalFormat, vec2u, boolean);
        Add(api, "TexImage3DMultisample", textureTarget, integer, internalFormat, vec3u, boolean);

        Add(api, "TexStorage2D", textureTarget, integer, sizedInternalFormat, vec2u);
        Add(api, "TexStorage3D", textureTarget, integer, sizedInternalFormat, vec3u);
        Add(api, "TexStorage2DMultisample", textureTarget, integer, sizedInternalFormat, vec2u, boolean);
        Add(api, "TexStorage3DMultisample", textureTarget, integer, sizedInternalFormat, vec3u, boolean);
        Add(api, "TextureStorage2D", texture, integer, sizedInternalFormat, vec2u);
        Add(api, "TextureStorage3D", texture, integer, sizedInternalFormat, vec3u);
        Add(api, "TextureStorage2DMultisample", texture, integer, sizedInternalFormat, vec2u, boolean);
        Add(api, "TextureStorage3DMultisample", texture, integer, sizedInternalFormat, vec3u, boolean);

        Add(api, "TexSubImage2D", textureTarget, integer, vec2i, vec2u, pixelFormat, pixelType, pointer);
        AddGeneric(api, "TexSubImage2D", "T", textureTarget, integer, vec2i, vec2u,
            pixelFormat, pixelType, ReadOnlySpan("T"));
        Add(api, "TexSubImage3D", textureTarget, integer, vec3i, vec3u, pixelFormat, pixelType, pointer);
        AddGeneric(api, "TexSubImage3D", "T", textureTarget, integer, vec3i, vec3u,
            pixelFormat, pixelType, ReadOnlySpan("T"));
        Add(api, "CompressedTexSubImage2D", textureTarget, integer, vec2i, vec2u, internalFormat, integer, pointer);
        Add(api, "CompressedTexSubImage3D", textureTarget, integer, vec3i, vec3u, internalFormat, integer, pointer);
        Add(api, "TextureSubImage2D", texture, integer, vec2i, vec2u, pixelFormat, pixelType, pointer);
        AddGeneric(api, "TextureSubImage2D", "T", texture, integer, vec2i, vec2u,
            pixelFormat, pixelType, ReadOnlySpan("T"));
        Add(api, "TextureSubImage3D", texture, integer, vec3i, vec3u, pixelFormat, pixelType, pointer);
        AddGeneric(api, "TextureSubImage3D", "T", texture, integer, vec3i, vec3u,
            pixelFormat, pixelType, ReadOnlySpan("T"));
        Add(api, "CompressedTextureSubImage2D", texture, integer, vec2i, vec2u, internalFormat, integer, pointer);
        Add(api, "CompressedTextureSubImage3D", texture, integer, vec3i, vec3u, internalFormat, integer, pointer);

        Add(api, "CopyTexImage1D", textureTarget, integer, internalFormat, vec2i, unsigned, integer);
        Add(api, "CopyTexImage2D", textureTarget, integer, internalFormat, vec2i, vec2u, integer);
        Add(api, "CopyTexSubImage1D", textureTarget, integer, integer, vec2i, unsigned);
        Add(api, "CopyTexSubImage2D", textureTarget, integer, vec2i, vec2i, vec2u);
        Add(api, "CopyTexSubImage3D", textureTarget, integer, vec3i, vec2i, vec2u);
        Add(api, "CopyTextureSubImage1D", texture, integer, integer, vec2i, unsigned);
        Add(api, "CopyTextureSubImage2D", texture, integer, vec2i, vec2i, vec2u);
        Add(api, "CopyTextureSubImage3D", texture, integer, vec3i, vec2i, vec2u);
        Add(api, "CopyImageSubData", unsigned, copyTarget, integer, vec3i,
            unsigned, copyTarget, integer, vec3i, vec3u);

        Add(api, "ClearTexSubImage", texture, integer, vec3i, vec3u, pixelFormat, pixelType, pointer);
        Add(api, "InvalidateTexSubImage", texture, integer, vec3i, vec3u);
        Add(api, "GetTextureSubImage", texture, integer, vec3i, vec3u, pixelFormat, pixelType, integer, pointer);
        Add(api, "GetCompressedTextureSubImage", texture, integer, vec3i, vec3u, integer, pointer);
    }

    private static void AddFramebuffers(HashSet<string> api)
    {
        var target = TypeName(typeof(GlRenderbufferTarget));
        var renderbuffer = TypeName(typeof(GlRenderbufferHandle));
        var framebuffer = TypeName(typeof(GlFramebufferHandle));
        var internalFormat = TypeName(typeof(GlInternalFormat));
        var pixelFormat = TypeName(typeof(GlPixelFormat));
        var pixelType = TypeName(typeof(GlPixelType));
        var mask = TypeName(typeof(GlClearBufferMask));
        var filter = TypeName(typeof(GlBlitFramebufferFilter));
        var integer = TypeName(typeof(int));
        var pointer = TypeName(typeof(nint));
        var vec2i = TypeName(typeof(Vec2i));
        var vec2u = TypeName(typeof(Vec2u));
        var vec4i = TypeName(typeof(Vec4i));

        Add(api, "RenderbufferStorage", target, internalFormat, vec2u);
        Add(api, "RenderbufferStorageMultisample", target, integer, internalFormat, vec2u);
        Add(api, "NamedRenderbufferStorage", renderbuffer, internalFormat, vec2u);
        Add(api, "NamedRenderbufferStorageMultisample", renderbuffer, integer, internalFormat, vec2u);
        Add(api, "ReadPixels", vec2i, vec2u, pixelFormat, pixelType, pointer);
        AddGeneric(api, "ReadPixels", "T", vec2i, vec2u, pixelFormat, pixelType, Span("T"));
        Add(api, "ReadPixels", vec2u, pixelFormat, pixelType, pointer);
        AddGeneric(api, "ReadPixels", "T", vec2u, pixelFormat, pixelType, Span("T"));
        Add(api, "ReadnPixels", vec2i, vec2u, pixelFormat, pixelType, integer, pointer);
        Add(api, "ReadnPixels", vec2u, pixelFormat, pixelType, integer, pointer);
        Add(api, "BlitFramebuffer", vec2i, vec2u, vec2i, vec2u, mask, filter);
        Add(api, "BlitNamedFramebuffer", framebuffer, framebuffer, vec2i, vec2u, vec2i, vec2u, mask, filter);
        Add(api, "BlitFramebuffer", vec4i, vec4i, mask, filter);
        Add(api, "BlitNamedFramebuffer", framebuffer, framebuffer, vec4i, vec4i, mask, filter);
    }

    private static void AddVertexAttributes(HashSet<string> api)
    {
        var unsigned = TypeName(typeof(uint));
        var integer = TypeName(typeof(int));
        var boolean = TypeName(typeof(bool));
        var pointer = TypeName(typeof(nint));
        var vertexArray = TypeName(typeof(GlVertexArrayHandle));

        AddGeneric(api, "VertexAttribPointer", "TVector", unsigned, boolean, integer, pointer);
        AddGeneric(api, "VertexAttribIPointer", "TVector", unsigned, integer, pointer);
        AddGeneric(api, "VertexAttribLPointer", "TVector", unsigned, integer, pointer);
        AddGeneric(api, "VertexAttribFormat", "TVector", unsigned, boolean, unsigned);
        AddGeneric(api, "VertexAttribIFormat", "TVector", unsigned, unsigned);
        AddGeneric(api, "VertexAttribLFormat", "TVector", unsigned, unsigned);
        AddGeneric(api, "VertexArrayAttribFormat", "TVector", vertexArray, unsigned, boolean, unsigned);
        AddGeneric(api, "VertexArrayAttribIFormat", "TVector", vertexArray, unsigned, unsigned);
        AddGeneric(api, "VertexArrayAttribLFormat", "TVector", vertexArray, unsigned, unsigned);

        AddVertexValues(api, "2f", typeof(Vec2));
        AddVertexValues(api, "3f", typeof(Vec3));
        AddVertexValues(api, "4f", typeof(Vec4));
        AddVertexValues(api, "2d", typeof(Vec2d));
        AddVertexValues(api, "3d", typeof(Vec3d));
        AddVertexValues(api, "4d", typeof(Vec4d));
        AddVertexValues(api, "I2i", typeof(Vec2i));
        AddVertexValues(api, "I3i", typeof(Vec3i));
        AddVertexValues(api, "I4i", typeof(Vec4i));
        AddVertexValues(api, "I2ui", typeof(Vec2u));
        AddVertexValues(api, "I3ui", typeof(Vec3u));
        AddVertexValues(api, "I4ui", typeof(Vec4u));
        AddVertexValues(api, "L2d", typeof(Vec2d));
        AddVertexValues(api, "L3d", typeof(Vec3d));
        AddVertexValues(api, "L4d", typeof(Vec4d));
    }

    private static void AddUniform(HashSet<string> api, string scalarSuffix, string arraySuffix, Type valueType)
    {
        var integer = TypeName(typeof(int));
        var program = TypeName(typeof(GlProgramHandle));
        var value = TypeName(valueType);

        Add(api, $"Uniform{scalarSuffix}", integer, value);
        Add(api, $"Uniform{arraySuffix}", integer, ReadOnlySpan(valueType));
        Add(api, $"ProgramUniform{scalarSuffix}", program, integer, value);
        Add(api, $"ProgramUniform{arraySuffix}", program, integer, ReadOnlySpan(valueType));
    }

    private static void AddMatrix(HashSet<string> api, string suffix, Type matrixType)
    {
        var integer = TypeName(typeof(int));
        var program = TypeName(typeof(GlProgramHandle));
        var matrix = TypeName(matrixType);

        Add(api, $"UniformMatrix{suffix}", integer, $"in {matrix}");
        Add(api, $"UniformMatrix{suffix}", integer, ReadOnlySpan(matrixType));
        Add(api, $"ProgramUniformMatrix{suffix}", program, integer, $"in {matrix}");
        Add(api, $"ProgramUniformMatrix{suffix}", program, integer, ReadOnlySpan(matrixType));
    }

    private static void AddVertexValues(HashSet<string> api, string suffix, Type valueType) =>
        Add(api, $"VertexAttrib{suffix}", TypeName(typeof(uint)), TypeName(valueType));

    private static void Add(HashSet<string> api, string name, params string[] parameters) =>
        api.Add($"{name}({GlType},{string.Join(',', parameters)})");

    private static void AddGeneric(
        HashSet<string> api,
        string name,
        string genericParameter,
        params string[] parameters) =>
        api.Add($"{name}<{genericParameter}>({GlType},{string.Join(',', parameters)})");

    private static string Format(ParameterInfo parameter)
    {
        if (!parameter.ParameterType.IsByRef)
            return TypeName(parameter.ParameterType);

        return $"in {TypeName(parameter.ParameterType.GetElementType()!)}";
    }

    private static string ReadOnlySpan(Type elementType) =>
        $"{GenericTypeName(typeof(ReadOnlySpan<>))}<{TypeName(elementType)}>";

    private static string ReadOnlySpan(string elementType) =>
        $"{GenericTypeName(typeof(ReadOnlySpan<>))}<{elementType}>";

    private static string Span(string elementType) =>
        $"{GenericTypeName(typeof(Span<>))}<{elementType}>";

    private static string GenericTypeName(Type type)
    {
        var name = type.FullName!;
        return name[..name.IndexOf('`')];
    }

    private static string TypeName(Type type)
    {
        if (type.IsGenericParameter)
            return type.Name;

        if (!type.IsGenericType)
            return type.FullName!;

        var name = GenericTypeName(type.GetGenericTypeDefinition());
        return $"{name}<{string.Join(',', type.GetGenericArguments().Select(TypeName))}>";
    }
}
