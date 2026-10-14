namespace AlvorKit;

/// <summary>Compares generated managed layouts with sizeof and offsetof values from the real native compiler.</summary>
internal static class Program
{
    /// <summary>Runs layout verification without loading a native binding backend or touching an audio device.</summary>
    private static int Main(string[] args)
    {
        var assembly = new Option<string>("--assembly") { Required = true, Description = "Managed binding DLL." };
        var native = new Option<string>("--native") { Required = true, Description = "Native library with ABI probes." };
        var manifest = new Option<string>("--manifest") { Required = true, Description = "Generated ABI manifest JSON." };
        var command = new RootCommand("Verify generated managed layouts against native compiler values.");
        command.Options.Add(assembly);
        command.Options.Add(native);
        command.Options.Add(manifest);
        command.SetAction(result => Verify(result.GetRequiredValue(assembly),
            result.GetRequiredValue(native), result.GetRequiredValue(manifest)));
        return command.Parse(args).Invoke();
    }

    /// <summary>Fails on missing records, missing fields, count mismatches, or any differing managed layout.</summary>
    private static unsafe void Verify(string assemblyPath, string nativePath, string manifestPath)
    {
        var assembly = Assembly.LoadFrom(Path.GetFullPath(assemblyPath));
        var entries = JsonSerializer.Deserialize<AbiEntry[]>(File.ReadAllText(manifestPath))!;
        var native = NativeLibrary.Load(Path.GetFullPath(nativePath));

        try
        {
            var count = (delegate* unmanaged[Cdecl]<nuint>)NativeLibrary.GetExport(native, "alvorkit_miniaudio_abi_count");
            var value = (delegate* unmanaged[Cdecl]<nuint, nuint>)NativeLibrary.GetExport(native, "alvorkit_miniaudio_abi_value");

            if (count() != (nuint)entries.Length)
                throw new InvalidOperationException($"ABI manifest has {entries.Length} checks; native library has {count()}.");

            for (var index = 0; index < entries.Length; index++)
            {
                var entry = entries[index];
                var type = assembly.GetType("AlvorKit." + entry.Type, true)!;
                var actual = entry.Field.Length == 0 ? Marshal.SizeOf(type) : FieldOffset(type, entry.Field);
                var expected = value((nuint)index);

                if ((nuint)actual != expected)
                    throw new InvalidOperationException($"{entry.Type}.{entry.Field}: managed {actual}, native {expected}.");
            }

            Console.WriteLine($"PASS {RuntimeInformation.RuntimeIdentifier}: {entries.Length} native size and field-offset checks.");
        }
        finally
        {
            NativeLibrary.Free(native);
        }
    }

    /// <summary>Computes the offset of a nested managed field without dereferencing any native pointers.</summary>
    private static int FieldOffset(Type type, string path)
    {
        var offset = 0;

        foreach (var name in path.Split('.'))
        {
            offset += (int)Marshal.OffsetOf(type, name);
            type = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.FieldType;
        }

        return offset;
    }
}
