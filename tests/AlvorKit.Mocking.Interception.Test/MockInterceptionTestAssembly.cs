namespace AlvorKit;

/// <summary>Selects the dynamic and interception capabilities used by this fixture.</summary>
[TestClass]
public sealed class MockInterceptionTestAssembly
{
    /// <summary>Enables proxy, callback, and concrete-operation interception support.</summary>
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        MockDynamic.Enable();
        MockInterception.Enable();
    }
}
