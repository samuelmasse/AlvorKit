using System.Reflection;

namespace AlvorKit;

/// <summary>Verifies explicit code-first loaded-caller preparation.</summary>
[TestClass]
public sealed class LoadedInterceptionPreparationPlannerTest
{
    private const int FirstToken = 0x0A000101;
    private const int SecondToken = 0x0A000102;
    private const string Signature = "static int32 Metrics::Read()";

    /// <summary>An unqualified repeated exact signature previews every site and rejects ambiguity.</summary>
    [TestMethod]
    public void PreviewRepeatedSignatureRejectsAmbiguity()
    {
        var body = RepeatedBody();
        var caller = Caller(body);
        var request = Request(caller);

        var preview = LoadedInterceptionPreparationPlanner.Preview(
            request,
            RepeatedMetadata());
        var preparation =
            LoadedInterceptionPreparationPlanner.Prepare(preview);

        Assert.IsFalse(preview.IsSuccessful);
        Assert.AreEqual(2, preview.ResolvedSites.Length);
        Assert.IsTrue(preview.SelectedSites.IsEmpty);
        Assert.IsTrue(preview.RecognitionRejections.IsEmpty);
        Assert.AreEqual(
            LoadedInterceptionPreparationRejectionReason
                .AmbiguousMemberSignature,
            preview.SelectionRejections.Single().Reason);
        Assert.IsFalse(preparation.IsSuccessful);
        Assert.IsNull(preparation.Generation);
        Assert.IsTrue(preparation.CompositionRejections.IsEmpty);
    }

    /// <summary>Stable-site and occurrence selectors each compose only their exact site.</summary>
    [TestMethod]
    public void PreviewExactSelectorsComposeSelectedGeneration()
    {
        var body = RepeatedBody();
        var caller = Caller(body);
        var broad = LoadedInterceptionPreparationPlanner.Preview(
            Request(caller),
            RepeatedMetadata());
        var firstId = broad.ResolvedSites[0].StableId;
        var stablePreview = LoadedInterceptionPreparationPlanner.Preview(
            Request(caller, stableSiteId: firstId),
            RepeatedMetadata());
        var occurrencePreview =
            LoadedInterceptionPreparationPlanner.Preview(
                Request(caller, occurrence: 1),
                RepeatedMetadata());

        var stable = LoadedInterceptionPreparationPlanner.Prepare(
            stablePreview);
        var occurrence = LoadedInterceptionPreparationPlanner.Prepare(
            occurrencePreview);

        Assert.IsTrue(stable.IsSuccessful);
        Assert.AreEqual(
            firstId,
            stable.Generation!.Sites.Single().StableId);
        Assert.IsTrue(occurrence.IsSuccessful);
        Assert.AreEqual(
            broad.ResolvedSites[1].StableId,
            occurrence.Generation!.Sites.Single().StableId);
        Assert.AreEqual(2, occurrence.Preview.ResolvedSites.Length);
        Assert.AreEqual(
            caller.SourceMethod,
            occurrence.Preview.Request.Caller.SourceMethod);
        Assert.AreEqual(
            caller.BodyMethod.MethodToken,
            occurrence.Generation.ContainingMethodToken);
        Assert.AreEqual(
            caller.BodyMethod.ModuleMvid,
            occurrence.Generation.ModuleVersionId);
    }

    /// <summary>A plan for another body identity rejects before recognizing or composing sites.</summary>
    [TestMethod]
    public void PreviewStaleBodyIdentityRejectsPristinely()
    {
        var currentBody = RepeatedBody();
        var staleBody = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Tiny([0x00, 0x2A]));
        var caller = Caller(currentBody);
        var request = new LoadedInterceptionPreparationRequest(
            caller,
            staleBody.Identity,
            Signature);

        var preview = LoadedInterceptionPreparationPlanner.Preview(
            request,
            RepeatedMetadata());
        var preparation =
            LoadedInterceptionPreparationPlanner.Prepare(preview);

        Assert.IsFalse(preview.IsSuccessful);
        Assert.IsTrue(preview.ResolvedSites.IsEmpty);
        Assert.IsTrue(preview.SelectedSites.IsEmpty);
        Assert.IsTrue(preview.RecognitionRejections.IsEmpty);
        Assert.AreEqual(
            LoadedInterceptionPreparationRejectionReason.StaleBodyIdentity,
            preview.SelectionRejections.Single().Reason);
        StringAssert.Contains(
            preview.SelectionRejections.Single().Detail,
            staleBody.Identity.Value);
        Assert.IsNull(preparation.Generation);
        Assert.IsTrue(preparation.CompositionRejections.IsEmpty);
    }

    /// <summary>A semantic recognition failure publishes no partial sites or generation.</summary>
    [TestMethod]
    public void PreviewRecognitionFailureRemainsPristine()
    {
        var body = LoadedMethodBodyDecoder.Decode(
            new LoadedOperationBodyBuilder()
                .EmitToken(0x28, FirstToken)
                .EmitTwoByte(0x14)
                .EmitToken(0x28, SecondToken)
                .Emit(0x2A)
                .ToTiny());
        var caller = Caller(body);

        var preview = LoadedInterceptionPreparationPlanner.Preview(
            Request(caller),
            RepeatedMetadata());
        var preparation =
            LoadedInterceptionPreparationPlanner.Prepare(preview);

        Assert.IsFalse(preview.IsSuccessful);
        Assert.IsTrue(preview.ResolvedSites.IsEmpty);
        Assert.IsTrue(preview.SelectedSites.IsEmpty);
        Assert.IsTrue(preview.SelectionRejections.IsEmpty);
        Assert.AreEqual(
            LoadedOperationRejectionReason.UnsupportedPrefix,
            preview.RecognitionRejections.Single().Reason);
        Assert.IsNull(preparation.Generation);
        Assert.IsTrue(preparation.CompositionRejections.IsEmpty);
    }

    /// <summary>Creates a request for the repeated exact operation signature.</summary>
    private static LoadedInterceptionPreparationRequest Request(
        LoadedSourceMethodTarget caller,
        string? stableSiteId = null,
        int? occurrence = null) =>
        new(
            caller,
            caller.BodyIdentity,
            Signature,
            stableSiteId: stableSiteId,
            occurrence: occurrence);

    /// <summary>Creates an authoritative body with two matching exact operations.</summary>
    private static LoadedMethodBodySnapshot RepeatedBody() =>
        LoadedMethodBodyDecoder.Decode(
            new LoadedOperationBodyBuilder()
                .EmitToken(0x28, FirstToken)
                .EmitToken(0x28, SecondToken)
                .Emit(0x2A)
                .ToTiny());

    /// <summary>Creates exact metadata for two sites sharing one member signature.</summary>
    private static LoadedOperationMetadataFixture RepeatedMetadata() =>
        new LoadedOperationMetadataFixture()
            .Method(FirstToken, Method())
            .Method(SecondToken, Method());

    /// <summary>Creates one exact resolved synchronous source/body target.</summary>
    private static LoadedSourceMethodTarget Caller(
        LoadedMethodBodySnapshot body)
    {
        var source = typeof(LoadedInterceptionPreparationPlannerTest)
            .GetMethod(
                nameof(CallerSource),
                BindingFlags.Static | BindingFlags.NonPublic)!;
        var resolver = new SingleBodyResolver(
            InterceptionTarget.FromMethod(source),
            body);
        return LoadedSourceMethodResolver.Resolve(source, resolver).Target!;
    }

    /// <summary>Creates the exact static method operand fixture.</summary>
    private static LoadedMethodOperand Method() =>
        new(
            Signature,
            hasThis: false,
            isConstructor: false,
            isVariableArguments: false,
            containsOpenGenericParameters: false,
            declaringTypeShape: LoadedTypeShape.ReferenceType,
            isByRefLikeReceiver: false);

    /// <summary>Provides one real source MethodDef for exact code-first identity.</summary>
    private static int CallerSource() => 0;

    /// <summary>Resolves one authoritative loaded body by exact runtime identity.</summary>
    private sealed class SingleBodyResolver(
        InterceptionTarget target,
        LoadedMethodBodySnapshot snapshot) :
        ILoadedMethodBodySnapshotResolver
    {
        /// <summary>The one exact runtime method identity.</summary>
        private readonly InterceptionTarget target = target;

        /// <summary>The one authoritative loaded-body snapshot.</summary>
        private readonly LoadedMethodBodySnapshot snapshot = snapshot;

        /// <summary>Resolves the controlled body only for its exact identity.</summary>
        public bool TryResolveLoadedBody(
            InterceptionTarget method,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out LoadedMethodBodySnapshot? body)
        {
            body = method == target ? snapshot : null;
            return body is not null;
        }
    }
}
