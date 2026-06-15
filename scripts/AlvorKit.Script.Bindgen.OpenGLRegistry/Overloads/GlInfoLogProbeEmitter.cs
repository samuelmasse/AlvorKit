namespace AlvorKit.Script.Bindgen;

/// <summary>Emits shared probe-and-grow bodies for OpenGL info-log helpers.</summary>
internal static class GlInfoLogProbeEmitter
{
    /// <summary>Emits the shared probe-and-grow body used by string-returning info-log helpers.</summary>
    public static void Emit(StringBuilder output, GlCommand command, string coreArgs, params string[] onComplete)
    {
        output.AppendLine("    {");
        output.AppendLine("        Span<byte> buffer = stackalloc byte[1024];");
        output.AppendLine("        void* native = null;");
        output.AppendLine("        try");
        output.AppendLine("        {");
        output.AppendLine("            while (true)");
        output.AppendLine("            {");
        output.AppendLine("                int written;");
        output.AppendLine("                fixed (byte* bufferPtr = buffer)");
        output.AppendLine($"                    this.{command.ManagedName}({coreArgs});");
        output.AppendLine("                if (written < buffer.Length - 1)");
        output.AppendLine("                {");
        foreach (var line in onComplete)
            output.AppendLine(line);
        output.AppendLine("                }");
        output.AppendLine("                var size = buffer.Length * 2;");
        output.AppendLine("                NativeMemory.Free(native);");
        output.AppendLine("                native = NativeMemory.Alloc((nuint)size);");
        output.AppendLine("                buffer = new Span<byte>(native, size);");
        output.AppendLine("            }");
        output.AppendLine("        }");
        output.AppendLine("        finally");
        output.AppendLine("        {");
        output.AppendLine("            NativeMemory.Free(native);");
        output.AppendLine("        }");
        output.AppendLine("    }");
        output.AppendLine();
    }
}
