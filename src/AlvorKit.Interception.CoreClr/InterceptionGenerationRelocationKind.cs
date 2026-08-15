namespace AlvorKit;

/// <summary>Metadata token kinds supported by native late relocation.</summary>
public enum InterceptionGenerationRelocationKind
{
    /// <summary>A StandAloneSig token used by calli or local storage.</summary>
    StandaloneSignature = 1,

    /// <summary>A TypeSpec token created from an exact ECMA signature blob.</summary>
    TypeSpec = 2,

    /// <summary>A MemberRef token created from parent, UTF-8 name, and exact signature.</summary>
    MemberRef = 3,

    /// <summary>A MethodSpec token created from a method parent and exact instantiation.</summary>
    MethodSpec = 4
}
