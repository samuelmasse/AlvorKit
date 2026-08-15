using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Previews and composes one explicit loaded-caller interception selection.
/// </summary>
public static class LoadedInterceptionPreparationPlanner
{
    /// <summary>
    /// Recognizes every supported site and selects one exact requested operation.
    /// </summary>
    public static LoadedInterceptionPreparationPreview Preview(
        LoadedInterceptionPreparationRequest request,
        ILoadedOperationMetadataResolver metadataResolver)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(metadataResolver);

        if (!request.ExpectedBodyIdentity.Equals(
                request.Caller.BodyIdentity))
        {
            return Rejected(
                request,
                LoadedInterceptionPreparationRejectionReason
                    .StaleBodyIdentity,
                $"Caller body '{request.Caller.BodyAttribution}' has " +
                $"identity '{request.Caller.BodyIdentity}', but the plan " +
                $"expects '{request.ExpectedBodyIdentity}'.");
        }

        var recognition = LoadedOperationRecognizer.Recognize(
            request.Caller.Body,
            request.Caller.BodyMethod.ModuleMvid,
            request.Caller.BodyMethod.MethodToken,
            metadataResolver,
            request.ConstructedContext);
        if (!recognition.IsSuccessful)
        {
            return new(
                request,
                [],
                [],
                recognition.Rejections,
                []);
        }

        var matches = recognition.Sites
            .Where(site => StringComparer.Ordinal.Equals(
                site.CanonicalSignature,
                request.MemberSignature))
            .OrderBy(site => site.BaselineOffset)
            .ThenBy(site => site.StableId, StringComparer.Ordinal)
            .ToImmutableArray();
        var rejection = Select(request, matches, out var selected);
        return new(
            request,
            recognition.Sites,
            selected,
            [],
            rejection is null ? [] : [rejection]);
    }

    /// <summary>
    /// Composes only a preview whose recognition and exact selection completed.
    /// </summary>
    public static LoadedInterceptionPreparationResult Prepare(
        LoadedInterceptionPreparationPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);
        if (!preview.IsSuccessful)
            return new(preview, null, []);

        var request = preview.Request;
        var composition = LoadedSymbolicCallerComposer.Compose(
            request.Caller.Body,
            request.Caller.BodyMethod.ModuleMvid,
            request.Caller.BodyMethod.MethodToken,
            preview.SelectedSites,
            request.ConstructedContext);
        return new(
            preview,
            composition.Generation,
            composition.Rejections);
    }

    /// <summary>Selects one exact site or creates one deterministic rejection.</summary>
    private static LoadedInterceptionPreparationRejection? Select(
        LoadedInterceptionPreparationRequest request,
        ImmutableArray<LoadedOperationSiteDescriptor> matches,
        out ImmutableArray<LoadedOperationSiteDescriptor> selected)
    {
        selected = [];
        if (request.StableSiteId is not null &&
            request.Occurrence is not null)
        {
            return Reject(
                request,
                LoadedInterceptionPreparationRejectionReason
                    .ConflictingSiteSelector,
                "Specify either an exact stable site or a zero-based " +
                "occurrence, not both.");
        }
        if (request.StableSiteId is not null)
        {
            var match = matches.FirstOrDefault(site =>
                StringComparer.Ordinal.Equals(
                    site.StableId,
                    request.StableSiteId));
            if (match is null)
            {
                return Reject(
                    request,
                    LoadedInterceptionPreparationRejectionReason
                        .StableSiteNotFound,
                    $"Stable site '{request.StableSiteId}' does not match " +
                    $"member signature '{request.MemberSignature}'.");
            }

            selected = [match];
            return null;
        }
        if (request.Occurrence is int occurrence)
        {
            if (occurrence < 0 || occurrence >= matches.Length)
            {
                return Reject(
                    request,
                    LoadedInterceptionPreparationRejectionReason
                        .OccurrenceOutOfRange,
                    $"Occurrence {occurrence} is outside the {matches.Length} " +
                    $"site(s) matching '{request.MemberSignature}'.");
            }

            selected = [matches[occurrence]];
            return null;
        }
        if (matches.IsEmpty)
        {
            return Reject(
                request,
                LoadedInterceptionPreparationRejectionReason
                    .MemberSignatureNotFound,
                $"No recognized site matches exact member signature " +
                $"'{request.MemberSignature}'.");
        }
        if (matches.Length != 1)
        {
            return Reject(
                request,
                LoadedInterceptionPreparationRejectionReason
                    .AmbiguousMemberSignature,
                $"Exact member signature '{request.MemberSignature}' " +
                $"matches {matches.Length} sites; specify a stable site or " +
                "zero-based occurrence.");
        }

        selected = matches;
        return null;
    }

    /// <summary>Creates a preview rejected before operation recognition.</summary>
    private static LoadedInterceptionPreparationPreview Rejected(
        LoadedInterceptionPreparationRequest request,
        LoadedInterceptionPreparationRejectionReason reason,
        string detail) =>
        new(request, [], [], [], [Reject(request, reason, detail)]);

    /// <summary>Creates one request-attributed selection rejection.</summary>
    private static LoadedInterceptionPreparationRejection Reject(
        LoadedInterceptionPreparationRequest request,
        LoadedInterceptionPreparationRejectionReason reason,
        string detail) =>
        new(
            reason,
            request.MemberSignature,
            request.StableSiteId,
            request.Occurrence,
            detail);
}
