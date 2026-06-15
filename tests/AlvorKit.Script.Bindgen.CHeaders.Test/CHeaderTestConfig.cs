namespace AlvorKit.Script.Bindgen.CHeaders.Test;

internal static class CHeaderTestConfig
{
    public static BindgenConfig Create() => new()
    {
        Namespace = "AlvorKit.Bindgen.Fixture",
        ApiClass = "Test",
        ApiSummary = "Fixture API.",
        BackendClass = "TestBackend",
        NativeClass = "TestNative",
        NativeLibrary = "fixture",
        Prefix = "test_",
        WorkDir = "fixture-work",
        SourceDir = "fixture-source",
        Header = "fixture.h",
        ApiProject = "generated/Fixture",
        BackendProject = "generated/Fixture.Backend"
    };
}
