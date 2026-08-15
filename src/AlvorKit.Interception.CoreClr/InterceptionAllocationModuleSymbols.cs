using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace AlvorKit;

/// <summary>Resolves one selected runtime module through its adjacent Portable PDB.</summary>
internal class InterceptionAllocationModuleSymbols : IDisposable
{
    /// <summary>Runtime module used for metadata-token method names.</summary>
    private readonly Module module;
    /// <summary>Owner of the optional Portable PDB metadata reader.</summary>
    private readonly MetadataReaderProvider? provider;
    /// <summary>Portable PDB reader when a valid adjacent symbol file exists.</summary>
    private readonly MetadataReader? reader;
    /// <summary>Whether <see cref="reader"/> contains a usable Portable PDB.</summary>
    private readonly bool hasReader;

    /// <summary>Loads an adjacent Portable PDB when the module has one in the supported format.</summary>
    internal InterceptionAllocationModuleSymbols(Module module)
    {
        this.module = module;
        var pdbPath = Path.ChangeExtension(module.FullyQualifiedName, ".pdb");
        if (!File.Exists(pdbPath))
            return;

        try
        {
            provider = MetadataReaderProvider.FromPortablePdbImage(
                ImmutableArray.Create(File.ReadAllBytes(pdbPath)));
            reader = provider.GetMetadataReader();
            hasReader = true;
        }
        catch (BadImageFormatException)
        {
            provider?.Dispose();
        }
        catch (IOException)
        {
            provider?.Dispose();
        }
    }

    /// <summary>Resolves a retained metadata frame to a readable method and optional source line.</summary>
    internal InterceptionAllocationSourceFrame Resolve(InterceptionAllocationStackFrame frame)
    {
        var method = MethodName(frame.MethodToken);
        if (!hasReader || frame.IlOffset is not { } ilOffset)
            return new(method, null, null);

        var row = frame.MethodToken & 0x00ff_ffff;
        if ((frame.MethodToken & unchecked((int)0xff00_0000)) != 0x0600_0000 || row == 0)
            return new(method, null, null);

        var pdbReader = reader!;
        var debug = pdbReader.GetMethodDebugInformation(MetadataTokens.MethodDebugInformationHandle(row));
        SequencePoint? best = null;
        foreach (var point in debug.GetSequencePoints())
        {
            if (point.IsHidden || point.Offset > ilOffset)
                continue;
            if (best is null || point.Offset >= best.Value.Offset)
                best = point;
        }

        if (best is not { } sequencePoint)
            return new(method, null, null);
        var documentHandle = sequencePoint.Document.IsNil ? debug.Document : sequencePoint.Document;
        if (documentHandle.IsNil)
            return new(method, null, null);
        var document = pdbReader.GetDocument(documentHandle);
        return new(method, pdbReader.GetString(document.Name), sequencePoint.StartLine);
    }

    /// <summary>Releases the optional Portable PDB reader owner.</summary>
    public void Dispose() =>
        provider?.Dispose();

    /// <summary>Resolves a method token to its declaring type and method name.</summary>
    private string MethodName(int token)
    {
        try
        {
            var method = module.ResolveMethod(token);
            var typeName = method?.DeclaringType?.FullName;
            return typeName is null ? method?.Name ?? $"0x{token:X8}" : $"{typeName}.{method!.Name}";
        }
        catch (ArgumentException)
        {
            return $"0x{token:X8}";
        }
        catch (BadImageFormatException)
        {
            return $"0x{token:X8}";
        }
    }
}
