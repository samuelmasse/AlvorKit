namespace AlvorKit;

/// <summary>Captures the optional typed matcher used by a field-write contract.</summary>
internal static class MockFieldPatternCapture
{
    /// <summary>Returns the validated field patterns for the requested operation.</summary>
    internal static MockArgumentPattern[] Capture<T>(
        MockInvocationOperationKind operationKind,
        Func<T>? value)
        where T : allows ref struct
    {
        if (operationKind == MockInvocationOperationKind.FieldRead)
        {
            if (value is not null)
            {
                throw new MockException(
                    "A field-read contract cannot capture a write value.");
            }

            return [];
        }
        if (operationKind != MockInvocationOperationKind.FieldWrite ||
            value is null)
        {
            throw new MockException(
                "A field-write contract requires one typed value lambda.");
        }

        global::AlvorKit.Capture.Start(CaptureOperation.Setup);
        try
        {
            object? ordinary = null;
            if (typeof(T).IsByRefLike)
                _ = value();
            else
                ordinary = value.DynamicInvoke();

            int matcherCount =
                global::AlvorKit.Capture.FirstMatchers.Count +
                global::AlvorKit.Capture.FirstIndexedMatchers.Count;
            if (matcherCount > 1)
            {
                throw new MockException(
                    "A field-write value can contain only one matcher.");
            }
            if (global::AlvorKit.Capture.FirstIndexedMatchers.Count == 1)
            {
                MockIndexedMatcher indexed =
                    global::AlvorKit.Capture.FirstIndexedMatchers[0];
                if (indexed.DeclaredIndex != 0 ||
                    indexed.ValueType != typeof(T) ||
                    indexed.PassingKind !=
                        MockIndexedMatcherPassingKind.Value)
                {
                    throw new MockException(
                        "A field-write indexed matcher must target value " +
                        "parameter 0 with the exact field type.");
                }

                return [new(indexed.Matcher)];
            }
            if (global::AlvorKit.Capture.FirstMatchers.Count == 1)
            {
                return
                [
                    new(global::AlvorKit.Capture.FirstMatchers[0])
                ];
            }
            if (typeof(T).IsByRefLike)
            {
                throw new MockException(
                    "A byref-like field-write value requires an indexed " +
                    "matcher at parameter 0.");
            }

            return [new(ordinary)];
        }
        catch (TargetInvocationException exception)
            when (exception.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo
                .Capture(exception.InnerException)
                .Throw();
            throw new UnreachableException();
        }
        finally
        {
            global::AlvorKit.Capture.End();
        }
    }
}
