namespace AlvorKit.Script.Bindgen.CHeaders.Test;

/// <summary>Covers pointer-shaped public signatures for transparent native records.</summary>
[TestClass]
public sealed class CHeaderBindingParserPointerTypeTest
{
    /// <summary>Transparent record typedef pointers remain raw pointers instead of being erased to nint.</summary>
    [TestMethod]
    public void Parse_TransparentRecordPointersUseRawPointerTypes()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_face_rec_ {
                int value;
            } test_face_rec;
            typedef test_face_rec* test_face;
            typedef struct test_slot_rec_ {
                test_face face;
            } test_slot_rec;
            void test_load(test_face face);
            test_face test_current(void);
            """);
        var config = CHeaderTestConfig.Create();
        config.TransparentStructs = ["test_face_rec", "test_slot_rec"];

        var model = CHeaderParserHarness.Parse(translationUnit, source, config);
        var load = model.Functions.Single(function => function.NativeName == "test_load");
        var current = model.Functions.Single(function => function.NativeName == "test_current");
        var slot = model.Structs.Single(bindingStruct => bindingStruct.ManagedName == "TestSlotRec");

        Assert.AreEqual("TestFaceRec*", load.Parameters.Single().ManagedType);
        Assert.AreEqual("TestFaceRec*", load.Parameters.Single().InteropType);
        Assert.AreEqual("TestFaceRec*", current.ReturnType);
        Assert.AreEqual("TestFaceRec*", current.ReturnInteropType);
        Assert.AreEqual("TestFaceRec*", slot.Fields.Single(field => field.ManagedName == "Face").ManagedType);
    }
}
