namespace AlvorKit.Mocking.Test;

/// <summary>Proves generated proxy overrides dispatch through owned exact prefixes.</summary>
[TestClass]
public sealed class MockDirectProxyDispatchTest
{
    /// <summary>Every emitted non-generic interface override calls its exact cache prefix.</summary>
    [TestMethod]
    public void GeneratedOverrides_CallDirectPrefixes()
    {
        var strict = Mock.Create<IDirectProxyTarget>();
        Type proxyType = strict.GetType();
        InterfaceMapping mapping =
            proxyType.GetInterfaceMap(typeof(IDirectProxyTarget));

        foreach (MethodInfo method in mapping.TargetMethods)
        {
            Assert.IsTrue(
                CallsDirectPrefix(method),
                $"{method.Name} does not call a ProxyMethodCache prefix.");
        }

        Assert.IsFalse(proxyType.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic)
            .Any(static field =>
                field.Name.StartsWith("__ref_", StringComparison.Ordinal)));
        Assert.ThrowsExactly<MockException>(() => strict.Unconfigured());
        Assert.AreEqual(
            MockBackendLabel.ProxyInstance,
            Snapshot(strict).Single().Identity.Backend);

        var loose = Mock.CreateLoose<IDirectProxyTarget>();
        Assert.AreEqual(0, loose.Unconfigured());
    }

    /// <summary>Direct bodies preserve typed frames, reference writeback, aliases, factories, throws, and events.</summary>
    [TestMethod]
    public void ExactDispatch_PreservesTypedAndReferenceContracts()
    {
        var target = Mock.Create<IDirectProxyTarget>();
        var aliases = new DirectProxyAliasOwner([89, 144]);
        int[] viewOwner = [13, 21, 34];
        int setupValue = 5;
        var expected = new IOException("direct proxy");
        Mock.When(() => target.Calculate(
                Arg.Any<ReadOnlySpan<int>>(0),
                ref setupValue,
                out _))
            .Answer(new DirectProxyCalculateAnswer(
                static (
                    values,
                    ref value,
                    out doubled) =>
                {
                    value += values.Length;
                    doubled = value * 2;
                    return values.ToArray().Sum();
                }));
        Mock.When(target.View)
            .ReturnFactory(() => viewOwner.AsSpan());
        Mock.WhenRef(target.Mutable)
            .ReturnRef(aliases.Mutable);
        Mock.WhenRefReadonly(target.ReadOnly)
            .ReturnRef(aliases.ReadOnly);
        Mock.When(() => target.Fail(7)).Throw(expected);
        var raised = 0;
        target.Changed += value => raised += value;

        int value = 5;
        Assert.AreEqual(
            6,
            target.Calculate([1, 2, 3], ref value, out int doubled));
        ReadOnlySpan<int> view = target.View();
        ref int mutable = ref target.Mutable();
        ref readonly int readOnly = ref target.ReadOnly();
        Exception actual = Assert.ThrowsExactly<IOException>(
            () => target.Fail(7));
        Mock.Raise(() => target.Changed += null, 11);

        Assert.AreEqual(8, value);
        Assert.AreEqual(16, doubled);
        Assert.IsTrue(view.SequenceEqual(viewOwner));
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref mutable,
                ref aliases.Mutable()));
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref System.Runtime.CompilerServices.Unsafe.AsRef(in readOnly),
                ref System.Runtime.CompilerServices.Unsafe.AsRef(
                    in aliases.ReadOnly())));
        Assert.AreSame(expected, actual);
        Assert.AreEqual(11, raised);
        Assert.IsTrue(Snapshot(target).All(static invocation =>
            invocation.Identity.Backend == MockBackendLabel.ProxyInstance));
    }

    private static MockInvocation[] Snapshot(object target) =>
        [.. Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];

    private static bool CallsDirectPrefix(MethodInfo method)
    {
        byte[] il = method.GetMethodBody()!.GetILAsByteArray()!;
        for (int index = 0; index < il.Length;)
        {
            OpCode opcode = ReadOpcode(il, ref index);
            int operandIndex = index;
            int operandSize = OperandSize(opcode, il, index);
            index += operandSize;
            if (opcode != OpCodes.Call ||
                opcode.OperandType != OperandType.InlineMethod)
            {
                continue;
            }

            int token = BitConverter.ToInt32(il, operandIndex);
            MethodBase? called = method.Module.ResolveMethod(token);
            if (called?.Name == "Prefix" &&
                called.DeclaringType?.Name.StartsWith(
                    "ProxyMethodCache_",
                    StringComparison.Ordinal) == true)
            {
                return true;
            }
        }

        return false;
    }

    private static OpCode ReadOpcode(
        byte[] bytes,
        ref int index)
    {
        ushort value = bytes[index++];
        if (value == 0xfe)
            value = (ushort)(0xfe00 | bytes[index++]);

        foreach (FieldInfo field in typeof(OpCodes).GetFields(
            BindingFlags.Public | BindingFlags.Static))
        {
            var opcode = (OpCode)field.GetValue(null)!;
            if ((ushort)opcode.Value == value)
                return opcode;
        }

        throw new InvalidOperationException(
            $"Unknown IL opcode 0x{value:x4}.");
    }

    private static int OperandSize(
        OpCode opcode,
        byte[] bytes,
        int index) =>
        opcode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or
            OperandType.ShortInlineI or
            OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI or
            OperandType.InlineBrTarget or
            OperandType.InlineField or
            OperandType.InlineMethod or
            OperandType.InlineSig or
            OperandType.InlineString or
            OperandType.InlineTok or
            OperandType.InlineType or
            OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch =>
                4 + BitConverter.ToInt32(bytes, index) * 4,
            _ => throw new InvalidOperationException(
                $"Unknown operand type {opcode.OperandType}.")
        };
}

internal delegate int DirectProxyCalculateAnswer(
    ReadOnlySpan<int> values,
    ref int value,
    out int doubled);

internal interface IDirectProxyTarget
{
    event Action<int> Changed;

    int Calculate(
        ReadOnlySpan<int> values,
        ref int value,
        out int doubled);

    ReadOnlySpan<int> View();

    ref int Mutable();

    ref readonly int ReadOnly();

    int Fail(int value);

    int Unconfigured();
}

internal sealed class DirectProxyAliasOwner(int[] values)
{
    internal ref int Mutable() => ref values[0];

    internal ref readonly int ReadOnly() => ref values[1];
}
