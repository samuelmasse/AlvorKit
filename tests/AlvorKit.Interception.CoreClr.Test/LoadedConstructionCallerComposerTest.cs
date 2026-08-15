using System.Buffers.Binary;
using System.Reflection;
using System.Reflection.Emit;

namespace AlvorKit;

/// <summary>Verifies exact one-site newobj lowering over complete loaded caller bodies.</summary>
[TestClass]
public sealed class LoadedConstructionCallerComposerTest
{
    /// <summary>Changes only one selected newobj while preserving locals, branches, and EH bytes.</summary>
    [TestMethod]
    public void ComposeRewritesOnlySelectedConstructionInCompleteBody()
    {
        MethodInfo caller = Method(nameof(ComplexCaller));
        ConstructorInfo constructor = Constructor(typeof(ConstructionTarget));
        LoadedMethodBodySnapshot body = Body(caller);
        LoadedOperationRecognition recognition = Recognize(caller, body);
        LoadedOperationSiteDescriptor[] constructions =
        [
            .. recognition.Sites.Where(site =>
                site.Kind == LoadedOperationKind.ObjectConstruction)
        ];
        Assert.IsTrue(body.InitLocals);
        Assert.AreNotEqual(0, body.LocalSignatureToken);
        Assert.IsFalse(body.ExceptionRegions.IsEmpty);
        Assert.IsTrue(body.Instructions.Any(instruction =>
            !instruction.Operand.BranchTargets.IsEmpty));
        Assert.IsTrue(constructions.Length >= 3);
        LoadedOperationSiteDescriptor selected = constructions[1];
        MethodInfo route = Method(nameof(Route));

        InterceptionGenerationPlan generation =
            LoadedConstructionCallerComposer.Compose(
                caller,
                body,
                selected,
                constructor,
                route,
                17,
                16);

        Assert.AreEqual(
            caller.MetadataToken,
            generation.Target.MethodToken);
        Assert.AreSame(body.Identity, generation.BaselineBodyIdentity);
        Assert.AreEqual(17ul, generation.GenerationId);
        Assert.AreEqual(16ul, generation.PriorGenerationId);
        Assert.IsTrue(generation.Relocations.IsEmpty);
        CollectionAssert.AreEqual(
            (uint[])
            [
                .. body.Instructions
                .Select(instruction =>
                    ((uint)instruction.BaselineOffset))
            ],
            (uint[])[
                .. generation.IlMap.Select(entry => entry.OldOffset)
            ]);
        Assert.IsTrue(generation.IlMap.All(entry =>
            entry.Accurate && entry.OldOffset == entry.NewOffset));

        byte[] original = [.. body.Bytes];
        byte[] rewritten = [.. generation.MethodBody.Bytes.Span];
        int absolute = body.HeaderSize + selected.BaselineOffset;
        for (int index = 0; index < original.Length; ++index)
        {
            if (index < absolute || index >= absolute + 5)
                Assert.AreEqual(original[index], rewritten[index]);
        }
        Assert.AreEqual(0x73, original[absolute]);
        Assert.AreEqual(0x28, rewritten[absolute]);
        Assert.AreEqual(
            route.MetadataToken,
            BinaryPrimitives.ReadInt32LittleEndian(
                rewritten.AsSpan(absolute + 1)));

        LoadedMethodBodySnapshot decoded =
            LoadedMethodBodyDecoder.Decode(rewritten);
        Assert.AreEqual(body.LocalSignatureToken, decoded.LocalSignatureToken);
        Assert.AreEqual(body.InitLocals, decoded.InitLocals);
        Assert.AreEqual(
            body.ExceptionRegions.Length,
            decoded.ExceptionRegions.Length);
        CollectionAssert.AreEqual(
            (int[])
            [
                .. constructions
                .Where(site => site.BaselineOffset != selected.BaselineOffset)
                .Select(site => site.BaselineOffset)
            ],
            (int[])
            [
                .. decoded.Instructions.Where(instruction =>
                    instruction.OpCodeValue == 0x73 &&
                    instruction.Operand.IntegerValue ==
                        constructor.MetadataToken)
                .Select(instruction => instruction.BaselineOffset)
            ]);
    }

    /// <summary>A site from a different loaded identity is rejected before generation.</summary>
    [TestMethod]
    public void ComposeRejectsStaleBodyIdentity()
    {
        MethodInfo caller = Method(nameof(ComplexCaller));
        ConstructorInfo constructor = Constructor(typeof(ConstructionTarget));
        LoadedMethodBodySnapshot body = Body(caller);
        LoadedOperationSiteDescriptor site = Recognize(caller, body)
            .Sites
            .First(candidate =>
                candidate.Kind ==
                    LoadedOperationKind.ObjectConstruction);
        byte[] staleBytes = [.. body.Bytes];
        staleBytes[2] = ((byte)(staleBytes[2] + 1));
        LoadedMethodBodySnapshot stale =
            LoadedMethodBodyDecoder.Decode(staleBytes);

        ArgumentException exception =
            Assert.ThrowsExactly<ArgumentException>(() =>
                LoadedConstructionCallerComposer.Compose(
                    caller,
                    stale,
                    site,
                    constructor,
                    Method(nameof(Route)),
                    1));

        Assert.AreEqual("site", exception.ParamName);
        StringAssert.Contains(exception.Message, "loaded body");
    }

    /// <summary>Caller coordinates and constructed context must identify one exact site.</summary>
    [TestMethod]
    public void ComposeRejectsNonUniqueSiteIdentityCoordinates()
    {
        MethodInfo caller = Method(nameof(ComplexCaller));
        LoadedMethodBodySnapshot body = Body(caller);
        var resolver =
            new ReflectionLoadedOperationMetadataResolver(caller);
        LoadedOperationSiteDescriptor wrongCaller =
            LoadedOperationRecognizer.Recognize(
                body,
                caller.Module.ModuleVersionId,
                caller.MetadataToken + 1,
                resolver,
                resolver.ConstructedContext)
            .Sites
            .First(site =>
                site.Kind == LoadedOperationKind.ObjectConstruction);
        LoadedOperationSiteDescriptor wrongContext =
            LoadedOperationRecognizer.Recognize(
                body,
                caller.Module.ModuleVersionId,
                caller.MetadataToken,
                resolver,
                "forged-context")
            .Sites
            .First(site =>
                site.Kind == LoadedOperationKind.ObjectConstruction);

        Assert.ThrowsExactly<ArgumentException>(() =>
            LoadedConstructionCallerComposer.Compose(
                caller,
                body,
                wrongCaller,
                Constructor(typeof(ConstructionTarget)),
                Method(nameof(Route)),
                1));
        Assert.ThrowsExactly<ArgumentException>(() =>
            LoadedConstructionCallerComposer.Compose(
                caller,
                body,
                wrongContext,
                Constructor(typeof(ConstructionTarget)),
                Method(nameof(Route)),
                1));
    }

    /// <summary>The selected opcode and token must name the supplied constructor.</summary>
    [TestMethod]
    public void ComposeRejectsWrongOperationKindOrConstructor()
    {
        MethodInfo caller = Method(nameof(MixedCaller));
        LoadedMethodBodySnapshot body = Body(caller);
        LoadedOperationRecognition recognition = Recognize(caller, body);
        LoadedOperationSiteDescriptor callSite = recognition.Sites
            .Single(site => site.Kind == LoadedOperationKind.StaticCall);
        LoadedOperationSiteDescriptor construction = recognition.Sites
            .Single(site =>
                site.Kind == LoadedOperationKind.ObjectConstruction);

        Assert.ThrowsExactly<ArgumentException>(() =>
            LoadedConstructionCallerComposer.Compose(
                caller,
                body,
                callSite,
                Constructor(typeof(ConstructionTarget)),
                Method(nameof(Route)),
                1));
        Assert.ThrowsExactly<ArgumentException>(() =>
            LoadedConstructionCallerComposer.Compose(
                caller,
                body,
                construction,
                Constructor(typeof(OtherConstructionTarget)),
                Method(nameof(Route)),
                1));
    }

    /// <summary>The route must be an exact same-module nongeneric static MethodDef.</summary>
    [TestMethod]
    public void ComposeRejectsInexactOrGenericRoutes()
    {
        MethodInfo caller = Method(nameof(ComplexCaller));
        LoadedMethodBodySnapshot body = Body(caller);
        LoadedOperationSiteDescriptor site = Recognize(caller, body)
            .Sites
            .First(candidate =>
                candidate.Kind ==
                    LoadedOperationKind.ObjectConstruction);
        ConstructorInfo constructor = Constructor(typeof(ConstructionTarget));
        MethodInfo generic = Method(nameof(GenericRoute))
            .MakeGenericMethod(typeof(int));
        var dynamicRoute = new DynamicMethod(
            "DynamicConstructionRoute",
            typeof(ConstructionTarget),
            [typeof(int)],
            caller.Module,
            true);

        Assert.ThrowsExactly<ArgumentException>(() =>
            LoadedConstructionCallerComposer.Compose(
                caller,
                body,
                site,
                constructor,
                Method(nameof(WrongRoute)),
                1));
        Assert.ThrowsExactly<ArgumentException>(() =>
            LoadedConstructionCallerComposer.Compose(
                caller,
                body,
                site,
                constructor,
                generic,
                1));
        Assert.ThrowsExactly<ArgumentException>(() =>
            LoadedConstructionCallerComposer.Compose(
                caller,
                body,
                site,
                constructor,
                dynamicRoute,
                1));
    }

    /// <summary>Generic caller MethodDefs reject construction-specific route signatures.</summary>
    [TestMethod]
    public void ComposeRejectsGenericCallerDefinitions()
    {
        MethodInfo genericMethod = Method(nameof(GenericCaller))
            .MakeGenericMethod(typeof(int));
        MethodInfo genericTypeMethod =
            typeof(GenericCallerHost<int>).GetMethod(
                nameof(GenericCallerHost<>.Call),
                BindingFlags.NonPublic | BindingFlags.Static)!;

        foreach (MethodInfo caller in
            new MethodInfo[] { genericMethod, genericTypeMethod })
        {
            LoadedMethodBodySnapshot body = Body(caller);
            LoadedOperationSiteDescriptor site = Recognize(caller, body)
                .Sites
                .Single(candidate =>
                    candidate.Kind ==
                        LoadedOperationKind.ObjectConstruction);

            Assert.ThrowsExactly<NotSupportedException>(() =>
                LoadedConstructionCallerComposer.Compose(
                    caller,
                    body,
                    site,
                    Constructor(typeof(ConstructionTarget)),
                    Method(nameof(Route)),
                    1));
        }
    }

    /// <summary>A type initializer is not an allocatable newobj operation.</summary>
    [TestMethod]
    public void OriginalConstructionRejectsStaticConstructor()
    {
        ConstructorInfo constructor =
            typeof(StaticConstructionTarget).TypeInitializer!;

        Assert.ThrowsExactly<NotSupportedException>(() =>
            LoadedConstructionOriginalDelegate.Create<Func<object>>(
                constructor));
    }

    private static LoadedMethodBodySnapshot Body(MethodInfo caller) =>
        LoadedMethodBodyDecoder.Decode(
            ReflectionLoadedBodyFixture.Read(caller));

    private static LoadedOperationRecognition Recognize(
        MethodInfo caller,
        LoadedMethodBodySnapshot body)
    {
        var resolver =
            new ReflectionLoadedOperationMetadataResolver(caller);
        LoadedOperationRecognition recognition =
            LoadedOperationRecognizer.Recognize(
                body,
                caller.Module.ModuleVersionId,
                caller.MetadataToken,
                resolver,
                resolver.ConstructedContext);
        Assert.IsTrue(
            recognition.IsSuccessful,
            string.Join(
                Environment.NewLine,
                recognition.Rejections.Select(rejection =>
                    rejection.Detail)));
        return recognition;
    }

    private static MethodInfo Method(string name) =>
        typeof(LoadedConstructionCallerComposerTest).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static ConstructorInfo Constructor(Type type) =>
        type.GetConstructor(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic,
            null,
            [typeof(int)],
            null)!;

    private static ConstructionTarget ComplexCaller(int value)
    {
        try
        {
            ConstructionTarget target;
            if (value < 0)
                target = new ConstructionTarget(-value);
            else
                target = new ConstructionTarget(value);
            return target;
        }
        catch (InvalidOperationException)
        {
            return new ConstructionTarget(0);
        }
    }

    private static ConstructionTarget MixedCaller(int value) =>
        new(Pass(value));

    private static int Pass(int value) => value;

    private static ConstructionTarget Route(int value) =>
        new(value + 100);

    private static ConstructionTarget WrongRoute(long value) =>
        new(((int)value));

    private static ConstructionTarget GenericRoute<T>(int value) =>
        new(value + typeof(T).Name.Length);

    private static ConstructionTarget GenericCaller<T>(int value) =>
        new(value);

    private sealed class ConstructionTarget
    {
        internal ConstructionTarget(int value) => Value = value;

        internal int Value { get; }
    }

    private sealed class OtherConstructionTarget
    {
        internal OtherConstructionTarget(int value) => Value = value;

        internal int Value { get; }
    }

    private static class GenericCallerHost<T>
    {
        internal static ConstructionTarget Call(int value) =>
            new(value + typeof(T).Name.Length);
    }

    private static class StaticConstructionTarget
    {
        static StaticConstructionTarget()
        {
        }
    }
}
