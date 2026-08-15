namespace AlvorKit;

/// <summary>Verifies exact trampolines for fully closed generic contexts.</summary>
[TestClass]
public sealed class InterceptionHandlerTrampolineGenericTest
{
    /// <summary>Closed value and reference declaring-type constructions stay independent.</summary>
    [TestMethod]
    public unsafe void ClosedDeclaringTypeConstructionsRemainIndependent()
    {
        using var integers = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactClosedGenericTarget<int>>(
                nameof(ExactClosedGenericTarget<>.Echo)),
            new ExactClosedGenericHandler<int>(70),
            Method<ExactClosedGenericHandler<int>>(
                nameof(ExactClosedGenericHandler<>.Echo)));
        using var strings = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactClosedGenericTarget<string>>(
                nameof(ExactClosedGenericTarget<>.Echo)),
            new ExactClosedGenericHandler<string>("seventy"),
            Method<ExactClosedGenericHandler<string>>(
                nameof(ExactClosedGenericHandler<>.Echo)));

        Assert.IsTrue(integers.TryAcquire(out var integerEntry));
        Assert.IsTrue(strings.TryAcquire(out var stringEntry));
        var integerResult =
            ((delegate* managed<
                ExactClosedGenericTarget<int>,
                int,
                int>)integerEntry)(new(), 7);
        var stringResult =
            ((delegate* managed<
                ExactClosedGenericTarget<string>,
                string,
                string>)stringEntry)(new(), "seven");

        Assert.AreEqual(70, integerResult);
        Assert.AreEqual("seventy", stringResult);
        Assert.AreNotEqual(integerEntry, stringEntry);
    }

    /// <summary>Two fully constructed generic-method signatures stay independent.</summary>
    [TestMethod]
    public unsafe void ConstructedMethodConstructionsRemainIndependent()
    {
        MethodInfo definition = Method<ExactConstructedGenericTarget>(
            nameof(ExactConstructedGenericTarget.Echo));
        var handler = new ExactConstructedGenericHandler();
        using var integers = InterceptionHandlerTrampolineFactory.Create(
            definition.MakeGenericMethod(typeof(int)),
            handler,
            Method<ExactConstructedGenericHandler>(
                nameof(ExactConstructedGenericHandler.EchoInt32)));
        using var strings = InterceptionHandlerTrampolineFactory.Create(
            definition.MakeGenericMethod(typeof(string)),
            handler,
            Method<ExactConstructedGenericHandler>(
                nameof(ExactConstructedGenericHandler.EchoString)));

        Assert.IsTrue(integers.TryAcquire(out var integerEntry));
        Assert.IsTrue(strings.TryAcquire(out var stringEntry));
        var target = new ExactConstructedGenericTarget();
        Assert.AreEqual(
            110,
            ((delegate* managed<
                ExactConstructedGenericTarget,
                int,
                int>)integerEntry)(target, 11));
        Assert.AreEqual(
            "one hundred ten",
            ((delegate* managed<
                ExactConstructedGenericTarget,
                string,
                string>)stringEntry)(target, "eleven"));
    }

    /// <summary>Closed generic handlers still require the exact constructed signature.</summary>
    [TestMethod]
    public void ConstructedHandlerMismatchIsRejected()
    {
        MethodInfo operation = Method<ExactConstructedGenericTarget>(
            nameof(ExactConstructedGenericTarget.Echo))
            .MakeGenericMethod(typeof(int));

        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
            InterceptionHandlerTrampolineFactory.Create(
                operation,
                new ExactConstructedGenericHandler(),
                Method<ExactConstructedGenericHandler>(
                    nameof(ExactConstructedGenericHandler.EchoString))));

        StringAssert.Contains(exception.Message, "return type");
    }

    /// <summary>Open and byref-like generic contexts remain rejected pristinely.</summary>
    [TestMethod]
    public void OpenOrUnsafeGenericContextsRemainRejected()
    {
        Assert.ThrowsExactly<NotSupportedException>(() =>
            InterceptionHandlerTrampolineFactory.Create(
                Method(
                    typeof(ExactClosedGenericTarget<>),
                    nameof(ExactClosedGenericTarget<>.Echo)),
                new ExactClosedGenericHandler<int>(1),
                Method<ExactClosedGenericHandler<int>>(
                    nameof(ExactClosedGenericHandler<>.Echo))));

        MethodInfo unsafeConstruction =
            Method<ExactUnsafeGenericTarget>(
                nameof(ExactUnsafeGenericTarget.Observe))
            .MakeGenericMethod(typeof(ExactBorrowedWindow));
        var exception = Assert.ThrowsExactly<NotSupportedException>(() =>
            InterceptionHandlerTrampolineFactory.Create(
                unsafeConstruction,
                new ExactUnsafeGenericHandler(),
                Method<ExactUnsafeGenericHandler>(
                    nameof(ExactUnsafeGenericHandler.Observe))));
        StringAssert.Contains(exception.Message, "unsupported");
    }

    private static MethodInfo Method<T>(string name) =>
        Method(typeof(T), name);

    private static MethodInfo Method(Type type, string name) =>
        type.GetMethod(name) ??
        throw new InvalidOperationException(
            $"Method '{type.FullName}.{name}' was not found.");
}

/// <summary>Defines an ordinary method on a generic declaring type.</summary>
public sealed class ExactClosedGenericTarget<T>
{
    /// <summary>Returns the supplied value.</summary>
    public T Echo(T value) => value;
}

/// <summary>Handles one closed declaring-type construction.</summary>
public sealed class ExactClosedGenericHandler<T>(T result)
{
    /// <summary>Returns the construction-specific replacement.</summary>
    public T Echo(ExactClosedGenericTarget<T> _, T __) => result;
}

/// <summary>Defines one generic method with independently constructed signatures.</summary>
public sealed class ExactConstructedGenericTarget
{
    /// <summary>Returns the supplied constructed value.</summary>
    public T Echo<T>(T value) => value;
}

/// <summary>Handles value and reference constructions independently.</summary>
public sealed class ExactConstructedGenericHandler
{
    /// <summary>Handles the integer construction.</summary>
    public int EchoInt32(
        ExactConstructedGenericTarget _,
        int __) =>
        110;

    /// <summary>Handles the string construction.</summary>
    public string EchoString(
        ExactConstructedGenericTarget _,
        string __) =>
        "one hundred ten";
}

/// <summary>Defines a generic method that explicitly permits ref-struct arguments.</summary>
public sealed class ExactUnsafeGenericTarget
{
    /// <summary>Observes one generic value without retaining it.</summary>
    public int Observe<T>(T value)
        where T : allows ref struct
    {
        _ = value;
        return 0;
    }
}

/// <summary>Mirrors the unsafe closed signature for pristine validation.</summary>
public sealed class ExactUnsafeGenericHandler
{
    /// <summary>Observes one borrowed value.</summary>
    public int Observe(
        ExactUnsafeGenericTarget _,
        ExactBorrowedWindow __) =>
        0;
}
