using static AlvorKit.Interception.CoreClr.Advanced.LoadedSourceMethodMetadata;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>
/// Resolves synchronous and compiler state-machine source methods to authoritative loaded bodies.
/// </summary>
public static class LoadedSourceMethodResolver
{
    /// <summary>
    /// Resolves source metadata without reading reflection method-body bytes or composing an IL map.
    /// </summary>
    public static LoadedSourceMethodResolution Resolve(
        MethodInfo sourceMethod,
        ILoadedMethodBodySnapshotResolver bodyResolver)
    {
        ArgumentNullException.ThrowIfNull(sourceMethod);
        ArgumentNullException.ThrowIfNull(bodyResolver);

        if (!TryTarget(
                sourceMethod,
                out var sourceTarget,
                out var sourceFailure))
        {
            return Rejected(
                LoadedSourceMethodRejectionReason.UnsupportedSourceMethod,
                TokenOrZero(sourceMethod),
                Display(sourceMethod),
                sourceFailure!);
        }

        var attributes = sourceMethod.CustomAttributes
            .Where(attribute =>
                typeof(StateMachineAttribute).IsAssignableFrom(
                    attribute.AttributeType))
            .OrderBy(
                attribute => attribute.AttributeType.FullName,
                StringComparer.Ordinal)
            .ToArray();
        if (attributes.Length == 0)
        {
            return ResolveBody(
                sourceMethod,
                sourceTarget,
                sourceMethod,
                sourceTarget,
                LoadedSourceMethodKind.Synchronous,
                bodyResolver);
        }

        var unsupported = attributes.FirstOrDefault(attribute =>
            !TryKind(attribute.AttributeType, out _));
        if (unsupported is not null)
        {
            var name = unsupported.AttributeType.FullName ??
                unsupported.AttributeType.Name;
            return Rejected(
                LoadedSourceMethodRejectionReason
                    .UnsupportedStateMachineMetadata,
                sourceTarget.MethodToken,
                name,
                $"Source '{Display(sourceMethod)}' uses unsupported " +
                $"state-machine metadata '{name}'.");
        }
        if (attributes.Length != 1)
        {
            var names = string.Join(
                ", ",
                attributes.Select(attribute =>
                    attribute.AttributeType.FullName ??
                    attribute.AttributeType.Name));
            return Rejected(
                LoadedSourceMethodRejectionReason
                    .AmbiguousStateMachineMetadata,
                sourceTarget.MethodToken,
                names,
                $"Source '{Display(sourceMethod)}' has {attributes.Length} " +
                $"state-machine markers: {names}.");
        }

        var attribute = attributes[0];
        _ = TryKind(attribute.AttributeType, out var kind);
        if (!TryStateMachineType(attribute, out var stateMachineType))
        {
            var name = attribute.AttributeType.FullName ??
                attribute.AttributeType.Name;
            return Rejected(
                LoadedSourceMethodRejectionReason
                    .UnsupportedStateMachineMetadata,
                sourceTarget.MethodToken,
                name,
                $"Source '{Display(sourceMethod)}' has malformed " +
                $"state-machine metadata '{name}'.");
        }
        if (stateMachineType.ContainsGenericParameters)
        {
            return Rejected(
                LoadedSourceMethodRejectionReason
                    .UnsupportedStateMachineMetadata,
                sourceTarget.MethodToken,
                TypeName(stateMachineType),
                $"Source '{Display(sourceMethod)}' maps to open " +
                $"state-machine type '{TypeName(stateMachineType)}'.");
        }

        var moveNextCandidates = stateMachineType
            .GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly)
            .Where(method => IsMoveNext(method, kind))
            .OrderBy(method => method.MetadataToken)
            .ToArray();
        if (moveNextCandidates.Length == 0)
        {
            return Rejected(
                LoadedSourceMethodRejectionReason.MissingMoveNextBody,
                sourceTarget.MethodToken,
                TypeName(stateMachineType),
                $"State-machine type '{TypeName(stateMachineType)}' for " +
                $"source '{Display(sourceMethod)}' has no exact MoveNext body.");
        }
        if (moveNextCandidates.Length != 1)
        {
            var tokens = string.Join(
                ", ",
                moveNextCandidates.Select(method =>
                    $"0x{method.MetadataToken:X8}"));
            return Rejected(
                LoadedSourceMethodRejectionReason.AmbiguousMoveNextBody,
                sourceTarget.MethodToken,
                TypeName(stateMachineType),
                $"State-machine type '{TypeName(stateMachineType)}' has " +
                $"{moveNextCandidates.Length} MoveNext bodies: {tokens}.");
        }

        var bodyMethod = moveNextCandidates[0];
        if (!TryTarget(bodyMethod, out var bodyTarget, out var bodyFailure))
        {
            return Rejected(
                LoadedSourceMethodRejectionReason
                    .UnsupportedStateMachineMetadata,
                sourceTarget.MethodToken,
                Display(bodyMethod),
                bodyFailure!);
        }

        return ResolveBody(
            sourceMethod,
            sourceTarget,
            bodyMethod,
            bodyTarget,
            kind,
            bodyResolver);
    }

    /// <summary>Resolves the authoritative body and preserves source/body attribution.</summary>
    private static LoadedSourceMethodResolution ResolveBody(
        MethodInfo sourceMethod,
        InterceptionTarget sourceTarget,
        MethodInfo bodyMethod,
        InterceptionTarget bodyTarget,
        LoadedSourceMethodKind kind,
        ILoadedMethodBodySnapshotResolver bodyResolver)
    {
        if (!bodyResolver.TryResolveLoadedBody(bodyTarget, out var body))
        {
            return Rejected(
                LoadedSourceMethodRejectionReason.MissingLoadedBody,
                sourceTarget.MethodToken,
                Display(bodyMethod),
                $"No authoritative loaded body was supplied for " +
                $"'{Display(bodyMethod)}' selected by " +
                $"source '{Display(sourceMethod)}'.");
        }

        return new(
            new(
                sourceTarget,
                bodyTarget,
                body,
                kind,
                Display(sourceMethod),
                Display(bodyMethod)),
            []);
    }

    /// <summary>Creates one exact runtime target or a deterministic unsupported diagnostic.</summary>
    private static bool TryTarget(
        MethodInfo method,
        out InterceptionTarget target,
        out string? failure)
    {
        try
        {
            target = InterceptionTarget.FromMethod(method);
            failure = null;
            return true;
        }
        catch (Exception exception) when (
            exception is NotSupportedException or ArgumentException)
        {
            target = default;
            failure =
                $"Method '{Display(method)}' is not a supported loaded body: " +
                exception.Message;
            return false;
        }
    }

    /// <summary>Creates one immutable rejected result.</summary>
    private static LoadedSourceMethodResolution Rejected(
        LoadedSourceMethodRejectionReason reason,
        int sourceMethodToken,
        string relatedMetadata,
        string detail) =>
        new(
            null,
            [new(reason, sourceMethodToken, relatedMetadata, detail)]);

}
