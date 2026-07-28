using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Reads and validates exact managed module identities without loading an assembly.</summary>
public static class CoreClrModuleIdentity
{
    /// <summary>Reads one managed PE module's metadata MVID from disk.</summary>
    public static Guid ReadModuleMvid(string modulePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modulePath);
        var fullPath = Path.GetFullPath(modulePath);
        using FileStream stream = File.OpenRead(fullPath);
        using PEReader portableExecutable = new(stream);
        if (!portableExecutable.HasMetadata)
        {
            throw new BadImageFormatException(
                $"Module '{fullPath}' does not contain managed metadata.",
                fullPath);
        }

        var metadata = portableExecutable.GetMetadataReader();
        var handle = metadata.GetModuleDefinition().Mvid;
        if (handle.IsNil)
        {
            throw new InvalidDataException(
                $"Managed module '{fullPath}' has no module version identifier.");
        }

        var moduleVersionId = metadata.GetGuid(handle);
        if (moduleVersionId == Guid.Empty)
        {
            throw new InvalidDataException(
                $"Managed module '{fullPath}' has an empty module version identifier.");
        }

        return moduleVersionId;
    }

    /// <summary>
    /// Requires an on-disk managed module to retain the exact expected loaded-module identity.
    /// </summary>
    public static void ValidateModuleMvid(
        string modulePath,
        Guid expectedModuleVersionId)
    {
        if (expectedModuleVersionId == Guid.Empty)
        {
            throw new ArgumentException(
                "An expected module version identifier is required.",
                nameof(expectedModuleVersionId));
        }

        var actualModuleVersionId = ReadModuleMvid(modulePath);
        if (actualModuleVersionId != expectedModuleVersionId)
        {
            throw new InvalidDataException(
                $"Managed module MVID mismatch for '{Path.GetFullPath(modulePath)}': " +
                $"expected {expectedModuleVersionId:D}, " +
                $"actual {actualModuleVersionId:D}.");
        }
    }
}
