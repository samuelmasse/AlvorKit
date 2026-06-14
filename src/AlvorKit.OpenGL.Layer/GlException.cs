namespace AlvorKit.OpenGL.Layer;

/// <summary>Base for every strict-usage violation the layer detects; the message is prefixed with the GL function.</summary>
public class GlException(string function, string message) : Exception($"gl{function}: {message}")
{
    /// <summary>The GL function whose strict-usage contract was violated.</summary>
    public string Function { get; } = function;
}

/// <summary>A nonzero object was bound over a target that already has one bound.</summary>
public sealed class GlAlreadyBoundException(string function, string message) : GlException(function, message);

/// <summary>An unbind was attempted on a target that has nothing bound.</summary>
public sealed class GlNotBoundException(string function, string message) : GlException(function, message);

/// <summary>A bind would conflict with another live binding (e.g. binding a VAO while a VBO is bound).</summary>
public sealed class GlBindConflictException(string function, string message) : GlException(function, message);

/// <summary>A call needs another piece of state set first (e.g. a texture bind without an active texture unit).</summary>
public sealed class GlMissingPrerequisiteException(string function, string message) : GlException(function, message);

/// <summary>A state value was set while it was already set; reset it first.</summary>
public sealed class GlAlreadySetException(string function, string message) : GlException(function, message);

/// <summary>A state value was reset while it was not set.</summary>
public sealed class GlAlreadyUnsetException(string function, string message) : GlException(function, message);

/// <summary>A setter from one family was used while a mutually-exclusive family is active (e.g. glBlendFunc vs glBlendFuncSeparate).</summary>
public sealed class GlConflictException(string function, string conflict) : GlException(function, $"cannot be used while gl{conflict} is active.");

/// <summary>A resource delete or size-tracking call referenced an object the layer does not own.</summary>
public sealed class GlResourceNotTrackedException<THandle>(string function, string resourceName, THandle handle)
    : GlException(function, $"{resourceName} {handle} is not tracked.");
