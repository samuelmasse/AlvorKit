namespace AlvorKit.Script.MathsGen;

/// <summary>Emits vector functions and derived properties.</summary>
internal static class FunctionEmitter
{
    /// <summary>Appends functions for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        ValueSemanticsEmitter.Emit(vector, members);
        if (vector.Scalar.IsBool)
        {
            BoolFunctionsEmitter.Emit(vector, members);
            return;
        }

        NumericFunctionsEmitter.Emit(vector, members);
        RelationalEmitter.Emit(vector, members);
        if (vector.Scalar.IsInteger)
            BitFunctionEmitter.Emit(vector, members);
    }
}
