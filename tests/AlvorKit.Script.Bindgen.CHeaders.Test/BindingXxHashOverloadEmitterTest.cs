namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingXxHashOverloadEmitterTest
{
    [TestMethod]
    public void Emit_XxHashConvenienceAddsSecretTypeResetOverloadsAndCompareOverload()
    {
        using var workspace = TempWorkspace.Create();
        var config = CHeaderTestConfig.Create();
        config.ApiClass = "Xxh";
        config.XxHashConvenience = true;
        var model = new BindingModel(
            Enums: [],
            Structs: [],
            Handles: [new("XXH3_state_s", "Xxh3State")],
            Delegates: [],
            Functions:
            [
                ResetWithSecret("XXH3_64bits_reset_withSecret", "ResetHash3To64", seeded: false),
                ResetWithSecret("XXH3_64bits_reset_withSecretandSeed", "ResetHash3To64", seeded: true),
                ResetWithSecret("XXH3_128bits_reset_withSecret", "ResetHash3To128", seeded: false),
                ResetWithSecret("XXH3_128bits_reset_withSecretandSeed", "ResetHash3To128", seeded: true),
                CompareHash128(),
            ],
            SkippedFunctions: [],
            SizeofTypes: []);

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var secretSource = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "XxhSecret.cs"));
        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "XxhOverloads.cs"));
        StringAssert.Contains(secretSource, "<c>XXH3_64bits_reset_withSecret</c>");
        StringAssert.Contains(secretSource, "<c>XXH3_SECRET_SIZE_MIN</c>");
        StringAssert.Contains(secretSource, "<c>XXH3_generateSecret</c>");
        StringAssert.Contains(secretSource, "public sealed class XxhSecret : IDisposable");
        StringAssert.Contains(secretSource, "public unsafe XxhSecret(nuint size)");
        StringAssert.Contains(secretSource, "if (size < (nuint)(long)XxhEnum.Xxh3SecretSizeMin)");
        StringAssert.Contains(secretSource, "if (size > int.MaxValue)");
        StringAssert.Contains(secretSource, "Pointer = (nint)NativeMemory.AllocZeroed(size);");
        StringAssert.Contains(secretSource, "throw new OutOfMemoryException");
        Assert.IsFalse(secretSource.Contains("public XxhSecret(nint pointer, nuint size)", StringComparison.Ordinal));
        StringAssert.Contains(secretSource, "public unsafe Span<byte> Bytes");
        StringAssert.Contains(secretSource, "NativeMemory.Free((void*)Pointer);");
        StringAssert.Contains(overloads, "public unsafe partial class Xxh");
        StringAssert.Contains(overloads, "public XxhErrorCode ResetHash3To64(Xxh3State statePtr, XxhSecret secret) =>");
        StringAssert.Contains(overloads, "ResetHash3To64(statePtr, secret.Pointer, secret.Size);");
        StringAssert.Contains(overloads, "public XxhErrorCode ResetHash3To128(Xxh3State statePtr, XxhSecret secret, ulong seed64) =>");
        StringAssert.Contains(overloads, "ResetHash3To128(statePtr, secret.Pointer, secret.Size, seed64);");
        StringAssert.Contains(overloads, "Do not dispose the secret until the streaming state");
        StringAssert.Contains(overloads, "public unsafe int CompareHash128(UInt128 left, UInt128 right)");
        StringAssert.Contains(overloads, "var leftHash = Xxh128Hash.FromUInt128(left);");
        StringAssert.Contains(overloads, "return CompareHash128((nint)(&leftHash), (nint)(&rightHash));");
    }

    /// <summary>Builds an xxHash reset-with-secret fixture function.</summary>
    private static BindingFunction ResetWithSecret(string nativeName, string managedName, bool seeded)
    {
        var parameters = new List<BindingParameter>
        {
            new("statePtr", "Xxh3State", "Xxh3State", "", HasStringConvenience: false),
            new("secret", "nint", "nint", "", HasStringConvenience: false),
            new("secretSize", "nuint", "nuint", "", HasStringConvenience: false),
        };
        if (seeded)
            parameters.Add(new("seed64", "ulong", "ulong", "", HasStringConvenience: false));

        return new(nativeName, managedName, "XxhErrorCode", "XxhErrorCode", parameters, Documentation: null);
    }

    /// <summary>Builds the xxHash 128-bit comparator fixture function.</summary>
    private static BindingFunction CompareHash128() =>
        new(
            "XXH128_cmp",
            "CompareHash128",
            "int",
            "int",
            [
                new("h128_1", "nint", "nint", "", HasStringConvenience: false),
                new("h128_2", "nint", "nint", "", HasStringConvenience: false),
            ],
            Documentation: null);
}
