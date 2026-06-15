using ClangSharp.Interop;
using System.Reflection;

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

    /// <summary>Unconfigured forward-declared record pointers remain raw native handles.</summary>
    [TestMethod]
    public void Parse_UnconfiguredForwardRecordPointerUsesNint()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_legacy test_legacy;
            void test_use(test_legacy* legacy);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source, CHeaderTestConfig.Create());

        Assert.AreEqual("nint", ParameterType(model, "test_use"));
    }

    /// <summary>Record pointers whose pointee cannot be emitted fall back to raw handles.</summary>
    [TestMethod]
    public void Parse_UnsupportedRecordPointerUsesNint()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef struct test_bad {
                long double value;
            } test_bad;
            void test_use_bad(test_bad* bad);
            test_bad test_bad_value(void);
            long double test_bad_scalar(void);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source, CHeaderTestConfig.Create());

        Assert.AreEqual("nint", ParameterType(model, "test_use_bad"));
        CollectionAssert.Contains(model.SkippedFunctions, "test_bad_value (return type: test_bad)");
        CollectionAssert.Contains(model.SkippedFunctions, "test_bad_scalar (return type: long double)");
    }

    /// <summary>Primitive scalar returns keep precise managed types where C and C# have direct equivalents.</summary>
    [TestMethod]
    public void Parse_PrimitiveScalarReturnsUseManagedPrimitiveTypes()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            typedef enum test_mode {
                test_MODE_A = 1
            } test_mode;
            typedef int test_bool;
            void test_void(void);
            test_mode test_current_mode(void);
            test_bool test_flag(void);
            _Bool test_native_bool(void);
            unsigned char test_uchar(void);
            signed char test_schar(void);
            unsigned short test_ushort(void);
            short test_short(void);
            unsigned int test_uint(void);
            int test_int(void);
            unsigned long test_ulong(void);
            long test_long(void);
            unsigned long long test_ulonglong(void);
            long long test_longlong(void);
            float test_float(void);
            double test_double(void);
            void test_array(int values[4]);
            """);

        var model = CHeaderParserHarness.Parse(translationUnit, source, CHeaderTestConfig.Create());

        Assert.AreEqual("void", ReturnType(model, "test_void"));
        Assert.AreEqual("TestMode", ReturnType(model, "test_current_mode"));
        Assert.AreEqual("bool", ReturnType(model, "test_flag"));
        Assert.AreEqual("bool", ReturnType(model, "test_native_bool"));
        Assert.AreEqual("byte", ReturnType(model, "test_uchar"));
        Assert.AreEqual("sbyte", ReturnType(model, "test_schar"));
        Assert.AreEqual("ushort", ReturnType(model, "test_ushort"));
        Assert.AreEqual("short", ReturnType(model, "test_short"));
        Assert.AreEqual("uint", ReturnType(model, "test_uint"));
        Assert.AreEqual("int", ReturnType(model, "test_int"));
        Assert.AreEqual("CULong", ReturnType(model, "test_ulong"));
        Assert.AreEqual("CLong", ReturnType(model, "test_long"));
        Assert.AreEqual("ulong", ReturnType(model, "test_ulonglong"));
        Assert.AreEqual("long", ReturnType(model, "test_longlong"));
        Assert.AreEqual("float", ReturnType(model, "test_float"));
        Assert.AreEqual("double", ReturnType(model, "test_double"));
        Assert.AreEqual("nint", ParameterType(model, "test_array"));
    }

    /// <summary>Enum integer kinds map to the nearest managed underlying type.</summary>
    [TestMethod]
    public void MapIntegerKind_UsesManagedEnumUnderlyingTypes()
    {
        Assert.AreEqual("byte", MapIntegerKind(CXTypeKind.CXType_UChar));
        Assert.AreEqual("byte", MapIntegerKind(CXTypeKind.CXType_Char_U));
        Assert.AreEqual("sbyte", MapIntegerKind(CXTypeKind.CXType_SChar));
        Assert.AreEqual("sbyte", MapIntegerKind(CXTypeKind.CXType_Char_S));
        Assert.AreEqual("ushort", MapIntegerKind(CXTypeKind.CXType_UShort));
        Assert.AreEqual("short", MapIntegerKind(CXTypeKind.CXType_Short));
        Assert.AreEqual("uint", MapIntegerKind(CXTypeKind.CXType_UInt));
        Assert.AreEqual("ulong", MapIntegerKind(CXTypeKind.CXType_ULongLong));
        Assert.AreEqual("long", MapIntegerKind(CXTypeKind.CXType_LongLong));
        Assert.AreEqual("int", MapIntegerKind(CXTypeKind.CXType_Int));
    }

    /// <summary>Primitive pointer returns keep their pointed type when they are not C strings.</summary>
    [TestMethod]
    public void Parse_PrimitivePointerReturnsUseRawPointerTypes()
    {
        using var workspace = TempWorkspace.Create();
        var source = workspace.CreateDirectory("source");
        var translationUnit = CHeaderParserHarness.WriteHeader(workspace, source, """
            _Bool* test_bools(void);
            unsigned short* test_ushorts(void);
            short* test_shorts(void);
            unsigned int* test_values(void);
            int* test_ints(void);
            unsigned long* test_ulongs(void);
            long* test_longs(void);
            unsigned long long* test_ulonglongs(void);
            long long* test_longlongs(void);
            float* test_floats(void);
            double* test_doubles(void);
            const char* test_name(void);
            void test_string(const char* name);
            void test_bytes(signed char* bytes);
            typedef struct test_holder {
                unsigned int* values;
            } test_holder;
            """);
        var config = CHeaderTestConfig.Create();
        config.TransparentStructs = ["test_holder"];
        var model = CHeaderParserHarness.Parse(translationUnit, source, config);

        Assert.AreEqual("bool*", ReturnType(model, "test_bools"));
        Assert.AreEqual("ushort*", ReturnType(model, "test_ushorts"));
        Assert.AreEqual("short*", ReturnType(model, "test_shorts"));
        Assert.AreEqual("uint*", ReturnType(model, "test_values"));
        Assert.AreEqual("int*", ReturnType(model, "test_ints"));
        Assert.AreEqual("CULong*", ReturnType(model, "test_ulongs"));
        Assert.AreEqual("CLong*", ReturnType(model, "test_longs"));
        Assert.AreEqual("ulong*", ReturnType(model, "test_ulonglongs"));
        Assert.AreEqual("long*", ReturnType(model, "test_longlongs"));
        Assert.AreEqual("float*", ReturnType(model, "test_floats"));
        Assert.AreEqual("double*", ReturnType(model, "test_doubles"));
        Assert.AreEqual("nint", ReturnType(model, "test_name"));
        Assert.IsTrue(Parameter(model, "test_string").HasStringConvenience);
        Assert.AreEqual("nint", Parameter(model, "test_string").ManagedType);
        Assert.AreEqual("nint", ParameterType(model, "test_bytes"));
        Assert.AreEqual("nint", model.Structs.Single(type => type.NativeName == "test_holder").Fields.Single().ManagedType);
    }

    private static string ReturnType(BindingModel model, string nativeName) =>
        model.Functions.Single(function => function.NativeName == nativeName).ReturnType;

    private static string ParameterType(BindingModel model, string nativeName) =>
        Parameter(model, nativeName).ManagedType;

    private static BindingParameter Parameter(BindingModel model, string nativeName) =>
        model.Functions.Single(function => function.NativeName == nativeName).Parameters.Single();

    private static string MapIntegerKind(CXTypeKind kind)
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .Single(candidate => candidate.GetName().Name == "AlvorKit.Script.Bindgen.CHeaders");
        var mapper = assembly.GetType("AlvorKit.Script.Bindgen.CHeaderTypeMapper")!;
        var method = mapper.GetMethod("MapIntegerKind", BindingFlags.Static | BindingFlags.NonPublic)!;
        return (string)method.Invoke(null, [kind])!;
    }
}
