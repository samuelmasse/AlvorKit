namespace AlvorKit;

/// <summary>Verifies immutable multi-site symbolic caller rewrite composition.</summary>
[TestClass]
public sealed class LoadedSymbolicCallerComposerTest
{
    private static readonly Guid ModuleId =
        new("8ef20d48-426b-49e4-9bf7-a38d2e1c5ac4");
    private const int CallerToken = 0x06000071;

    /// <summary>Composes disjoint sites in baseline order with token-free relocations.</summary>
    [TestMethod]
    public void ComposeMultipleSitesIsDeterministicAcrossInputOrder()
    {
        const int methodToken = 0x0A000071;
        const int fieldToken = 0x04000071;
        const string context = "Caller<int>";
        var rawBody = new LoadedOperationBodyBuilder()
            .EmitToken(0x28, methodToken)
            .EmitToken(0x7E, fieldToken)
            .Emit(0x2A)
            .ToTiny();
        var metadata = new LoadedOperationMetadataFixture()
            .Method(methodToken, Method("static void C::Run()", hasThis: false))
            .Field(fieldToken, Field("static int32 C::Value", isStatic: true));
        var body = LoadedMethodBodyDecoder.Decode(rawBody);
        var sites = Recognize(body, metadata, context).Sites;

        var first = Compose(body, sites, context).Generation!;
        var reversed = Compose(body, sites.Reverse(), context).Generation!;

        Assert.AreEqual(first.Identity, reversed.Identity);
        CollectionAssert.AreEqual(
            new[] { 0, 5 },
            first.Sites.Select(site => site.BaselineOffset).ToArray());
        Assert.AreEqual(20, first.Instructions.Length);
        Assert.AreEqual(10, first.Relocations.Length);
        Assert.AreEqual(
            2,
            first.Relocations.Count(relocation =>
                relocation.Kind ==
                    LoadedSymbolicRelocationKind.ExactOperandLocals));
        Assert.AreEqual(
            2,
            first.Relocations.Count(relocation =>
                relocation.Kind ==
                    LoadedSymbolicRelocationKind.CallSiteSignature));
        Assert.AreEqual(
            2,
            first.Relocations.Count(relocation =>
                relocation.Kind ==
                    LoadedSymbolicRelocationKind.ConstructedMethodHandle));
        Assert.IsTrue(first.Relocations.All(
            relocation =>
                relocation.Symbol.StartsWith("li1-", StringComparison.Ordinal)));
        CollectionAssert.AreEqual(
            Fingerprint(first),
            Fingerprint(reversed));
    }

    /// <summary>Preserves branch, switch, EH, end, and IL-map baseline labels.</summary>
    [TestMethod]
    public void ComposeControlFlowAndExceptionRegionUsesBaselineLabels()
    {
        const int methodToken = 0x0A000072;
        var code = new byte[]
        {
            0x2B, 0x05,
            0x20, 0x00, 0x00, 0x00, 0x00,
            0x28, 0x72, 0x00, 0x00, 0x0A,
            0x45, 0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            0x2A,
            0x2A
        };
        var section = LoadedMethodBodyFixture.SmallCatch(
            tryOffset: 0,
            tryLength: 12,
            handlerOffset: 12,
            handlerLength: 13,
            catchToken: 0x02000003);
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Fat(code, maxStack: 3, section: section));
        var metadata = new LoadedOperationMetadataFixture()
            .Method(methodToken, Method("static void C::Run()", hasThis: false));
        var site = Recognize(body, metadata).Sites.Single();

        var generation = Compose(body, [site]).Generation!;

        var branch = generation.Instructions.Single(instruction =>
            instruction.Kind == LoadedSymbolicInstructionKind.Baseline &&
            instruction.BaselineOffset == 0);
        CollectionAssert.AreEqual(
            new[] { "IL_00000007" },
            branch.TargetLabels.ToArray());
        var @switch = generation.Instructions.Single(instruction =>
            instruction.Kind == LoadedSymbolicInstructionKind.Baseline &&
            instruction.BaselineOffset == 12);
        CollectionAssert.AreEqual(
            new[] { "IL_00000019", "IL_0000001A" },
            @switch.TargetLabels.ToArray());
        var siteMap = generation.IlMap.Single(entry =>
            entry.BaselineOffset == 7);
        Assert.AreEqual(
            LoadedSymbolicInstructionKind.SpillOperands,
            generation.Instructions[siteMap.InstructionIndex].Kind);
        CollectionAssert.Contains(
            generation.Instructions[siteMap.InstructionIndex].Labels.ToArray(),
            "IL_00000007");
        var region = generation.ExceptionRegions.Single();
        Assert.AreEqual("IL_00000000", region.TryStartLabel);
        Assert.AreEqual("IL_0000000C", region.TryEndLabel);
        Assert.AreEqual("IL_0000000C", region.HandlerStartLabel);
        Assert.AreEqual("IL_00000019", region.HandlerEndLabel);
        Assert.AreEqual(0x02000003, region.CatchTypeToken);
        CollectionAssert.AreEquivalent(
            new[] { "IL_END", "IL_0000001B" },
            generation.Instructions[^1].Labels.ToArray());
        Assert.AreEqual((ushort)3, generation.BaselineMaxStack);
        Assert.IsTrue(generation.RequiresMaxStackRecompute);
    }

    /// <summary>Moves accepted prefixes into each site's exact inline-original replay.</summary>
    [TestMethod]
    public void ComposeReplaysAcceptedPrefixesAtOriginalCoordinates()
    {
        const int constrainedType = 0x1B000073;
        const int interfaceCall = 0x0A000073;
        const int fieldToken = 0x04000073;
        var rawBody = new LoadedOperationBodyBuilder()
            .EmitTwoByteToken(0x16, constrainedType)
            .EmitToken(0x6F, interfaceCall)
            .EmitTwoByte(0x13)
            .EmitToken(0x7E, fieldToken)
            .Emit(0x2A)
            .ToTiny();
        var metadata = new LoadedOperationMetadataFixture()
            .Method(
                interfaceCall,
                Method(
                    "instance int32 I::Read()",
                    hasThis: true,
                    shape: LoadedTypeShape.Interface))
            .Type(
                constrainedType,
                new("Metric", LoadedTypeShape.ValueType, isByRefLike: false))
            .Field(fieldToken, Field("static int32 C::Value", isStatic: true));
        var body = LoadedMethodBodyDecoder.Decode(rawBody);
        var sites = Recognize(body, metadata).Sites;

        var generation = Compose(body, sites).Generation!;

        foreach (var site in sites)
        {
            var route = generation.Instructions
                .Where(instruction => instruction.SiteId == site.StableId)
                .ToArray();
            var replayPrefix = Array.FindIndex(
                route,
                instruction =>
                    instruction.Kind ==
                        LoadedSymbolicInstructionKind.ReplayPrefix);
            var replayOriginal = Array.FindIndex(
                route,
                instruction =>
                    instruction.Kind ==
                        LoadedSymbolicInstructionKind.ReplayOriginal);
            Assert.AreEqual(replayPrefix + 1, replayOriginal);
            Assert.AreEqual(
                site.Prefixes.Single().BaselineOffset,
                route[replayPrefix].BaselineOffset);
            Assert.AreEqual(
                site.Prefixes.Single().Kind ==
                    LoadedOperationPrefixKind.Constrained
                        ? (ushort)0xFE16
                        : (ushort)0xFE13,
                route[replayPrefix].OpCodeValue);
            var prefixMap = generation.IlMap.Single(entry =>
                entry.BaselineOffset ==
                    site.Prefixes.Single().BaselineOffset);
            var operationMap = generation.IlMap.Single(entry =>
                entry.BaselineOffset == site.BaselineOffset);
            Assert.AreEqual(
                prefixMap.InstructionIndex,
                operationMap.InstructionIndex);
        }
    }

    /// <summary>Stale body, stale context, and overlapping edits reject without output.</summary>
    [TestMethod]
    public void ComposeRejectsStaleAndOverlappingSitesPristinely()
    {
        const int methodToken = 0x0A000074;
        var metadata = new LoadedOperationMetadataFixture()
            .Method(methodToken, Method("static void C::Run()", hasThis: false));
        var baseline = LoadedMethodBodyDecoder.Decode(
            new LoadedOperationBodyBuilder()
                .EmitToken(0x28, methodToken)
                .Emit(0x2A)
                .ToTiny());
        var changed = LoadedMethodBodyDecoder.Decode(
            new LoadedOperationBodyBuilder()
                .EmitToken(0x28, methodToken)
                .Emit(0x00)
                .Emit(0x2A)
                .ToTiny());
        var site = Recognize(baseline, metadata, "Caller<int>").Sites.Single();

        var staleBody = Compose(changed, [site], "Caller<int>");
        var staleContext = Compose(baseline, [site], "Caller<string>");
        var overlap = Compose(baseline, [site, site], "Caller<int>");

        Assert.IsNull(staleBody.Generation);
        Assert.AreEqual(
            LoadedSymbolicCompositionRejectionReason.StaleBodyIdentity,
            staleBody.Rejections.Single().Reason);
        Assert.IsNull(staleContext.Generation);
        Assert.AreEqual(
            LoadedSymbolicCompositionRejectionReason.StaleSiteIdentity,
            staleContext.Rejections.Single().Reason);
        Assert.IsNull(overlap.Generation);
        Assert.AreEqual(
            LoadedSymbolicCompositionRejectionReason.OverlappingEdit,
            overlap.Rejections.Single().Reason);
    }

    /// <summary>Removing the final site deterministically rebuilds the untouched baseline.</summary>
    [TestMethod]
    public void ComposeWithoutSitesReturnsBaselineSymbolicGeneration()
    {
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Tiny(0x16, 0x2A));

        var generation = Compose(body, []).Generation!;

        Assert.IsFalse(generation.RequiresMaxStackRecompute);
        Assert.IsTrue(generation.Relocations.IsEmpty);
        CollectionAssert.AreEqual(
            new[]
            {
                LoadedSymbolicInstructionKind.Baseline,
                LoadedSymbolicInstructionKind.Baseline,
                LoadedSymbolicInstructionKind.End
            },
            generation.Instructions
                .Select(instruction => instruction.Kind)
                .ToArray());
        CollectionAssert.AreEqual(
            new[] { 0, 1 },
            generation.IlMap.Select(entry => entry.BaselineOffset).ToArray());
    }

    private static LoadedOperationRecognition Recognize(
        LoadedMethodBodySnapshot body,
        LoadedOperationMetadataFixture metadata,
        string context = "") =>
        LoadedOperationRecognizer.Recognize(
            body,
            ModuleId,
            CallerToken,
            metadata,
            context);

    private static LoadedSymbolicComposition Compose(
        LoadedMethodBodySnapshot body,
        IEnumerable<LoadedOperationSiteDescriptor> sites,
        string context = "") =>
        LoadedSymbolicCallerComposer.Compose(
            body,
            ModuleId,
            CallerToken,
            sites,
            context);

    private static LoadedMethodOperand Method(
        string signature,
        bool hasThis,
        LoadedTypeShape shape = LoadedTypeShape.ReferenceType) =>
        new(
            signature,
            hasThis,
            isConstructor: false,
            isVariableArguments: false,
            containsOpenGenericParameters: false,
            shape,
            isByRefLikeReceiver: false);

    private static LoadedFieldOperand Field(
        string signature,
        bool isStatic) =>
        new(
            signature,
            isStatic,
            containsOpenGenericParameters: false,
            LoadedTypeShape.ReferenceType,
            isByRefLikeReceiver: false);

    private static string[] Fingerprint(
        LoadedSymbolicMethodGeneration generation) =>
        [.. generation.Instructions.Select(instruction =>
            $"{instruction.Kind}|" +
            $"{string.Join(",", instruction.Labels)}|" +
            $"{instruction.BaselineOffset}|" +
            $"{instruction.OpCodeValue:X4}|" +
            $"{string.Join(",", instruction.TargetLabels)}|" +
            $"{instruction.SiteId}")];
}
