namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockTypedBackendTest
{
    /// <summary>
    /// Proves configured value, ref, and out dispatch across every supported instance shape.
    /// </summary>
    [TestMethod]
    public void ConfiguredDispatch_AllInstanceShapesUseOneContract()
    {
        AssertConfigured(Mock.Create<IMockTarget>(), 101);
        AssertConfigured(Mock.Create<AbstractMock>(), 401);
        AssertConfigured(Mock.Create<PartialMock>(), 501);
        AssertConfigured(Mock.Create<VirtualMock>(), 601);
    }

    /// <summary>
    /// Proves strict and loose fallbacks agree across proxy-owned shapes.
    /// </summary>
    [TestMethod]
    public void FallbackDispatch_ProxyPathsHaveParity()
    {
        AssertFallbackParity<IMockTarget>();
        AssertFallbackParity<AbstractMock>();
    }

    /// <summary>
    /// Proves generated overrides preserve return and parameter metadata used by typed dispatch.
    /// </summary>
    [TestMethod]
    public void ProxyEmission_PreservesCanonicalMethodMetadata()
    {
        var mock = Mock.Create<InParamMock>();
        MethodInfo source = typeof(InParamMock).GetMethod(
            nameof(InParamMock.Transform))!;
        MethodInfo proxy = mock.GetType().GetMethod(
            nameof(InParamMock.Transform))!;

        AssertParameterMetadata(source.ReturnParameter, proxy.ReturnParameter);
        ParameterInfo[] sourceParameters = source.GetParameters();
        ParameterInfo[] proxyParameters = proxy.GetParameters();
        Assert.AreEqual(sourceParameters.Length, proxyParameters.Length);

        for (int index = 0; index < sourceParameters.Length; index++)
            AssertParameterMetadata(sourceParameters[index], proxyParameters[index]);

        Assert.AreEqual(
            MockCanonicalSignature.Create(source),
            MockCanonicalSignature.Create(proxy));
    }

    /// <summary>
    /// Proves unsupported proxy signatures are rejected before proxy emission.
    /// </summary>
    [TestMethod]
    public void UnsupportedSignature_IsRejectedBeforeProxyEmission()
    {
        Type unsupported = CreateVariableArgumentInterface();
        MethodInfo method = unsupported.GetMethod("Variable")!;

        MockException proxyError = Assert.Throws<MockException>(
            () => Mock.Create(unsupported));
        StringAssert.Contains(proxyError.Message, "Proxy ABI 2");
        StringAssert.Contains(proxyError.Message, "variable argument");

    }

    /// <summary>
    /// Proves typed backend process caches contain only runtime metadata, never per-mock behavior.
    /// </summary>
    [TestMethod]
    public void TypedBackendCaches_DoNotStorePerMockState()
    {
        Type[] forbidden =
        [
            typeof(Mocked),
            typeof(MockSetup),
            typeof(MockSetupStore),
            typeof(MockInvocationLedger),
        ];
        Type[] owners =
        [
            typeof(Proxies),
            typeof(MockTypedTrampolineCache),
        ];

        foreach (Type owner in owners)
        {
            foreach (FieldInfo field in owner.GetFields(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Assert.IsFalse(
                    ContainsAnyType(field.FieldType, forbidden),
                    $"{owner.Name}.{field.Name} stores per-mock state through {field.FieldType}.");
                Assert.IsFalse(
                    typeof(Delegate).IsAssignableFrom(field.FieldType),
                    $"{owner.Name}.{field.Name} stores a per-test delegate.");
            }
        }
    }

    private static void AssertConfigured<T>(T mock, int configured)
        where T : class, IMockTarget
    {
        Mock.When(mock.GetValue).Return(configured);
        Mock.When(() => mock.ComputeSum(2, 3)).Return(configured + 1);
        int setupReference = 7;
        Mock.When(() => mock.Write(ref setupReference))
            .Do(call => call.SetReference(0, configured + 2));
        Mock.When(() => mock.Read(out _))
            .Do(call => call.SetReference(0, configured + 3));

        Assert.AreEqual(configured, mock.GetValue());
        Assert.AreEqual(configured + 1, mock.ComputeSum(2, 3));

        int reference = 7;
        mock.Write(ref reference);
        mock.Read(out int output);
        Assert.AreEqual(configured + 2, reference);
        Assert.AreEqual(configured + 3, output);

        Mock.Verify(mock.GetValue).Once();
        Mock.Verify(() => mock.ComputeSum(2, 3)).Once();
        int verifiedReference = 7;
        Mock.Verify(() => mock.Write(ref verifiedReference)).Once();
        Mock.Verify(() => mock.Read(out _)).Once();
        Mock.VerifyNoOtherCalls(mock);
    }

    private static void AssertFallbackParity<T>()
        where T : class, IMockTarget
    {
        T strict = Mock.Create<T>();
        T loose = Mock.CreateLoose<T>();

        Assert.Throws<MockException>(() => strict.GetValue());
        Assert.AreEqual(0, loose.GetValue());

        Mock.Verify(strict.GetValue).Once();
        Mock.Verify(loose.GetValue).Once();
        Mock.VerifyNoOtherCalls(strict);
        Mock.VerifyNoOtherCalls(loose);
    }

    private static void AssertParameterMetadata(
        ParameterInfo expected,
        ParameterInfo actual)
    {
        Assert.AreEqual(expected.ParameterType, actual.ParameterType);
        Assert.AreEqual(expected.Attributes, actual.Attributes);
        CollectionAssert.AreEqual(
            expected.GetRequiredCustomModifiers(),
            actual.GetRequiredCustomModifiers());
        CollectionAssert.AreEqual(
            expected.GetOptionalCustomModifiers(),
            actual.GetOptionalCustomModifiers());
    }

    private static Type CreateVariableArgumentInterface()
    {
        var assembly = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"MockTypedBackendTest_{Guid.NewGuid():N}"),
            System.Reflection.Emit.AssemblyBuilderAccess.RunAndCollect);
        var module = assembly.DefineDynamicModule("Unsupported");
        var type = module.DefineType(
            "IVariableArgumentMock",
            TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
        type.DefineMethod(
            "Variable",
            MethodAttributes.Public
            | MethodAttributes.Abstract
            | MethodAttributes.Virtual
            | MethodAttributes.NewSlot,
            CallingConventions.HasThis | CallingConventions.VarArgs,
            typeof(void),
            Type.EmptyTypes);
        return type.CreateType()!;
    }

    private static bool ContainsAnyType(Type candidate, Type[] forbidden)
    {
        foreach (Type type in forbidden)
        {
            if (candidate == type)
                return true;
        }

        if (candidate.HasElementType)
            return ContainsAnyType(candidate.GetElementType()!, forbidden);

        foreach (Type argument in candidate.GetGenericArguments())
        {
            if (ContainsAnyType(argument, forbidden))
                return true;
        }

        return false;
    }
}
