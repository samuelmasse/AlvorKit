namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Exception thrown when one state family is used while a mutually exclusive family is active.
/// </summary>
/// <param name="function">The GL function whose strict-usage contract was violated.</param>
/// <param name="conflict">The active GL function family that conflicts with the attempted call.</param>
public sealed class GlConflictException(string function, string conflict) : GlException(function, $"cannot be used while gl{conflict} is active.");
