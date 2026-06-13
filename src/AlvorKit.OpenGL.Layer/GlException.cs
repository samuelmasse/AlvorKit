namespace AlvorKit.OpenGL.Layer;

public class GlException(string function, string message) : Exception($"gl{function}: {message}")
{
    public string Function { get; } = function;
}

public sealed class GlAlreadyBoundException(string function, string message) : GlException(function, message);

public sealed class GlNotBoundException(string function, string message) : GlException(function, message);

public sealed class GlBindConflictException(string function, string message) : GlException(function, message);

public sealed class GlMissingPrerequisiteException(string function, string message) : GlException(function, message);
