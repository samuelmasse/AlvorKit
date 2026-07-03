namespace AlvorKit.UI;

internal static class UiDebug
{
    public static string Format<T>(T val) => val switch
    {
        null => "null",
        string s => $"\"{s}\"",
        bool b => b ? "true" : "false",
        _ when typeof(T).IsPrimitive => $"{val}",
        _ => $"{{{val}}}"
    };

    public static string FormatDelegate(Delegate? del)
    {
        if (del is null)
            return "null";

        var m = del.Method;
        var ps = m.GetParameters();
        var args = ps.Length == 0 ? "" : string.Join(", ", ps.Select(p => FormatType(p.ParameterType)));
        var ret = m.ReturnType == typeof(void) ? "Void" : FormatType(m.ReturnType);
        return $"{{Method = {{{ret} {m.Name}({args})}}}}";
    }

    private static string FormatType(Type t)
    {
        if (!t.IsGenericType)
            return t.FullName ?? t.Name;

        var name = t.GetGenericTypeDefinition().FullName ?? t.Name;
        var typeArgs = string.Join(",", t.GetGenericArguments().Select(a => a.FullName ?? a.Name));
        return $"{name}[{typeArgs}]";
    }
}
