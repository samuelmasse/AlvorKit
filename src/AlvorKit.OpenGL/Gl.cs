namespace AlvorKit.OpenGL;

/// <summary>
/// Raw OpenGL bindings resolved at runtime through a platform proc loader
/// (e.g. Rgfw.GetProcAddressOpenGL). Call <see cref="Load"/> once after the
/// GL context is current.
/// </summary>
public static unsafe class Gl
{
    public const uint ColorBufferBit = 0x4000;
    public const uint Texture2D = 0x0DE1;
    public const uint TextureMinFilter = 0x2801;
    public const uint TextureMagFilter = 0x2800;
    public const uint Linear = 0x2601;
    public const uint UnpackAlignment = 0x0CF5;
    public const uint R8 = 0x8229;
    public const uint Red = 0x1903;
    public const uint UnsignedByte = 0x1401;
    public const uint VertexShader = 0x8B31;
    public const uint FragmentShader = 0x8B30;
    public const uint CompileStatus = 0x8B81;
    public const uint LinkStatus = 0x8B82;
    public const uint ArrayBuffer = 0x8892;
    public const uint StaticDraw = 0x88E4;
    public const uint Float = 0x1406;
    public const uint TriangleStrip = 0x0005;
    public const uint Texture0 = 0x84C0;

    private static delegate* unmanaged<int, int, int, int, void> viewport;
    private static delegate* unmanaged<float, float, float, float, void> clearColor;
    private static delegate* unmanaged<uint, void> clear;
    private static delegate* unmanaged<uint, int, void> pixelStorei;
    private static delegate* unmanaged<int, uint*, void> genTextures;
    private static delegate* unmanaged<uint, uint, void> bindTexture;
    private static delegate* unmanaged<uint, uint, int, void> texParameteri;
    private static delegate* unmanaged<uint, int, int, int, int, int, uint, uint, void*, void> texImage2D;
    private static delegate* unmanaged<uint, void> activeTexture;
    private static delegate* unmanaged<uint, uint> createShader;
    private static delegate* unmanaged<uint, int, byte**, int*, void> shaderSource;
    private static delegate* unmanaged<uint, void> compileShader;
    private static delegate* unmanaged<uint, uint, int*, void> getShaderiv;
    private static delegate* unmanaged<uint> createProgram;
    private static delegate* unmanaged<uint, uint, void> attachShader;
    private static delegate* unmanaged<uint, void> linkProgram;
    private static delegate* unmanaged<uint, uint, int*, void> getProgramiv;
    private static delegate* unmanaged<uint, void> useProgram;
    private static delegate* unmanaged<uint, byte*, int> getUniformLocation;
    private static delegate* unmanaged<int, int, void> uniform1i;
    private static delegate* unmanaged<int, uint*, void> genVertexArrays;
    private static delegate* unmanaged<uint, void> bindVertexArray;
    private static delegate* unmanaged<int, uint*, void> genBuffers;
    private static delegate* unmanaged<uint, uint, void> bindBuffer;
    private static delegate* unmanaged<uint, nint, void*, uint, void> bufferData;
    private static delegate* unmanaged<uint, int, uint, byte, int, void*, void> vertexAttribPointer;
    private static delegate* unmanaged<uint, void> enableVertexAttribArray;
    private static delegate* unmanaged<uint, int, int, void> drawArrays;
    private static delegate* unmanaged<uint, int, int*, byte*, void> getShaderInfoLog;
    private static delegate* unmanaged<uint, int, int*, byte*, void> getProgramInfoLog;

    public static void Load(Func<string, nint> getProcAddress)
    {
        nint Get(string name) => getProcAddress(name) is not 0 and var proc
            ? proc : throw new EntryPointNotFoundException($"OpenGL function not found: {name}");

        viewport = (delegate* unmanaged<int, int, int, int, void>)Get("glViewport");
        clearColor = (delegate* unmanaged<float, float, float, float, void>)Get("glClearColor");
        clear = (delegate* unmanaged<uint, void>)Get("glClear");
        pixelStorei = (delegate* unmanaged<uint, int, void>)Get("glPixelStorei");
        genTextures = (delegate* unmanaged<int, uint*, void>)Get("glGenTextures");
        bindTexture = (delegate* unmanaged<uint, uint, void>)Get("glBindTexture");
        texParameteri = (delegate* unmanaged<uint, uint, int, void>)Get("glTexParameteri");
        texImage2D = (delegate* unmanaged<uint, int, int, int, int, int, uint, uint, void*, void>)Get("glTexImage2D");
        activeTexture = (delegate* unmanaged<uint, void>)Get("glActiveTexture");
        createShader = (delegate* unmanaged<uint, uint>)Get("glCreateShader");
        shaderSource = (delegate* unmanaged<uint, int, byte**, int*, void>)Get("glShaderSource");
        compileShader = (delegate* unmanaged<uint, void>)Get("glCompileShader");
        getShaderiv = (delegate* unmanaged<uint, uint, int*, void>)Get("glGetShaderiv");
        createProgram = (delegate* unmanaged<uint>)Get("glCreateProgram");
        attachShader = (delegate* unmanaged<uint, uint, void>)Get("glAttachShader");
        linkProgram = (delegate* unmanaged<uint, void>)Get("glLinkProgram");
        getProgramiv = (delegate* unmanaged<uint, uint, int*, void>)Get("glGetProgramiv");
        useProgram = (delegate* unmanaged<uint, void>)Get("glUseProgram");
        getUniformLocation = (delegate* unmanaged<uint, byte*, int>)Get("glGetUniformLocation");
        uniform1i = (delegate* unmanaged<int, int, void>)Get("glUniform1i");
        genVertexArrays = (delegate* unmanaged<int, uint*, void>)Get("glGenVertexArrays");
        bindVertexArray = (delegate* unmanaged<uint, void>)Get("glBindVertexArray");
        genBuffers = (delegate* unmanaged<int, uint*, void>)Get("glGenBuffers");
        bindBuffer = (delegate* unmanaged<uint, uint, void>)Get("glBindBuffer");
        bufferData = (delegate* unmanaged<uint, nint, void*, uint, void>)Get("glBufferData");
        vertexAttribPointer = (delegate* unmanaged<uint, int, uint, byte, int, void*, void>)Get("glVertexAttribPointer");
        enableVertexAttribArray = (delegate* unmanaged<uint, void>)Get("glEnableVertexAttribArray");
        drawArrays = (delegate* unmanaged<uint, int, int, void>)Get("glDrawArrays");
        getShaderInfoLog = (delegate* unmanaged<uint, int, int*, byte*, void>)Get("glGetShaderInfoLog");
        getProgramInfoLog = (delegate* unmanaged<uint, int, int*, byte*, void>)Get("glGetProgramInfoLog");
    }

    public static void Viewport(int x, int y, int width, int height) => viewport(x, y, width, height);
    public static void ClearColor(float r, float g, float b, float a) => clearColor(r, g, b, a);
    public static void Clear(uint mask) => clear(mask);
    public static void PixelStorei(uint name, int value) => pixelStorei(name, value);
    public static void BindTexture(uint target, uint texture) => bindTexture(target, texture);
    public static void TexParameteri(uint target, uint name, int value) => texParameteri(target, name, value);
    public static void ActiveTexture(uint unit) => activeTexture(unit);
    public static uint CreateShader(uint type) => createShader(type);
    public static void CompileShader(uint shader) => compileShader(shader);
    public static uint CreateProgram() => createProgram();
    public static void AttachShader(uint program, uint shader) => attachShader(program, shader);
    public static void LinkProgram(uint program) => linkProgram(program);
    public static void UseProgram(uint program) => useProgram(program);
    public static void Uniform1i(int location, int value) => uniform1i(location, value);
    public static void BindVertexArray(uint array) => bindVertexArray(array);
    public static void BindBuffer(uint target, uint buffer) => bindBuffer(target, buffer);
    public static void EnableVertexAttribArray(uint index) => enableVertexAttribArray(index);
    public static void DrawArrays(uint mode, int first, int count) => drawArrays(mode, first, count);

    public static uint GenTexture()
    {
        uint texture;
        genTextures(1, &texture);
        return texture;
    }

    public static uint GenVertexArray()
    {
        uint array;
        genVertexArrays(1, &array);
        return array;
    }

    public static uint GenBuffer()
    {
        uint buffer;
        genBuffers(1, &buffer);
        return buffer;
    }

    public static void TexImage2D(uint target, int level, uint internalFormat, int width, int height, uint format, uint type, ReadOnlySpan<byte> pixels)
    {
        fixed (byte* p = pixels)
            texImage2D(target, level, (int)internalFormat, width, height, 0, format, type, p);
    }

    public static void ShaderSource(uint shader, string source)
    {
        var bytes = Encoding.UTF8.GetBytes(source);
        fixed (byte* p = bytes)
        {
            var length = bytes.Length;
            var pointer = p;
            shaderSource(shader, 1, &pointer, &length);
        }
    }

    public static int GetShader(uint shader, uint name)
    {
        int value;
        getShaderiv(shader, name, &value);
        return value;
    }

    public static int GetProgram(uint program, uint name)
    {
        int value;
        getProgramiv(program, name, &value);
        return value;
    }

    public static int GetUniformLocation(uint program, string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name + '\0');
        fixed (byte* p = bytes)
            return getUniformLocation(program, p);
    }

    public static void BufferData(uint target, ReadOnlySpan<float> data, uint usage)
    {
        fixed (float* p = data)
            bufferData(target, data.Length * sizeof(float), p, usage);
    }

    public static void VertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, int offset) =>
        vertexAttribPointer(index, size, type, normalized ? (byte)1 : (byte)0, stride, (void*)offset);

    public static string GetShaderInfoLog(uint shader)
    {
        var buffer = stackalloc byte[1024];
        int length;
        getShaderInfoLog(shader, 1024, &length, buffer);
        return Encoding.UTF8.GetString(buffer, length);
    }

    public static string GetProgramInfoLog(uint program)
    {
        var buffer = stackalloc byte[1024];
        int length;
        getProgramInfoLog(program, 1024, &length, buffer);
        return Encoding.UTF8.GetString(buffer, length);
    }
}
