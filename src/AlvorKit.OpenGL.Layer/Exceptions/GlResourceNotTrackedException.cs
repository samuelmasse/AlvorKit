namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Exception thrown when a delete or size-tracking call references an object the layer does not own.
/// </summary>
/// <typeparam name="THandle">The handle type used by the tracked GL resource.</typeparam>
/// <param name="function">The GL function whose strict-usage contract was violated.</param>
/// <param name="resourceName">The display name of the resource family.</param>
/// <param name="handle">The handle value that was not tracked.</param>
public sealed class GlResourceNotTrackedException<THandle>(string function, string resourceName, THandle handle)
    : GlException(function, $"{resourceName} {handle} is not tracked.");
