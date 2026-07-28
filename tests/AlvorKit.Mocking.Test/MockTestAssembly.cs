namespace AlvorKit.Mocking.Test;

/// <summary>Selects the optional JIT backend for the dynamic integration suite.</summary>
[TestClass]
public sealed class MockTestAssembly
{
    /// <summary>Enables the dynamic backend before any integration test runs.</summary>
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        MockDynamic.Enable();
        MockInterception.Enable();
    }
}
