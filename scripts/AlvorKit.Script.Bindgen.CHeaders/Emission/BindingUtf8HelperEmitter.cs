namespace AlvorKit.Script.Bindgen;

/// <summary>Emits the UTF-8 helper used by generated string overloads.</summary>
internal static class BindingUtf8HelperEmitter
{
    /// <summary>Emits the stack-first UTF-8 ref struct helper.</summary>
    public static void Utf8Helper(StringBuilder output)
    {
        output.AppendLine();
        output.AppendLine("    /// <summary>A NUL-terminated UTF-8 copy of a string for interop.</summary>");
        output.AppendLine("    private readonly unsafe ref struct Utf8");
        output.AppendLine("    {");
        output.AppendLine("        /// <summary>Native heap allocation when the stack buffer is too small.</summary>");
        output.AppendLine("        private readonly void* native;");
        output.AppendLine();
        output.AppendLine("        /// <summary>Pointer to the NUL-terminated UTF-8 bytes.</summary>");
        output.AppendLine("        public readonly nint Pointer;");
        output.AppendLine();
        output.AppendLine("        /// <summary>Copies a managed string into NUL-terminated UTF-8 storage.</summary>");
        output.AppendLine("        public Utf8(string text, Span<byte> stack)");
        output.AppendLine("        {");
        output.AppendLine("            var length = Encoding.UTF8.GetByteCount(text);");
        output.AppendLine("            native = length < stack.Length ? null : NativeMemory.Alloc((nuint)(length + 1));");
        output.AppendLine("            var buffer = native != null ? new Span<byte>(native, length + 1) : stack;");
        output.AppendLine("            Encoding.UTF8.GetBytes(text, buffer);");
        output.AppendLine("            buffer[length] = 0;");
        output.AppendLine("            Pointer = (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));");
        output.AppendLine("        }");
        output.AppendLine();
        output.AppendLine("        /// <summary>Frees native heap storage when it was allocated.</summary>");
        output.AppendLine("        public void Dispose() => NativeMemory.Free(native);");
        output.AppendLine("    }");
    }
}
