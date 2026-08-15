namespace AlvorKit;

/// <summary>Identifies the intercepted operation represented by an invocation.</summary>
internal enum MockInvocationOperationKind
{
    /// <summary>An instance method or accessor call.</summary>
    InstanceMethod,

    /// <summary>A static method or accessor call.</summary>
    StaticMethod,

    /// <summary>An object-construction call site.</summary>
    Construction,

    /// <summary>An intercepted constructor body.</summary>
    ConstructorBody,

    /// <summary>A field-read call site.</summary>
    FieldRead,

    /// <summary>A field-write call site.</summary>
    FieldWrite,

    /// <summary>A value-type instance call with a live managed receiver.</summary>
    StructMethod
}
