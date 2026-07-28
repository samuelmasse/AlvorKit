namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>
/// Adds containing-constructor relationship metadata to loaded operation resolution.
/// </summary>
public interface ILoadedConstructorMetadataResolver :
    ILoadedOperationMetadataResolver
{
    /// <summary>
    /// Classifies a token only when it names the containing type's direct-base or delegating
    /// instance constructor.
    /// </summary>
    bool TryResolveInitializerKind(
        int metadataToken,
        [NotNullWhen(true)] out LoadedConstructorInitializerKind? kind);
}
