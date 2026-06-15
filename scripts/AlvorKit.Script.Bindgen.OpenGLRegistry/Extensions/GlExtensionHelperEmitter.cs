namespace AlvorKit.Script.Bindgen;

/// <summary>Emits shared helper members used by generated OpenGL extension overloads.</summary>
internal static class GlExtensionHelperEmitter
{
    /// <summary>Appends shared helper members to the generated partial API class.</summary>
    public static void Append(StringBuilder output)
    {
        output.AppendLine("    /// <summary>Returns the byte length of a span of unmanaged elements.</summary>");
        output.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        output.AppendLine("    private static nint ByteLength<T>(ReadOnlySpan<T> span) where T : unmanaged =>");
        output.AppendLine("        checked((nint)((nuint)span.Length * (nuint)sizeof(T)));");
        output.AppendLine();
        AppendUtf8(output);
        output.AppendLine();
        AppendUtf8Array(output);
    }

    /// <summary>Appends the generated UTF-8 single-string helper.</summary>
    private static void AppendUtf8(StringBuilder output)
    {
        output.AppendLine("    /// <summary>A NUL-terminated UTF-8 copy of a string for interop.</summary>");
        output.AppendLine("    private readonly ref struct Utf8");
        output.AppendLine("    {");
        output.AppendLine("        /// <summary>Native heap allocation when the stack buffer is too small.</summary>");
        output.AppendLine("        private readonly void* native;");
        output.AppendLine("        /// <summary>Pointer to the NUL-terminated UTF-8 bytes.</summary>");
        output.AppendLine("        public readonly nint Pointer;");
        output.AppendLine("        /// <summary>Byte length excluding the NUL terminator.</summary>");
        output.AppendLine("        public readonly int Length;");
        output.AppendLine();
        output.AppendLine("        /// <summary>Encodes a managed string into stack or native NUL-terminated UTF-8 storage.</summary>");
        output.AppendLine("        public Utf8(string text, Span<byte> stack)");
        output.AppendLine("        {");
        output.AppendLine("            Length = Encoding.UTF8.GetByteCount(text);");
        output.AppendLine("            native = Length < stack.Length ? null : NativeMemory.Alloc((nuint)(Length + 1));");
        output.AppendLine("            var buffer = native != null ? new Span<byte>(native, Length + 1) : stack;");
        output.AppendLine("            Encoding.UTF8.GetBytes(text, buffer);");
        output.AppendLine("            buffer[Length] = 0;");
        output.AppendLine("            Pointer = (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));");
        output.AppendLine("        }");
        output.AppendLine();
        output.AppendLine("        /// <summary>Releases any native heap storage used by this helper.</summary>");
        output.AppendLine("        public void Dispose() => NativeMemory.Free(native);");
        output.AppendLine("    }");
    }

    /// <summary>Appends the generated UTF-8 string-array helper.</summary>
    private static void AppendUtf8Array(StringBuilder output)
    {
        output.AppendLine("    /// <summary>An array of NUL-terminated UTF-8 strings plus a char** pointer array.</summary>");
        output.AppendLine("    private readonly ref struct Utf8Array");
        output.AppendLine("    {");
        output.AppendLine("        /// <summary>Native heap allocation when the stack buffer is too small.</summary>");
        output.AppendLine("        private readonly void* native;");
        output.AppendLine("        /// <summary>Pointer to the array of string pointers.</summary>");
        output.AppendLine("        public readonly nint Pointers;");
        output.AppendLine();
        output.AppendLine("        /// <summary>Encodes managed strings into stack or native NUL-terminated UTF-8 storage.</summary>");
        output.AppendLine("        public Utf8Array(ReadOnlySpan<string> strings, Span<nint> stack)");
        output.AppendLine("        {");
        output.AppendLine("            var total = 0;");
        output.AppendLine("            for (var i = 0; i < strings.Length; i++)");
        output.AppendLine("                total += Encoding.UTF8.GetByteCount(strings[i]) + 1;");
        output.AppendLine("            var size = strings.Length * sizeof(nint) + total;");
        output.AppendLine("            native = size <= stack.Length * sizeof(nint) ? null : NativeMemory.Alloc((nuint)size);");
        output.AppendLine("            var basePtr = native != null ? (byte*)native : (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(stack));");
        output.AppendLine("            var pointers = (byte**)basePtr;");
        output.AppendLine("            var data = basePtr + strings.Length * sizeof(nint);");
        output.AppendLine("            var remaining = total;");
        output.AppendLine("            for (var i = 0; i < strings.Length; i++)");
        output.AppendLine("            {");
        output.AppendLine("                pointers[i] = data;");
        output.AppendLine("                var written = Encoding.UTF8.GetBytes(strings[i], new Span<byte>(data, remaining));");
        output.AppendLine("                data[written] = 0;");
        output.AppendLine("                data += written + 1;");
        output.AppendLine("                remaining -= written + 1;");
        output.AppendLine("            }");
        output.AppendLine("            Pointers = (nint)basePtr;");
        output.AppendLine("        }");
        output.AppendLine();
        output.AppendLine("        /// <summary>Releases any native heap storage used by this helper.</summary>");
        output.AppendLine("        public void Dispose() => NativeMemory.Free(native);");
        output.AppendLine("    }");
    }
}
