using System.Diagnostics.CodeAnalysis;

namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Verifies safe loaded-IL constructor splits and pristine deterministic rejections.</summary>
[TestClass]
public sealed class LoadedIlConstructorRemainderTest
{
    /// <summary>The controlled direct-base constructor token.</summary>
    private const int BaseInitializerToken = 0x0A000001;

    /// <summary>The controlled delegating-this constructor token.</summary>
    private const int ThisInitializerToken = 0x0A000002;

    /// <summary>Retains argument evaluation and the base call while moving the exact suffix.</summary>
    [TestMethod]
    public void ConstructorBody_UsesStableInitializerBoundary()
    {
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Tiny(
                0x00,
                0x02,
                0x03,
                0x28, 0x01, 0x00, 0x00, 0x0A,
                0x02,
                0x03,
                0x7D, 0x03, 0x00, 0x00, 0x04,
                0x2A));
        var resolver = Resolver(
            BaseInitializerToken,
            LoadedConstructorInitializerKind.Base,
            "instance System.Void Base::.ctor(System.Int32)");

        var planning = LoadedConstructorRemainderPlanner.Plan(body, resolver);

        Assert.IsTrue(planning.IsSuccessful);
        Assert.IsTrue(planning.Rejections.IsEmpty);
        var plan = planning.Plan!;
        Assert.AreSame(body.Identity, plan.BodyIdentity);
        Assert.AreEqual(
            LoadedConstructorInitializerKind.Base,
            plan.InitializerKind);
        Assert.AreEqual(3, plan.InitializerCallOffset);
        Assert.AreEqual(BaseInitializerToken, plan.InitializerMetadataToken);
        Assert.AreEqual(
            "instance System.Void Base::.ctor(System.Int32)",
            plan.InitializerSignature);
        Assert.AreEqual(0, plan.PreservedPrefix.StartOffset);
        Assert.AreEqual(8, plan.PreservedPrefix.EndOffset);
        CollectionAssert.AreEqual(
            new[] { "nop", "ldarg.0", "ldarg.1", "call" },
            plan.PreservedPrefix.Instructions
                .Select(instruction => instruction.OpCode.Name)
                .ToArray());
        Assert.AreEqual(8, plan.MovedRemainder.StartOffset);
        Assert.AreEqual(body.CodeSize, plan.MovedRemainder.EndOffset);
        CollectionAssert.AreEqual(
            new[] { "ldarg.0", "ldarg.1", "stfld", "ret" },
            plan.MovedRemainder.Instructions
                .Select(instruction => instruction.OpCode.Name)
                .ToArray());
    }

    /// <summary>Retains the exact signature and relation of a delegating this initializer.</summary>
    [TestMethod]
    public void ThisDelegatingConstructor_PreservesInitializerAndRemainder()
    {
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Tiny(
                0x02,
                0x16,
                0x28, 0x02, 0x00, 0x00, 0x0A,
                0x2A));
        var resolver = Resolver(
            ThisInitializerToken,
            LoadedConstructorInitializerKind.This,
            "instance System.Void Current::.ctor(System.Int32)");

        var plan = LoadedConstructorRemainderPlanner.Plan(body, resolver).Plan!;

        Assert.AreEqual(
            LoadedConstructorInitializerKind.This,
            plan.InitializerKind);
        Assert.AreEqual(2, plan.InitializerCallOffset);
        Assert.AreEqual(7, plan.PreservedPrefix.EndOffset);
        Assert.AreEqual(7, plan.MovedRemainder.StartOffset);
        Assert.AreEqual("ret", plan.MovedRemainder.Instructions.Single().OpCode.Name);
    }

    /// <summary>Rejects valid IL whose initializer leaves a value for the moved suffix.</summary>
    [TestMethod]
    public void InitializerBoundary_WithLiveStackValue_RejectsRemainder()
    {
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Tiny(
                0x17,
                0x02,
                0x28, 0x01, 0x00, 0x00, 0x0A,
                0x26,
                0x2A));
        var resolver = Resolver(
            BaseInitializerToken,
            LoadedConstructorInitializerKind.Base,
            "instance System.Void Base::.ctor()");

        var planning = LoadedConstructorRemainderPlanner.Plan(body, resolver);

        Assert.IsNull(planning.Plan);
        var rejection = planning.Rejections.Single(candidate =>
            candidate.Reason ==
                LoadedConstructorRemainderRejectionReason
                    .NonEmptyEvaluationStack);
        StringAssert.Contains(
            rejection.Detail,
            "retains 1 evaluation-stack value");
    }

    /// <summary>Rejects local storage referenced by both retained and moved IL.</summary>
    [TestMethod]
    public void InitializerBoundary_WithCrossingLocal_RejectsRemainder()
    {
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Fat(
                [
                    0x17,
                    0x0A,
                    0x02,
                    0x28, 0x01, 0x00, 0x00, 0x0A,
                    0x06,
                    0x26,
                    0x2A
                ]));
        var resolver = Resolver(
            BaseInitializerToken,
            LoadedConstructorInitializerKind.Base,
            "instance System.Void Base::.ctor()");

        var planning = LoadedConstructorRemainderPlanner.Plan(body, resolver);

        Assert.IsNull(planning.Plan);
        var rejection = planning.Rejections.Single(candidate =>
            candidate.Reason ==
                LoadedConstructorRemainderRejectionReason.CrossBoundaryLocal);
        StringAssert.Contains(rejection.Detail, "Local 0");
    }

    /// <summary>A retained back-edge fails the single-initializer-execution proof.</summary>
    [TestMethod]
    public void InitializerPrefix_WithBackEdge_RejectsRemainder()
    {
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Tiny(
                0x16,
                0x2B, 0xFD,
                0x02,
                0x28, 0x01, 0x00, 0x00, 0x0A,
                0x2A));
        var resolver = Resolver(
            BaseInitializerToken,
            LoadedConstructorInitializerKind.Base,
            "instance System.Void Base::.ctor()");

        var planning = LoadedConstructorRemainderPlanner.Plan(body, resolver);

        Assert.IsNull(planning.Plan);
        Assert.IsTrue(planning.Rejections.Any(candidate =>
            candidate.Reason ==
                LoadedConstructorRemainderRejectionReason
                    .PrefixControlFlowCycle));
    }

    /// <summary>Rejects two classified initializer calls without choosing one by order.</summary>
    [TestMethod]
    public void MultipleInitializers_RejectBeforeActivation()
    {
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Tiny(
                0x02,
                0x28, 0x01, 0x00, 0x00, 0x0A,
                0x02,
                0x28, 0x02, 0x00, 0x00, 0x0A,
                0x2A));
        ConstructorMetadataResolver resolver = new();
        resolver.Add(
            BaseInitializerToken,
            LoadedConstructorInitializerKind.Base,
            "instance System.Void Base::.ctor()");
        resolver.Add(
            ThisInitializerToken,
            LoadedConstructorInitializerKind.This,
            "instance System.Void Current::.ctor()");

        var planning = LoadedConstructorRemainderPlanner.Plan(body, resolver);

        Assert.IsFalse(planning.IsSuccessful);
        Assert.IsNull(planning.Plan);
        var rejection = planning.Rejections.Single();
        Assert.AreEqual(
            LoadedConstructorRemainderRejectionReason.InitializerCount,
            rejection.Reason);
        Assert.AreEqual(
            "Expected exactly one direct-base or delegating-this constructor " +
            "call; found 2 at IL_0001, IL_0007.",
            rejection.Detail);
    }

    /// <summary>Reports both directions of cross-split branches in stable baseline order.</summary>
    [TestMethod]
    public void CrossBoundaryBranch_RejectsBeforeActivation()
    {
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Tiny(
                0x2B, 0x06,
                0x02,
                0x28, 0x01, 0x00, 0x00, 0x0A,
                0x00,
                0x2B, 0xF7,
                0x2A));
        var resolver = Resolver(
            BaseInitializerToken,
            LoadedConstructorInitializerKind.Base,
            "instance System.Void Base::.ctor()");

        var first = LoadedConstructorRemainderPlanner.Plan(body, resolver);
        var second = LoadedConstructorRemainderPlanner.Plan(body, resolver);

        Assert.IsNull(first.Plan);
        CollectionAssert.AreEqual(
            new[] { 0, 9 },
            first.Rejections
                .Select(rejection => rejection.BaselineOffset)
                .ToArray());
        Assert.IsTrue(first.Rejections.All(rejection =>
            rejection.Reason ==
            LoadedConstructorRemainderRejectionReason.CrossBoundaryBranch));
        CollectionAssert.AreEqual(
            first.Rejections.Select(rejection => rejection.Detail).ToArray(),
            second.Rejections.Select(rejection => rejection.Detail).ToArray());
    }

    /// <summary>Rejects an exception clause whose protected range straddles the split.</summary>
    [TestMethod]
    public void CrossBoundaryExceptionRegion_RejectsBeforeActivation()
    {
        var section = LoadedMethodBodyFixture.SmallCatch(
            tryOffset: 0,
            tryLength: 7,
            handlerOffset: 7,
            handlerLength: 1,
            catchToken: 0x02000003);
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Fat(
                [
                    0x02,
                    0x28, 0x01, 0x00, 0x00, 0x0A,
                    0x00,
                    0x2A
                ],
                section: section));
        var resolver = Resolver(
            BaseInitializerToken,
            LoadedConstructorInitializerKind.Base,
            "instance System.Void Base::.ctor()");

        var planning = LoadedConstructorRemainderPlanner.Plan(body, resolver);

        var rejection = planning.Rejections.Single();
        Assert.IsNull(planning.Plan);
        Assert.AreEqual(
            LoadedConstructorRemainderRejectionReason
                .CrossBoundaryExceptionRegion,
            rejection.Reason);
        Assert.AreEqual(6, rejection.BaselineOffset);
        Assert.AreEqual(0, rejection.RelatedOffset);
        Assert.AreEqual(
            "Exception region 0 (Catch) cannot cross constructor initializer " +
            "split IL_0006: try=[IL_0000, IL_0007), " +
            "handler=[IL_0007, IL_0008).",
            rejection.Detail);
    }

    /// <summary>Moves a complete post-split exception clause with the remainder.</summary>
    [TestMethod]
    public void RemainderExceptionRegion_MovesAsCompleteClause()
    {
        var section = LoadedMethodBodyFixture.SmallCatch(
            tryOffset: 6,
            tryLength: 1,
            handlerOffset: 7,
            handlerLength: 1,
            catchToken: 0x02000003);
        var body = LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Fat(
                [
                    0x02,
                    0x28, 0x01, 0x00, 0x00, 0x0A,
                    0x00,
                    0x2A
                ],
                section: section));
        var resolver = Resolver(
            BaseInitializerToken,
            LoadedConstructorInitializerKind.Base,
            "instance System.Void Base::.ctor()");

        var plan = LoadedConstructorRemainderPlanner.Plan(body, resolver).Plan!;

        Assert.IsTrue(plan.PreservedExceptionRegions.IsEmpty);
        Assert.AreEqual(1, plan.MovedExceptionRegions.Length);
        Assert.AreSame(
            body.ExceptionRegions.Single(),
            plan.MovedExceptionRegions.Single());
    }

    /// <summary>Creates one constructor-aware exact metadata resolver.</summary>
    private static ConstructorMetadataResolver Resolver(
        int token,
        LoadedConstructorInitializerKind kind,
        string signature)
    {
        ConstructorMetadataResolver resolver = new();
        resolver.Add(token, kind, signature);
        return resolver;
    }

    /// <summary>Resolves controlled exact constructor metadata for planner tests.</summary>
    private sealed class ConstructorMetadataResolver :
        ILoadedConstructorMetadataResolver
    {
        /// <summary>The exact method metadata keyed by controlled token.</summary>
        private readonly Dictionary<int, LoadedMethodOperand> methods = [];

        /// <summary>The containing-constructor relationship keyed by controlled token.</summary>
        private readonly Dictionary<int, LoadedConstructorInitializerKind>
            kinds = [];

        /// <summary>Adds one exact direct-base or delegating-this constructor token.</summary>
        internal void Add(
            int token,
            LoadedConstructorInitializerKind kind,
            string signature)
        {
            methods.Add(
                token,
                new(
                    signature,
                    hasThis: true,
                    isConstructor: true,
                    isVariableArguments: false,
                    containsOpenGenericParameters: false,
                    LoadedTypeShape.ReferenceType,
                    isByRefLikeReceiver: false,
                    parameterCount:
                        signature.EndsWith(
                            "()",
                            StringComparison.Ordinal)
                            ? 0
                            : 1));
            kinds.Add(token, kind);
        }

        /// <summary>Resolves exact method metadata for a configured token.</summary>
        public bool TryResolveMethod(
            int metadataToken,
            [NotNullWhen(true)]
            out LoadedMethodOperand? method) =>
            methods.TryGetValue(metadataToken, out method);

        /// <summary>Does not resolve fields in constructor-boundary fixtures.</summary>
        public bool TryResolveField(
            int metadataToken,
            [NotNullWhen(true)]
            out LoadedFieldOperand? field)
        {
            _ = metadataToken;
            field = null;
            return false;
        }

        /// <summary>Does not resolve constrained types in constructor-boundary fixtures.</summary>
        public bool TryResolveType(
            int metadataToken,
            [NotNullWhen(true)]
            out LoadedTypeOperand? type)
        {
            _ = metadataToken;
            type = null;
            return false;
        }

        /// <summary>Classifies a configured direct-base or delegating-this token.</summary>
        public bool TryResolveInitializerKind(
            int metadataToken,
            [NotNullWhen(true)]
            out LoadedConstructorInitializerKind? kind)
        {
            if (kinds.TryGetValue(metadataToken, out var value))
            {
                kind = value;
                return true;
            }

            kind = null;
            return false;
        }
    }
}
