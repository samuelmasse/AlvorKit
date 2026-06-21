namespace AlvorKit.Script.Bindgen;

/// <summary>Emits xxHash-specific convenience overloads for owned secrets and native-layout helpers.</summary>
internal static class BindingXxHashOverloadEmitter
{
    /// <summary>Emits xxHash convenience overloads that keep native layout and lifetime details out of user code.</summary>
    public static void XxHashOverloads(StringBuilder output, BindingModel model, string apiClass)
    {
        foreach (var function in model.Functions.Where(IsSecretReset))
            SecretResetOverload(output, function, apiClass);
        if (model.Functions.FirstOrDefault(function => function.NativeName == "XXH128_cmp") is { } compare)
            Compare128Overload(output, compare, apiClass);
    }

    /// <summary>Returns true for xxHash streaming reset functions that retain a secret pointer.</summary>
    private static bool IsSecretReset(BindingFunction function) =>
        function.NativeName
            is "XXH3_64bits_reset_withSecret"
            or "XXH3_64bits_reset_withSecretandSeed"
            or "XXH3_128bits_reset_withSecret"
            or "XXH3_128bits_reset_withSecretandSeed";

    /// <summary>Emits one overload that forwards an owned <c>XxhSecret</c> into the native-shaped method.</summary>
    private static void SecretResetOverload(StringBuilder output, BindingFunction function, string apiClass)
    {
        var state = function.Parameters[0];
        var seed = function.Parameters.Count > 3 ? function.Parameters[3] : null;
        var signature = seed is null
            ? $"{state.ManagedType} {state.ManagedName}, XxhSecret secret"
            : $"{state.ManagedType} {state.ManagedName}, XxhSecret secret, {seed.ManagedType} {seed.ManagedName}";
        var arguments = seed is null
            ? $"{state.ManagedName}, secret.Pointer, secret.Size"
            : $"{state.ManagedName}, secret.Pointer, secret.Size, {seed.ManagedName}";

        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})",
            function.NativeName,
            "Uses owned native secret memory from <see cref=\"XxhSecret\"/>. Do not dispose the secret until the streaming state "
            + "is reset again, freed, or no longer used.");
        output.AppendLine($"    public {function.ReturnType} {function.ManagedName}({signature}) =>");
        output.AppendLine($"        {function.ManagedName}({arguments});");
        output.AppendLine();
    }

    /// <summary>Emits a managed 128-bit comparison overload over the pointer-shaped C comparator.</summary>
    private static void Compare128Overload(StringBuilder output, BindingFunction function, string apiClass)
    {
        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})",
            function.NativeName,
            "Converts managed <see cref=\"UInt128\"/> values to the native xxHash layout before comparing.");
        output.AppendLine($"    public unsafe {function.ReturnType} {function.ManagedName}(UInt128 left, UInt128 right)");
        output.AppendLine("    {");
        output.AppendLine("        var leftHash = Xxh128Hash.FromUInt128(left);");
        output.AppendLine("        var rightHash = Xxh128Hash.FromUInt128(right);");
        output.AppendLine($"        return {function.ManagedName}((nint)(&leftHash), (nint)(&rightHash));");
        output.AppendLine("    }");
        output.AppendLine();
    }
}
