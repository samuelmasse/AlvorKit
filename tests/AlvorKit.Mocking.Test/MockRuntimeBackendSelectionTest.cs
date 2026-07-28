namespace AlvorKit.Mocking.Test;

/// <summary>Freezes independent proxy and operation capability selection.</summary>
[TestClass]
public sealed class MockRuntimeBackendSelectionTest
{
    /// <summary>Repeated Dynamic selection changes only the proxy capability.</summary>
    [TestMethod]
    public void DynamicEnable_RepeatedSelectionIsIdempotent()
    {
        IMockProxyCallbackBackend expectedProxy =
            MockRuntimeBackendRegistry.Proxy;
        IMockOperationBackend? expectedOperation =
            MockRuntimeBackendRegistry.ExplicitOperation;

        MockDynamic.Enable();
        MockDynamic.Enable();

        Assert.AreSame(
            expectedProxy,
            MockRuntimeBackendRegistry.Proxy);
        Assert.AreEqual(
            "dynamic",
            MockRuntimeBackendRegistry.Proxy.Name);
        Assert.AreSame(
            expectedOperation,
            MockRuntimeBackendRegistry.ExplicitOperation);
    }

    /// <summary>The Interception operation backend is explicit and idempotent.</summary>
    [TestMethod]
    public void InterceptionEnable_RepeatedSelectionIsIdempotent()
    {
        IMockProxyCallbackBackend expectedProxy =
            MockRuntimeBackendRegistry.Proxy;

        MockInterception.Enable();
        MockInterception.Enable();

        Assert.AreSame(
            expectedProxy,
            MockRuntimeBackendRegistry.Proxy);
        Assert.AreSame(
            MockInterceptionOperationBackend.Instance,
            MockRuntimeBackendRegistry.ExplicitOperation);
        Assert.AreEqual(
            "interception",
            MockRuntimeBackendRegistry.Operation.Name);
        Assert.AreSame(
            MockInterceptionOperationBackend.Instance,
            MockRuntimeBackendRegistry.Operation);
    }

    /// <summary>A second proxy backend cannot replace the selected process proxy.</summary>
    [TestMethod]
    public void RegisterProxy_ConflictingBackendFailsClearly()
    {
        MockException exception = Assert.Throws<MockException>(
            () => MockRuntimeBackendRegistry.RegisterProxy(
                new ConflictingProxyBackend()));

        StringAssert.Contains(
            exception.Message,
            "dynamic");
        StringAssert.Contains(
            exception.Message,
            "generated-test");
        StringAssert.Contains(
            exception.Message,
            "same process");
    }

    /// <summary>A second operation backend cannot replace Interception.</summary>
    [TestMethod]
    public void RegisterOperation_ConflictingBackendFailsClearly()
    {
        MockDynamic.Enable();
        MockException conflict = Assert.Throws<MockException>(
            () => MockRuntimeBackendRegistry.RegisterOperation(
                new ConflictingOperationBackend()));

        Assert.AreSame(
            DynamicMockRuntimeBackend.Instance,
            MockRuntimeBackendRegistry.Proxy);
        Assert.AreSame(
            MockInterceptionOperationBackend.Instance,
            MockRuntimeBackendRegistry.Operation);
        StringAssert.Contains(
            conflict.Message,
            "operation-interception");
        StringAssert.Contains(
            conflict.Message,
            "interception");
        StringAssert.Contains(
            conflict.Message,
            "conflicting-operation");
    }

    /// <summary>Missing operation guidance names the Interception package and facade.</summary>
    [TestMethod]
    public void MissingOperationBackend_HasActionableDiagnostic()
    {
        MockException failure =
            MockRuntimeBackendRegistry.MissingOperationBackend();

        StringAssert.Contains(
            failure.Message,
            "Interception");
        StringAssert.Contains(
            failure.Message,
            "AlvorKit.Mocking.Interception");
        StringAssert.Contains(failure.Message, "MockInterception.Enable()");
    }

    /// <summary>A fresh core-only load explains the Dynamic JIT selection path.</summary>
    [TestMethod]
    public void CoreOnlyCreate_MissingBackendHasActionableDiagnostic()
    {
        var context = new System.Runtime.Loader.AssemblyLoadContext(
            "MockingCoreOnlyBoundary",
            isCollectible: true);
        try
        {
            Assembly core = context.LoadFromAssemblyPath(
                typeof(Mock).Assembly.Location);
            Type mockType = core.GetType(
                typeof(Mock).FullName!,
                throwOnError: true)!;
            Type behaviorType = core.GetType(
                typeof(MockBehavior).FullName!,
                throwOnError: true)!;
            MethodInfo create = mockType.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.Static)
                .Single(method =>
                {
                    ParameterInfo[] parameters =
                        method.GetParameters();
                    return method.Name == nameof(Mock.Create) &&
                        parameters.Length == 2 &&
                        parameters[0].ParameterType == typeof(Type);
                });
            object strict = Enum.Parse(
                behaviorType,
                nameof(MockBehavior.Strict));

            TargetInvocationException invocation =
                Assert.Throws<TargetInvocationException>(
                    () => create.Invoke(
                        null,
                        [typeof(Stream), strict]));
            Exception failure = invocation.InnerException!;

            Assert.AreEqual(
                typeof(MockException).FullName,
                failure.GetType().FullName);
            StringAssert.Contains(
                failure.Message,
                "AlvorKit.Mocking.Dynamic");
            StringAssert.Contains(failure.Message, "MockDynamic.Enable()");
        }
        finally
        {
            context.Unload();
        }
    }

    private sealed class ConflictingProxyBackend :
        IMockProxyCallbackBackend
    {
        public string Name => "generated-test";

        public Type ResolveMockType(Type mockedType) =>
            throw new NotSupportedException();

        public void PrepareCapture(Delegate capture) =>
            throw new NotSupportedException();

        public Delegate NormalizeCallback(
            Delegate callback,
            MethodInfo capturedMethod) =>
            throw new NotSupportedException();

        public Delegate NormalizeConstructorCallback(
            Delegate callback,
            MethodInfo logicalMethod) =>
            throw new NotSupportedException();
    }

    private sealed class ConflictingOperationBackend :
        IMockOperationBackend
    {
        public string Name => "conflicting-operation";

        public TDelegate BindInterception<TDelegate>(
            MockInterceptionSiteDescriptor site,
            MemberInfo operation,
            TDelegate original)
            where TDelegate : Delegate =>
            throw new NotSupportedException();
    }

}
