using System.Xml.Linq;

namespace AlvorKit.Script.Bindgen;

/// <summary>Parses configured OpenGL callback function-pointer typedefs from registry type text.</summary>
/// <param name="config">Bindgen configuration containing callback typedefs.</param>
internal sealed class GlCallbackTypedefParser(BindgenConfig config)
{
    /// <summary>Parses configured callback typedefs from raw registry type text.</summary>
    public GlCallbackDefinitions Parse(XElement registry)
    {
        if (config.Callbacks.Count == 0)
            return new(new Dictionary<string, GlCallbackSignature>(), new Dictionary<string, string>());

        var typeElements = registry.Elements("types").Elements("type")
            .Where(type => type.Element("name") is not null)
            .GroupBy(type => type.Element("name")!.Value)
            .ToDictionary(group => group.Key, group => group.First());
        var signatures = new Dictionary<string, GlCallbackSignature>();
        var managedNames = new Dictionary<string, string>();

        foreach (var (nativeName, callback) in config.Callbacks)
        {
            if (!typeElements.TryGetValue(nativeName, out var element))
                throw new InvalidOperationException($"Callback typedef {nativeName} is not a <type> in the registry.");
            managedNames[nativeName] = callback.ManagedName;
            signatures[nativeName] = ParseFunctionPointerTypedef(element.Value, nativeName);
        }
        return new(signatures, managedNames);
    }

    /// <summary>Extracts return type, typedef name, and parameters from a function-pointer typedef.</summary>
    private static GlCallbackSignature ParseFunctionPointerTypedef(string text, string nativeName)
    {
        const string keyword = "typedef";
        var firstParen = text.IndexOf('(');
        var open = text.LastIndexOf('(');
        var close = text.LastIndexOf(')');
        if (firstParen < 0 || open <= firstParen || close <= open || !text.Contains(keyword))
            throw new InvalidOperationException($"{nativeName} is not a function-pointer typedef: '{text.Trim()}'.");

        var returnType = text[(text.IndexOf(keyword) + keyword.Length)..firstParen].Trim();
        var parameters = text[(open + 1)..close]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => part != "void")
            .Select(SplitCallbackParameter)
            .ToList();
        return new(returnType, parameters);
    }

    /// <summary>Splits a callback parameter declaration into type and terminal identifier.</summary>
    private static GlCallbackParameterSignature SplitCallbackParameter(string part)
    {
        var split = part.Length;
        while (split > 0 && (char.IsLetterOrDigit(part[split - 1]) || part[split - 1] == '_'))
            split--;
        return new(part[..split].Trim(), part[split..]);
    }
}
