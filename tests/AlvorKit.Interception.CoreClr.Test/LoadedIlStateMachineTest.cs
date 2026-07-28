using System.Reflection;
using System.Runtime.CompilerServices;

namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Verifies source-method targeting of synchronous and generated loaded bodies.</summary>
[TestClass]
public sealed class LoadedIlStateMachineTest
{
    /// <summary>A synchronous source retains its own MethodDef, body, and attribution.</summary>
    [TestMethod]
    public void SynchronousSource_UsesOwnLoadedBodyAndAttribution()
    {
        var source = Source(nameof(SynchronousSource));
        var body = Body(1);
        var bodies = new LoadedBodyResolver().Add(source, body);

        var resolution = LoadedSourceMethodResolver.Resolve(source, bodies);

        Assert.IsTrue(resolution.IsSuccessful);
        var target = resolution.Target!;
        Assert.AreEqual(LoadedSourceMethodKind.Synchronous, target.Kind);
        Assert.AreEqual(
            InterceptionTarget.FromMethod(source),
            target.SourceMethod);
        Assert.AreEqual(target.SourceMethod, target.BodyMethod);
        Assert.IsFalse(target.UsesGeneratedBody);
        Assert.AreSame(body, target.Body);
        Assert.AreSame(body.Identity, target.BodyIdentity);
        StringAssert.Contains(
            target.SourceAttribution,
            nameof(SynchronousSource));
        Assert.AreEqual(
            target.SourceAttribution,
            target.BodyAttribution);
    }

    /// <summary>Async and iterator source methods map to their generated MoveNext MethodDefs.</summary>
    [TestMethod]
    public void SourceMembers_MapToMoveNextSites()
    {
        var asyncSource = Source(nameof(AsyncSource));
        var iteratorSource = Source(nameof(IteratorSource));
        var asyncMoveNext = MoveNext(asyncSource);
        var iteratorMoveNext = MoveNext(iteratorSource);
        var asyncBody = Body(2);
        var iteratorBody = Body(3);
        var bodies = new LoadedBodyResolver()
            .Add(asyncMoveNext, asyncBody)
            .Add(iteratorMoveNext, iteratorBody);

        var asyncResolution = LoadedSourceMethodResolver.Resolve(
            asyncSource,
            bodies);
        var iteratorResolution = LoadedSourceMethodResolver.Resolve(
            iteratorSource,
            bodies);

        AssertResolvedStateMachine(
            asyncResolution,
            LoadedSourceMethodKind.Async,
            asyncSource,
            asyncMoveNext,
            asyncBody);
        AssertResolvedStateMachine(
            iteratorResolution,
            LoadedSourceMethodKind.Iterator,
            iteratorSource,
            iteratorMoveNext,
            iteratorBody);
    }

    /// <summary>A generated body absent from the loaded baseline rejects without fallback.</summary>
    [TestMethod]
    public void MissingLoadedMoveNextBody_RejectsDeterministically()
    {
        var source = Source(nameof(AsyncSource));
        LoadedBodyResolver bodies = new();

        var first = LoadedSourceMethodResolver.Resolve(source, bodies);
        var second = LoadedSourceMethodResolver.Resolve(source, bodies);

        Assert.IsFalse(first.IsSuccessful);
        Assert.IsNull(first.Target);
        var rejection = first.Rejections.Single();
        Assert.AreEqual(
            LoadedSourceMethodRejectionReason.MissingLoadedBody,
            rejection.Reason);
        StringAssert.Contains(rejection.RelatedMetadata, "MoveNext");
        Assert.AreEqual(
            rejection.Detail,
            second.Rejections.Single().Detail);
    }

    /// <summary>Ambiguous, missing, and unsupported state-machine metadata reject distinctly.</summary>
    [TestMethod]
    public void MalformedStateMachineMetadata_RejectsDeterministically()
    {
        LoadedBodyResolver bodies = new();
        var ambiguous = LoadedSourceMethodResolver.Resolve(
            Source(nameof(AmbiguousStateMachineSource)),
            bodies);
        var missing = LoadedSourceMethodResolver.Resolve(
            Source(nameof(MissingMoveNextSource)),
            bodies);
        var unsupported = LoadedSourceMethodResolver.Resolve(
            Source(nameof(UnsupportedStateMachineSource)),
            bodies);
        var repeatedAmbiguous = LoadedSourceMethodResolver.Resolve(
            Source(nameof(AmbiguousStateMachineSource)),
            bodies);

        CollectionAssert.AreEqual(
            new[]
            {
                LoadedSourceMethodRejectionReason
                    .AmbiguousStateMachineMetadata,
                LoadedSourceMethodRejectionReason.MissingMoveNextBody,
                LoadedSourceMethodRejectionReason
                    .UnsupportedStateMachineMetadata
            },
            new[]
            {
                ambiguous.Rejections.Single().Reason,
                missing.Rejections.Single().Reason,
                unsupported.Rejections.Single().Reason
            });
        StringAssert.Contains(
            ambiguous.Rejections.Single().Detail,
            nameof(AsyncStateMachineAttribute));
        StringAssert.Contains(
            ambiguous.Rejections.Single().Detail,
            nameof(IteratorStateMachineAttribute));
        StringAssert.Contains(
            missing.Rejections.Single().Detail,
            "no exact MoveNext body");
        StringAssert.Contains(
            unsupported.Rejections.Single().Detail,
            nameof(UnsupportedStateMachineAttribute));
        Assert.AreEqual(
            ambiguous.Rejections.Single().Detail,
            repeatedAmbiguous.Rejections.Single().Detail);
    }

    /// <summary>Asserts one exact source-to-generated-body targeting result.</summary>
    private static void AssertResolvedStateMachine(
        LoadedSourceMethodResolution resolution,
        LoadedSourceMethodKind kind,
        MethodInfo source,
        MethodInfo moveNext,
        LoadedMethodBodySnapshot body)
    {
        Assert.IsTrue(resolution.IsSuccessful);
        var target = resolution.Target!;
        Assert.AreEqual(kind, target.Kind);
        Assert.AreEqual(
            InterceptionTarget.FromMethod(source),
            target.SourceMethod);
        Assert.AreEqual(
            InterceptionTarget.FromMethod(moveNext),
            target.BodyMethod);
        Assert.IsTrue(target.UsesGeneratedBody);
        Assert.AreSame(body, target.Body);
        Assert.AreSame(body.Identity, target.BodyIdentity);
        StringAssert.Contains(target.SourceAttribution, source.Name);
        StringAssert.Contains(target.BodyAttribution, "MoveNext");
        Assert.AreNotEqual(
            target.SourceMethod.MethodToken,
            target.BodyMethod.MethodToken);
    }

    /// <summary>Gets one declared fixture source method.</summary>
    private static MethodInfo Source(string name) =>
        typeof(LoadedIlStateMachineTest).GetMethod(
            name,
            BindingFlags.Static |
            BindingFlags.NonPublic)!;

    /// <summary>Gets the exact generated MoveNext MethodDef named by a standard marker.</summary>
    private static MethodInfo MoveNext(MethodInfo source)
    {
        var attribute = source.CustomAttributes.Single(value =>
            value.AttributeType == typeof(AsyncStateMachineAttribute) ||
            value.AttributeType == typeof(IteratorStateMachineAttribute));
        var stateMachineType =
            (Type)attribute.ConstructorArguments.Single().Value!;
        return stateMachineType.GetMethod(
            "MoveNext",
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly)!;
    }

    /// <summary>Creates a distinct authoritative loaded-body fixture.</summary>
    private static LoadedMethodBodySnapshot Body(int nopCount) =>
        LoadedMethodBodyDecoder.Decode(
            LoadedMethodBodyFixture.Tiny(
                [.. Enumerable.Repeat((byte)0x00, nopCount), 0x2A]));

    /// <summary>Provides one ordinary synchronous source body.</summary>
    private static int SynchronousSource(int value) => value + 1;

    /// <summary>Provides one compiler-generated async state-machine source.</summary>
    private static async Task<int> AsyncSource(int value)
    {
        await Task.Yield();
        return value;
    }

    /// <summary>Provides one compiler-generated iterator state-machine source.</summary>
    private static IEnumerable<int> IteratorSource(int value)
    {
        yield return value;
    }

    /// <summary>Provides deliberately ambiguous standard state-machine markers.</summary>
    [AsyncStateMachine(typeof(ValidStateMachine))]
    [IteratorStateMachine(typeof(ValidStateMachine))]
    private static void AmbiguousStateMachineSource()
    {
    }

    /// <summary>Provides a standard marker whose generated type has no MoveNext body.</summary>
    [IteratorStateMachine(typeof(MissingMoveNextStateMachine))]
    private static void MissingMoveNextSource()
    {
    }

    /// <summary>Provides a nonstandard state-machine marker.</summary>
    [UnsupportedStateMachine(typeof(ValidStateMachine))]
    private static void UnsupportedStateMachineSource()
    {
    }

    /// <summary>Provides one exact parameterless MoveNext candidate.</summary>
    private sealed class ValidStateMachine
    {
        /// <summary>Represents one controlled generated-body candidate.</summary>
        public void MoveNext()
        {
        }
    }

    /// <summary>Provides malformed state-machine metadata without a MoveNext method.</summary>
    private sealed class MissingMoveNextStateMachine
    {
    }

    /// <summary>Marks unsupported state-machine metadata for deterministic rejection.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    private sealed class UnsupportedStateMachineAttribute(Type stateMachineType) :
        StateMachineAttribute(stateMachineType);

    /// <summary>Resolves controlled authoritative bodies by exact runtime target.</summary>
    private sealed class LoadedBodyResolver :
        ILoadedMethodBodySnapshotResolver
    {
        /// <summary>The exact loaded bodies keyed by runtime identity.</summary>
        private readonly Dictionary<InterceptionTarget, LoadedMethodBodySnapshot>
            bodies = [];

        /// <summary>Adds one authoritative loaded body for a reflected MethodDef.</summary>
        internal LoadedBodyResolver Add(
            MethodInfo method,
            LoadedMethodBodySnapshot body)
        {
            bodies.Add(InterceptionTarget.FromMethod(method), body);
            return this;
        }

        /// <summary>Resolves a configured authoritative loaded body.</summary>
        public bool TryResolveLoadedBody(
            InterceptionTarget method,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out LoadedMethodBodySnapshot? body) =>
            bodies.TryGetValue(method, out body);
    }
}
