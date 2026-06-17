namespace AlvorKit.Mocking;

internal static partial class ReturnValues
{
    /// <summary>Finds a configured value for a call and writes configured reference outputs when needed.</summary>
    internal static bool Get(Mocked mocked, MethodInfo method, object?[] vargs, object?[] args, out object? rval)
    {
        if (!mocked.HasReturnValues || !mocked.ReturnValues.TryGetValue(method, out var val))
            return GetFallback(mocked, method, out rval);

        lock (mocked)
        {
            var indices = Indices.RefParameterIndices(mocked.Type, method);

            for (int i = val.Count - 1; i >= 0; i--)
            {
                if (IsMatch(val[i].Item2, vargs))
                {
                    rval = val[i].Item1;
                    WriteRefs(val[i].Item3, indices, args);
                    return true;
                }
            }
        }

        return GetFallback(mocked, method, out rval);
    }

    /// <summary>Returns true when configured arguments match actual call arguments.</summary>
    internal static bool IsMatch(object?[] target, object?[] actual)
    {
        if (target.Length != actual.Length)
            return false;

        for (int i = 0; i < target.Length; i++)
        {
            if (!IsArgumentMatch(target[i], actual[i]))
                return false;
        }

        return true;
    }

    /// <summary>Returns the partial-mock passthrough or full-mock default fallback result.</summary>
    private static bool GetFallback(Mocked mocked, MethodInfo method, out object? rval)
    {
        if (mocked.Partial)
        {
            rval = null;
            return false;
        }

        rval = GetDefault(mocked, method);
        return true;
    }

    /// <summary>Writes configured reference outputs into the active argument array.</summary>
    private static void WriteRefs(object?[] refs, int[] indices, object?[] args)
    {
        if (refs.Length == 0)
            return;

        for (int j = 0; j < indices.Length; j++)
            args[indices[j]] = refs[j];
    }

    /// <summary>Returns true when one configured argument matches one actual argument.</summary>
    private static bool IsArgumentMatch(object? target, object? actual)
    {
        if (target == null || actual == null)
            return target == null && actual == null;

        if (target is Matcher matcher)
            return matcher.Type == MatcherType.Any || ((Func<object, bool>)matcher.Object!).Invoke(actual);

        return target.Equals(actual);
    }
}
