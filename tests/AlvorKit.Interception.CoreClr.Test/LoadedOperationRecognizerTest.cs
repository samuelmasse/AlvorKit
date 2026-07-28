namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Verifies Cecil-free semantic operation recognition over loaded raw IL.</summary>
[TestClass]
public sealed class LoadedOperationRecognizerTest
{
    private static readonly Guid ModuleId =
        new("a6ab02a4-22c1-41d4-8b99-6f3a02cd40a1");
    private const int CallerToken = 0x06000011;

    /// <summary>Recognizes all supported method, construction, and field opcode shapes.</summary>
    [TestMethod]
    public void RecognizeReturnsExactSupportedOperationKinds()
    {
        const int instanceCall = 0x0A000001;
        const int virtualCall = 0x0A000002;
        const int staticCall = 0x0A000003;
        const int constructor = 0x0A000004;
        const int instanceRead = 0x04000001;
        const int instanceWrite = 0x04000002;
        const int staticRead = 0x04000003;
        const int staticWrite = 0x04000004;
        var body = new LoadedOperationBodyBuilder()
            .EmitToken(0x28, instanceCall)
            .EmitToken(0x6F, virtualCall)
            .EmitToken(0x28, staticCall)
            .EmitToken(0x73, constructor)
            .EmitToken(0x7B, instanceRead)
            .EmitToken(0x7D, instanceWrite)
            .EmitToken(0x7E, staticRead)
            .EmitToken(0x80, staticWrite)
            .Emit(0x2A)
            .ToTiny();
        var metadata = new LoadedOperationMetadataFixture()
            .Method(instanceCall, Method("instance void C::M()", hasThis: true))
            .Method(
                virtualCall,
                Method(
                    "instance int32 I::Get()",
                    hasThis: true,
                    shape: LoadedTypeShape.Interface))
            .Method(staticCall, Method("static void C::S()", hasThis: false))
            .Method(
                constructor,
                Method(
                    "instance void C::.ctor()",
                    hasThis: true,
                    constructor: true))
            .Field(instanceRead, Field("instance int32 C::A", isStatic: false))
            .Field(instanceWrite, Field("instance int32 C::B", isStatic: false))
            .Field(staticRead, Field("static int32 C::C", isStatic: true))
            .Field(staticWrite, Field("static int32 C::D", isStatic: true));

        var result = Recognize(body, metadata);

        Assert.IsTrue(result.IsSuccessful);
        CollectionAssert.AreEqual(
            new[]
            {
                LoadedOperationKind.InstanceCall,
                LoadedOperationKind.InstanceCall,
                LoadedOperationKind.StaticCall,
                LoadedOperationKind.ObjectConstruction,
                LoadedOperationKind.InstanceFieldRead,
                LoadedOperationKind.InstanceFieldWrite,
                LoadedOperationKind.StaticFieldRead,
                LoadedOperationKind.StaticFieldWrite
            },
            result.Sites.Select(site => site.Kind).ToArray());
        CollectionAssert.AreEqual(
            new ushort[] { 0x28, 0x6F, 0x28, 0x73, 0x7B, 0x7D, 0x7E, 0x80 },
            result.Sites.Select(site => site.OpCodeValue).ToArray());
        CollectionAssert.AreEqual(
            new[] { 0, 5, 10, 15, 20, 25, 30, 35 },
            result.Sites.Select(site => site.BaselineOffset).ToArray());
        Assert.IsTrue(result.Sites.All(
            site => site.BodyIdentity.Equals(
                LoadedMethodBodyDecoder.Decode(body).Identity)));
    }

    /// <summary>Retains constrained and volatile prefix sequences in original order.</summary>
    [TestMethod]
    public void RecognizeRetainsAcceptedPrefixDescriptors()
    {
        const int constrainedType = 0x1B000001;
        const int interfaceCall = 0x0A000011;
        const int field = 0x04000011;
        var body = new LoadedOperationBodyBuilder()
            .EmitTwoByteToken(0x16, constrainedType)
            .EmitToken(0x6F, interfaceCall)
            .EmitTwoByte(0x13)
            .EmitToken(0x7B, field)
            .Emit(0x2A)
            .ToTiny();
        var metadata = new LoadedOperationMetadataFixture()
            .Method(
                interfaceCall,
                Method(
                    "instance int32 IMetric::Read()",
                    hasThis: true,
                    shape: LoadedTypeShape.Interface))
            .Type(
                constrainedType,
                new("Metric", LoadedTypeShape.ValueType, isByRefLike: false))
            .Field(field, Field("instance int32 Box::Value", isStatic: false));

        var result = Recognize(body, metadata);

        Assert.IsTrue(result.IsSuccessful);
        Assert.AreEqual(LoadedOperationKind.StructMethod, result.Sites[0].Kind);
        Assert.AreEqual(
            LoadedOperationPrefixKind.Constrained,
            result.Sites[0].Prefixes.Single().Kind);
        Assert.AreEqual(
            constrainedType,
            result.Sites[0].Prefixes[0].MetadataToken);
        Assert.AreEqual("Metric", result.Sites[0].Prefixes[0].OperandSignature);
        Assert.AreEqual(
            LoadedOperationPrefixKind.Volatile,
            result.Sites[1].Prefixes.Single().Kind);
        Assert.AreEqual(11, result.Sites[1].Prefixes[0].BaselineOffset);
    }

    /// <summary>Produces stable IDs from body, context, coordinate, opcode, and signature.</summary>
    [TestMethod]
    public void RecognizeStableIdentityChangesWithConstructedContext()
    {
        const int methodToken = 0x0A000021;
        var body = new LoadedOperationBodyBuilder()
            .EmitToken(0x28, methodToken)
            .Emit(0x2A)
            .ToTiny();
        var metadata = new LoadedOperationMetadataFixture()
            .Method(methodToken, Method("static !0 C::Read<!0>()", hasThis: false));

        var first = Recognize(body, metadata, "C<int>::Read<int>").Sites.Single();
        var repeated = Recognize(body, metadata, "C<int>::Read<int>").Sites.Single();
        var other = Recognize(body, metadata, "C<string>::Read<string>").Sites.Single();

        Assert.AreEqual(first.StableId, repeated.StableId);
        Assert.AreNotEqual(first.StableId, other.StableId);
        Assert.AreEqual(ModuleId, first.ModuleVersionId);
        Assert.AreEqual(CallerToken, first.ContainingMethodToken);
        StringAssert.StartsWith(first.StableId, "li1-");
    }

    /// <summary>An unsupported prefix rejects the whole body without partial supported sites.</summary>
    [TestMethod]
    public void RecognizeUnsupportedPrefixReturnsPristineStructuredRejection()
    {
        const int firstMethod = 0x0A000031;
        const int prefixedMethod = 0x0A000032;
        var body = new LoadedOperationBodyBuilder()
            .EmitToken(0x28, firstMethod)
            .EmitTwoByte(0x14)
            .EmitToken(0x28, prefixedMethod)
            .Emit(0x2A)
            .ToTiny();
        var metadata = new LoadedOperationMetadataFixture()
            .Method(firstMethod, Method("static void C::First()", hasThis: false))
            .Method(prefixedMethod, Method("static void C::Second()", hasThis: false));

        var result = Recognize(body, metadata);

        Assert.IsFalse(result.IsSuccessful);
        Assert.IsTrue(result.Sites.IsEmpty);
        var rejection = result.Rejections.Single();
        Assert.AreEqual(
            LoadedOperationRejectionReason.UnsupportedPrefix,
            rejection.Reason);
        Assert.AreEqual(7, rejection.BaselineOffset);
        Assert.AreEqual(5, rejection.RelatedOffset);
        Assert.AreEqual((ushort)0xFE14, rejection.OpCodeValue);
        StringAssert.Contains(rejection.Detail, "tail.");
    }

    /// <summary>Variable-argument and open signatures return ordered exact rejection categories.</summary>
    [TestMethod]
    public void RecognizeUnsupportedSignaturesReturnsStructuredRejections()
    {
        const int varArgMethod = 0x0A000041;
        const int openField = 0x04000041;
        var body = new LoadedOperationBodyBuilder()
            .EmitToken(0x28, varArgMethod)
            .EmitToken(0x7E, openField)
            .Emit(0x2A)
            .ToTiny();
        var metadata = new LoadedOperationMetadataFixture()
            .Method(
                varArgMethod,
                Method(
                    "static vararg void C::Write(...)",
                    hasThis: false,
                    variableArguments: true))
            .Field(
                openField,
                Field(
                    "static !0 C<!0>::Value",
                    isStatic: true,
                    open: true));

        var result = Recognize(body, metadata);

        Assert.IsFalse(result.IsSuccessful);
        Assert.IsTrue(result.Sites.IsEmpty);
        CollectionAssert.AreEqual(
            new[]
            {
                LoadedOperationRejectionReason.VariableArguments,
                LoadedOperationRejectionReason.OpenGenericSignature
            },
            result.Rejections.Select(rejection => rejection.Reason).ToArray());
        CollectionAssert.AreEqual(
            new[] { 0, 5 },
            result.Rejections
                .Select(rejection => rejection.BaselineOffset)
                .ToArray());
    }

    /// <summary>Duplicate accepted prefixes are rejected at the duplicate coordinate.</summary>
    [TestMethod]
    public void RecognizeDuplicateVolatilePrefixReturnsDuplicateRejection()
    {
        const int fieldToken = 0x04000051;
        var body = new LoadedOperationBodyBuilder()
            .EmitTwoByte(0x13)
            .EmitTwoByte(0x13)
            .EmitToken(0x7E, fieldToken)
            .Emit(0x2A)
            .ToTiny();
        var metadata = new LoadedOperationMetadataFixture()
            .Field(fieldToken, Field("static int32 C::Value", isStatic: true));

        var rejection = Recognize(body, metadata).Rejections.Single();

        Assert.AreEqual(
            LoadedOperationRejectionReason.DuplicatePrefix,
            rejection.Reason);
        Assert.AreEqual(4, rejection.BaselineOffset);
        Assert.AreEqual(2, rejection.RelatedOffset);
    }

    /// <summary>A constrained ref-like receiver is rejected before any site is published.</summary>
    [TestMethod]
    public void RecognizeRefLikeConstrainedReceiverReturnsSignatureRejection()
    {
        const int constrainedType = 0x1B000061;
        const int interfaceCall = 0x0A000061;
        var body = new LoadedOperationBodyBuilder()
            .EmitTwoByteToken(0x16, constrainedType)
            .EmitToken(0x6F, interfaceCall)
            .Emit(0x2A)
            .ToTiny();
        var metadata = new LoadedOperationMetadataFixture()
            .Method(
                interfaceCall,
                Method(
                    "instance void I::Run()",
                    hasThis: true,
                    shape: LoadedTypeShape.Interface))
            .Type(
                constrainedType,
                new("SpanLike", LoadedTypeShape.ValueType, isByRefLike: true));

        var rejection = Recognize(body, metadata).Rejections.Single();

        Assert.AreEqual(
            LoadedOperationRejectionReason.RefLikeReceiver,
            rejection.Reason);
        Assert.AreEqual(interfaceCall, rejection.MetadataToken);
        StringAssert.Contains(rejection.Detail, "ref-like");
    }

    private static LoadedOperationRecognition Recognize(
        byte[] body,
        LoadedOperationMetadataFixture metadata,
        string context = "") =>
        LoadedOperationRecognizer.Recognize(
            LoadedMethodBodyDecoder.Decode(body),
            ModuleId,
            CallerToken,
            metadata,
            context);

    private static LoadedMethodOperand Method(
        string signature,
        bool hasThis,
        bool constructor = false,
        bool variableArguments = false,
        bool open = false,
        LoadedTypeShape shape = LoadedTypeShape.ReferenceType,
        bool byRefLike = false) =>
        new(
            signature,
            hasThis,
            constructor,
            variableArguments,
            open,
            shape,
            byRefLike);

    private static LoadedFieldOperand Field(
        string signature,
        bool isStatic,
        bool open = false,
        LoadedTypeShape shape = LoadedTypeShape.ReferenceType,
        bool byRefLike = false) =>
        new(signature, isStatic, open, shape, byRefLike);
}
