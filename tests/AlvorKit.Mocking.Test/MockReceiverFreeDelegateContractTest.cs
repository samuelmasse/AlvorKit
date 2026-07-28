namespace AlvorKit.Mocking.Test;

/// <summary>Verifies receiver-free delegate metadata fails closed.</summary>
[TestClass]
public sealed class MockReceiverFreeDelegateContractTest
{
    /// <summary>Type initializers are neither construction nor constructor-body operations.</summary>
    [TestMethod]
    public void ValidateRejectsStaticConstructorRoutes()
    {
        ConstructorInfo typeInitializer =
            typeof(StaticConstructorTarget).TypeInitializer!;

        Assert.ThrowsExactly<MockException>(() =>
            MockReceiverFreeDelegateContract.Validate(
                Site(MockInvocationOperationKind.Construction),
                typeInitializer,
                typeof(StaticConstruction).GetMethod(
                    nameof(StaticConstruction.Invoke))!));
        Assert.ThrowsExactly<MockException>(() =>
            MockReceiverFreeDelegateContract.Validate(
                Site(MockInvocationOperationKind.ConstructorBody),
                typeInitializer,
                typeof(Action<StaticConstructorTarget>).GetMethod(
                    nameof(Action.Invoke))!));
    }

    private static MockInterceptionSiteDescriptor Site(
        MockInvocationOperationKind kind) =>
        new(
            typeof(MockReceiverFreeDelegateContractTest).Module
                .ModuleVersionId,
            typeof(MockReceiverFreeDelegateContractTest).GetMethod(
                nameof(ValidateRejectsStaticConstructorRoutes))!
                .MetadataToken,
            0,
            kind);

    private delegate StaticConstructorTarget StaticConstruction();

    private sealed class StaticConstructorTarget
    {
        static StaticConstructorTarget()
        {
        }
    }
}
