namespace AlvorKit;

[TestClass]
public sealed unsafe class MockCanonicalSignatureTest
{
    private static readonly MockBackendIdentity InterceptionBackend =
        new(MockBackendKind.Interception, 1);
    private static readonly MockBackendIdentity ProxyBackend = new(MockBackendKind.Proxy, 1);

    /// <summary>Canonicalization preserves declared order, passing modes, scoped metadata, pointers, and exact modifiers.</summary>
    [TestMethod]
    public void Create_MixedSignaturePreservesExactDeclaredShape()
    {
        MethodInfo method = GetClosedMethod(nameof(SignatureTarget<>.Mixed));

        MockCanonicalSignature signature = MockCanonicalSignature.Create(method);

        Assert.AreEqual(method.CallingConvention, signature.CallingConvention);
        Assert.AreEqual(MockReturnKind.Value, signature.Return.Kind);
        Assert.AreEqual(typeof(int), signature.Return.Type.RuntimeType);
        AssertModifiersEqual(method.ReturnParameter, signature.Return.RequiredModifiers, signature.Return.OptionalModifiers);
        Assert.AreEqual(7, signature.Parameters.Length);

        MockParameterShape value = signature.Parameters[0];
        Assert.AreEqual(0, value.DeclaredIndex);
        Assert.AreEqual(MockPassingKind.Value, value.Passing);
        Assert.AreEqual(typeof(int), value.Type.RuntimeType);

        MockParameterShape input = signature.Parameters[1];
        Assert.AreEqual(1, input.DeclaredIndex);
        Assert.AreEqual(MockPassingKind.ManagedReference, input.Passing);
        Assert.IsTrue(input.IsIn);
        Assert.IsFalse(input.IsOut);

        MockParameterShape mutable = signature.Parameters[2];
        Assert.AreEqual(2, mutable.DeclaredIndex);
        Assert.AreEqual(MockPassingKind.ManagedReference, mutable.Passing);
        Assert.IsFalse(mutable.IsIn);
        Assert.IsFalse(mutable.IsOut);

        MockParameterShape output = signature.Parameters[3];
        Assert.AreEqual(3, output.DeclaredIndex);
        Assert.AreEqual(MockPassingKind.ManagedReference, output.Passing);
        Assert.IsTrue(output.IsOut);

        MockParameterShape scopedSpan = signature.Parameters[4];
        Assert.AreEqual(4, scopedSpan.DeclaredIndex);
        Assert.AreEqual(MockPassingKind.RefStructValue, scopedSpan.Passing);
        Assert.AreEqual(typeof(Span<int>), scopedSpan.Type.RuntimeType);
        Assert.IsTrue(scopedSpan.IsScoped);

        MockParameterShape pointer = signature.Parameters[5];
        Assert.AreEqual(5, pointer.DeclaredIndex);
        Assert.AreEqual(MockPassingKind.Pointer, pointer.Passing);
        Assert.AreEqual(typeof(int*), pointer.Type.RuntimeType);

        MockParameterShape functionPointer = signature.Parameters[6];
        Assert.AreEqual(6, functionPointer.DeclaredIndex);
        Assert.AreEqual(MockPassingKind.FunctionPointer, functionPointer.Passing);
        Assert.AreEqual(typeof(delegate* unmanaged[Cdecl]<int, int>), functionPointer.Type.RuntimeType);

        ParameterInfo[] reflectedParameters = method.GetParameters();
        for (int index = 0; index < reflectedParameters.Length; index++)
        {
            Assert.AreEqual(index, signature.Parameters[index].DeclaredIndex);
            Assert.AreEqual(reflectedParameters[index].ParameterType, signature.Parameters[index].Type.RuntimeType);
            AssertModifiersEqual(
                reflectedParameters[index],
                signature.Parameters[index].RequiredModifiers,
                signature.Parameters[index].OptionalModifiers);
        }
    }

    /// <summary>Return canonicalization distinguishes mutable refs, readonly refs, and by-value ref structs.</summary>
    [TestMethod]
    public void Create_ReturnShapesPreserveReferenceAndRefStructKinds()
    {
        MethodInfo mutableMethod = GetClosedMethod(nameof(SignatureTarget<>.MutableReference));
        MethodInfo readOnlyMethod = GetClosedMethod(nameof(SignatureTarget<>.ReadOnlyReference));
        MethodInfo spanMethod = GetClosedMethod(nameof(SignatureTarget<>.ReturnSpan));

        MockCanonicalSignature mutable = MockCanonicalSignature.Create(mutableMethod);
        MockCanonicalSignature readOnly = MockCanonicalSignature.Create(readOnlyMethod);
        MockCanonicalSignature span = MockCanonicalSignature.Create(spanMethod);

        Assert.AreEqual(MockReturnKind.ManagedReference, mutable.Return.Kind);
        Assert.AreEqual(typeof(int).MakeByRefType(), mutable.Return.Type.RuntimeType);
        Assert.AreEqual(MockReturnKind.ReadOnlyManagedReference, readOnly.Return.Kind);
        Assert.AreEqual(typeof(int).MakeByRefType(), readOnly.Return.Type.RuntimeType);
        Assert.IsTrue(
            readOnly.Return.RequiredModifiers.Length + readOnly.Return.OptionalModifiers.Length > 0,
            "The readonly managed reference must retain its reflected custom modifier.");
        Assert.AreEqual(MockReturnKind.RefStructValue, span.Return.Kind);
        Assert.AreEqual(typeof(ReadOnlySpan<int>), span.Return.Type.RuntimeType);
        Assert.AreNotEqual(mutable, readOnly);
    }

    /// <summary>Method identity preserves overload definitions and substitutes constructed declaring and method type arguments.</summary>
    [TestMethod]
    public void Identity_OverloadsAndConstructedGenericsRemainDistinct()
    {
        Type targetType = typeof(SignatureTarget<int>);
        MethodInfo integerOverload = targetType.GetMethod(nameof(SignatureTarget<>.Overload), [typeof(int)])!;
        MethodInfo textOverload = targetType.GetMethod(nameof(SignatureTarget<>.Overload), [typeof(string)])!;
        MethodInfo genericDefinition = GetClosedMethod(nameof(SignatureTarget<>.Convert));
        MethodInfo stringConstruction = genericDefinition.MakeGenericMethod(typeof(string));
        MethodInfo guidConstruction = genericDefinition.MakeGenericMethod(typeof(Guid));

        MockMethodIdentity integerIdentity = MockMethodIdentity.Create(integerOverload);
        MockMethodIdentity textIdentity = MockMethodIdentity.Create(textOverload);
        MockMethodIdentity stringIdentity = MockMethodIdentity.Create(stringConstruction);
        MockMethodIdentity guidIdentity = MockMethodIdentity.Create(guidConstruction);
        MockCanonicalSignature stringSignature = MockCanonicalSignature.Create(stringConstruction);

        Assert.AreNotEqual(integerIdentity, textIdentity);
        Assert.AreNotEqual(stringIdentity, guidIdentity);
        CollectionAssert.AreEqual(
            new[] { typeof(int) },
            stringIdentity.DeclaringTypeArguments.Select(static argument => argument.RuntimeType).ToArray());
        CollectionAssert.AreEqual(
            new[] { typeof(string) },
            stringIdentity.MethodArguments.Select(static argument => argument.RuntimeType).ToArray());
        Assert.AreEqual(typeof(string), stringSignature.Return.Type.RuntimeType);
        Assert.AreEqual(typeof(int), stringSignature.Parameters[0].Type.RuntimeType);
        Assert.AreEqual(typeof(string), stringSignature.Parameters[1].Type.RuntimeType);
    }

    /// <summary>Equivalent keys compare equal while backend, ABI, operation, construction, definition, and runtime identity remain isolated.</summary>
    [TestMethod]
    public void DispatchKey_EquivalentAndDistinctRuntimeAxesDoNotCollide()
    {
        MethodInfo method = GetClosedMethod(nameof(SignatureTarget<>.Convert)).MakeGenericMethod(typeof(string));
        MockDispatchCacheKey expected = MockDispatchCacheKey.Create(
            typeof(SignatureTarget<int>),
            method,
            InterceptionBackend,
            MockOperationKind.InstanceMethod);
        MockDispatchCacheKey equivalent = MockDispatchCacheKey.Create(
            typeof(SignatureTarget<int>),
            method,
            InterceptionBackend,
            MockOperationKind.InstanceMethod);

        Assert.AreEqual(expected, equivalent);
        Assert.AreEqual(expected.GetHashCode(), equivalent.GetHashCode());
        Assert.AreNotEqual(
            expected,
            MockDispatchCacheKey.Create(
                typeof(SignatureTarget<int>),
                method,
                ProxyBackend,
                MockOperationKind.InstanceMethod));
        Assert.AreNotEqual(
            expected,
            MockDispatchCacheKey.Create(
                typeof(SignatureTarget<int>),
                method,
                new MockBackendIdentity(MockBackendKind.Interception, 2),
                MockOperationKind.InstanceMethod));
        Assert.AreNotEqual(
            expected,
            MockDispatchCacheKey.Create(
                typeof(SignatureTarget<int>),
                method,
                InterceptionBackend,
                MockOperationKind.StaticMethod));
        Assert.AreNotEqual(
            expected,
            MockDispatchCacheKey.Create(
                typeof(SignatureTarget<int>),
                GetClosedMethod(nameof(SignatureTarget<>.Convert)).MakeGenericMethod(typeof(Guid)),
                InterceptionBackend,
                MockOperationKind.InstanceMethod));

        (Type firstType, MethodInfo firstMethod) = CreateRuntimeTwin();
        (Type secondType, MethodInfo secondMethod) = CreateRuntimeTwin();
        MockDispatchCacheKey firstRuntime = MockDispatchCacheKey.Create(
            firstType,
            firstMethod,
            InterceptionBackend,
            MockOperationKind.InstanceMethod);
        MockDispatchCacheKey secondRuntime = MockDispatchCacheKey.Create(
            secondType,
            secondMethod,
            InterceptionBackend,
            MockOperationKind.InstanceMethod);

        Assert.AreEqual(firstType.FullName, secondType.FullName);
        Assert.AreNotSame(firstType, secondType);
        Assert.AreNotEqual(firstRuntime, secondRuntime);
    }

    /// <summary>Validation returns repeatable immutable reasons for backend, open-generic, and varargs rejection.</summary>
    [TestMethod]
    public void Validate_UnsupportedSignaturesReturnDeterministicReasons()
    {
        MethodInfo staticMethod = GetClosedMethod(nameof(SignatureTarget<>.Mixed));
        MethodInfo openMethod = typeof(SignatureTarget<>).GetMethod(nameof(SignatureTarget<>.Convert))!;
        MethodInfo variableArguments = typeof(VariableArgumentTarget).GetMethod(
            nameof(VariableArgumentTarget.VariableArguments))!;

        MockSignatureValidation firstProxy = MockSignatureValidator.Validate(
            staticMethod,
            ProxyBackend,
            MockOperationKind.StaticMethod);
        MockSignatureValidation secondProxy = MockSignatureValidator.Validate(
            staticMethod,
            ProxyBackend,
            MockOperationKind.StaticMethod);
        MockSignatureValidation open = MockSignatureValidator.Validate(
            openMethod,
            InterceptionBackend,
            MockOperationKind.InstanceMethod);
        MockSignatureValidation variable = MockSignatureValidator.Validate(
            variableArguments,
            InterceptionBackend,
            MockOperationKind.StaticMethod);

        Assert.IsFalse(firstProxy.IsSupported);
        Assert.AreEqual(MockUnsupportedSignatureReason.UnsupportedOperation, firstProxy.Rejection!.Reason);
        Assert.AreEqual(firstProxy.Rejection, secondProxy.Rejection);
        Assert.AreEqual(firstProxy.Rejection.Message, secondProxy.Rejection!.Message);
        StringAssert.Contains(firstProxy.Rejection.Message, "Proxy ABI 1");
        StringAssert.Contains(firstProxy.Rejection.Message, "StaticMethod");
        Assert.AreEqual(MockUnsupportedSignatureReason.OpenGenericSignature, open.Rejection!.Reason);
        Assert.AreEqual(MockUnsupportedSignatureReason.VariableArguments, variable.Rejection!.Reason);
        Assert.AreSame(open.Signature, open.Rejection.Signature);
        Assert.AreSame(variable.Signature, variable.Rejection.Signature);
    }

    /// <summary>Dispatch keys derive only from immutable runtime metadata and never retain instance, setup, history, session, or delegate state.</summary>
    [TestMethod]
    public void DispatchKey_ModelContainsNoMockOrInvocationState()
    {
        var first = new SignatureTarget<int>();
        var second = new SignatureTarget<int>();
        MethodInfo method = GetClosedMethod(nameof(SignatureTarget<>.Overload), typeof(int));

        MockDispatchCacheKey firstKey = MockDispatchCacheKey.Create(
            first.GetType(),
            method,
            InterceptionBackend,
            MockOperationKind.InstanceMethod);
        MockDispatchCacheKey secondKey = MockDispatchCacheKey.Create(
            second.GetType(),
            method,
            InterceptionBackend,
            MockOperationKind.InstanceMethod);

        Assert.AreEqual(firstKey, secondKey);
        AssertMetadataGraphContainsNoPerMockState(typeof(MockDispatchCacheKey));
    }

    private static MethodInfo GetClosedMethod(string name, params Type[] parameterTypes)
    {
        Type targetType = typeof(SignatureTarget<int>);
        return parameterTypes.Length == 0
            ? targetType.GetMethod(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)!
            : targetType.GetMethod(name, parameterTypes)!;
    }

    private static void AssertModifiersEqual(
        ParameterInfo reflected,
        System.Collections.Immutable.ImmutableArray<MockCustomModifier> required,
        System.Collections.Immutable.ImmutableArray<MockCustomModifier> optional)
    {
        CollectionAssert.AreEqual(
            reflected.GetRequiredCustomModifiers(),
            required.Select(static modifier => modifier.Type.RuntimeType).ToArray());
        CollectionAssert.AreEqual(
            reflected.GetOptionalCustomModifiers(),
            optional.Select(static modifier => modifier.Type.RuntimeType).ToArray());
    }

    private static void AssertMetadataGraphContainsNoPerMockState(Type root)
    {
        string[] forbiddenNames = ["Mocked", "Setup", "Behavior", "Invocation", "History", "Epoch", "Session", "Checkpoint"];
        var pending = new Stack<Type>();
        var visited = new HashSet<Type>();
        pending.Push(root);

        while (pending.TryPop(out Type? current))
        {
            if (!visited.Add(current))
                continue;

            foreach (FieldInfo field in current.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Type fieldType = field.FieldType;
                Assert.AreNotEqual(typeof(object), fieldType, $"{current.Name}.{field.Name} is an untyped state carrier.");
                Assert.IsFalse(typeof(Delegate).IsAssignableFrom(fieldType), $"{current.Name}.{field.Name} retains a delegate.");
                Assert.IsFalse(
                    forbiddenNames.Any(fieldType.Name.Contains),
                    $"{current.Name}.{field.Name} retains forbidden state type {fieldType}.");

                if (fieldType.Namespace == typeof(MockDispatchCacheKey).Namespace)
                    pending.Push(fieldType);
                foreach (Type argument in fieldType.GetGenericArguments())
                {
                    if (argument.Namespace == typeof(MockDispatchCacheKey).Namespace)
                        pending.Push(argument);
                }
            }
        }
    }

    private static (Type Type, MethodInfo Method) CreateRuntimeTwin()
    {
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("AlvorKit.Mocking.RuntimeTwin"),
            AssemblyBuilderAccess.RunAndCollect);
        ModuleBuilder module = assembly.DefineDynamicModule("RuntimeTwin");
        TypeBuilder type = module.DefineType("RuntimeTwin.Target", TypeAttributes.Public);
        MethodBuilder method = type.DefineMethod(
            "Echo",
            MethodAttributes.Public,
            CallingConventions.HasThis,
            typeof(int),
            [typeof(int)]);
        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ret);
        Type runtimeType = type.CreateType()!;
        return (runtimeType, runtimeType.GetMethod("Echo")!);
    }

    private sealed class SignatureTarget<T>
    {
        private int storage;

        public T? Overload(int value)
        {
            _ = value;
            return default;
        }

        public T? Overload(string value)
        {
            _ = value;
            return default;
        }

        public TResult Convert<TResult>(T input, TResult fallback)
        {
            _ = input;
            return fallback;
        }

        public static int Mixed(
            int value,
            in int input,
            ref int mutable,
            out int output,
            scoped Span<int> values,
            int* pointer,
            delegate* unmanaged[Cdecl]<int, int> callback)
        {
            output = value + input + mutable + values.Length + (pointer is null ? 0 : *pointer);
            return callback is null ? output : callback(output);
        }

        public ref int MutableReference() => ref storage;

        public ref readonly int ReadOnlyReference() => ref storage;

        public ReadOnlySpan<int> ReturnSpan(int[] values) => values;

    }

    private static class VariableArgumentTarget
    {
        public static void VariableArguments(__arglist)
        {
        }
    }
}
