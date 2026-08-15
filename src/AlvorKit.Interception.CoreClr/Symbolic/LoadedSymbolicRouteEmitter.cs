using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Emits one symbolic exact-site route into a composed generation.</summary>
internal static class LoadedSymbolicRouteEmitter
{
    /// <summary>The symbolic neutral route resolver signature.</summary>
    private const string RouteResolverSignature =
        "AlvorKit.Interception.ResolveExactRoute(site,method,type)";

    /// <summary>Emits one spill, inert fallback, active calli, and common merge block.</summary>
    internal static void Emit(
        LoadedSymbolicEdit edit,
        string constructedContext,
        ImmutableArray<LoadedSymbolicInstruction>.Builder instructions,
        ImmutableArray<LoadedSymbolicRelocation>.Builder relocations,
        ImmutableArray<LoadedSymbolicIlMapEntry>.Builder ilMap)
    {
        var site = edit.Site;
        var active = $"{site.StableId}:active";
        var merge = $"{site.StableId}:merge";
        var routeStart = instructions.Count;
        var aliases = ImmutableArray.CreateBuilder<string>(
            edit.Prefixes.Length + 2);
        aliases.Add($"{site.StableId}:start");
        foreach (var prefix in edit.Prefixes)
            aliases.Add(LoadedSymbolicEmitter.Label(prefix.BaselineOffset));
        aliases.Add(LoadedSymbolicEmitter.Label(edit.Operation.BaselineOffset));

        AddSynthetic(
            instructions,
            LoadedSymbolicInstructionKind.SpillOperands,
            aliases.MoveToImmutable(),
            site,
            []);
        AddRelocation(
            relocations,
            LoadedSymbolicRelocationKind.ExactOperandLocals,
            routeStart,
            site,
            "locals",
            site.CanonicalSignature);
        foreach (var prefix in edit.Prefixes)
        {
            ilMap.Add(new(
                prefix.BaselineOffset,
                routeStart,
                LoadedSymbolicEmitter.Label(prefix.BaselineOffset)));
        }
        ilMap.Add(new(
            edit.Operation.BaselineOffset,
            routeStart,
            LoadedSymbolicEmitter.Label(edit.Operation.BaselineOffset)));

        var resolveIndex = instructions.Count;
        AddSynthetic(
            instructions,
            LoadedSymbolicInstructionKind.ResolveRoute,
            [],
            site,
            []);
        AddRelocation(
            relocations,
            LoadedSymbolicRelocationKind.RouteResolverMethod,
            resolveIndex,
            site,
            "resolver",
            RouteResolverSignature);
        AddContextRelocations(
            constructedContext,
            resolveIndex,
            site,
            relocations);
        AddSynthetic(
            instructions,
            LoadedSymbolicInstructionKind.BranchIfRoute,
            [],
            site,
            [active]);
        AddSynthetic(
            instructions,
            LoadedSymbolicInstructionKind.ReloadOperands,
            [$"{site.StableId}:miss"],
            site,
            []);
        foreach (var prefix in edit.Prefixes)
        {
            instructions.Add(new(
                LoadedSymbolicInstructionKind.ReplayPrefix,
                [],
                prefix.BaselineOffset,
                prefix.OpCodeValue,
                prefix.Operand,
                [],
                site.StableId,
                site.CanonicalSignature));
        }
        instructions.Add(new(
            LoadedSymbolicInstructionKind.ReplayOriginal,
            [],
            edit.Operation.BaselineOffset,
            edit.Operation.OpCodeValue,
            edit.Operation.Operand,
            [],
            site.StableId,
            site.CanonicalSignature));
        AddSynthetic(
            instructions,
            LoadedSymbolicInstructionKind.Branch,
            [],
            site,
            [merge]);
        AddSynthetic(
            instructions,
            LoadedSymbolicInstructionKind.ReloadOperands,
            [active],
            site,
            []);
        var callIndex = instructions.Count;
        AddSynthetic(
            instructions,
            LoadedSymbolicInstructionKind.CallIndirect,
            [],
            site,
            []);
        AddRelocation(
            relocations,
            LoadedSymbolicRelocationKind.CallSiteSignature,
            callIndex,
            site,
            "calli",
            site.CanonicalSignature);
        AddSynthetic(
            instructions,
            LoadedSymbolicInstructionKind.Merge,
            [merge],
            site,
            []);
    }

    /// <summary>Adds generic context handle relocations when the caller is constructed.</summary>
    private static void AddContextRelocations(
        string constructedContext,
        int resolveIndex,
        LoadedOperationSiteDescriptor site,
        ImmutableArray<LoadedSymbolicRelocation>.Builder relocations)
    {
        if (string.IsNullOrEmpty(constructedContext))
            return;
        AddRelocation(
            relocations,
            LoadedSymbolicRelocationKind.ConstructedMethodHandle,
            resolveIndex,
            site,
            "method-handle",
            constructedContext);
        AddRelocation(
            relocations,
            LoadedSymbolicRelocationKind.ConstructedTypeHandle,
            resolveIndex,
            site,
            "type-handle",
            constructedContext);
    }

    /// <summary>Adds one route-local symbolic instruction.</summary>
    private static void AddSynthetic(
        ImmutableArray<LoadedSymbolicInstruction>.Builder instructions,
        LoadedSymbolicInstructionKind kind,
        ImmutableArray<string> labels,
        LoadedOperationSiteDescriptor site,
        ImmutableArray<string> targets) =>
        instructions.Add(new(
            kind,
            labels,
            site.BaselineOffset,
            0,
            LoadedIlOperand.None,
            targets,
            site.StableId,
            site.CanonicalSignature));

    /// <summary>Adds one deterministic token-free relocation.</summary>
    private static void AddRelocation(
        ImmutableArray<LoadedSymbolicRelocation>.Builder relocations,
        LoadedSymbolicRelocationKind kind,
        int instructionIndex,
        LoadedOperationSiteDescriptor site,
        string suffix,
        string signature) =>
        relocations.Add(new(
            kind,
            instructionIndex,
            $"{site.StableId}:{suffix}",
            site.StableId,
            signature));
}
