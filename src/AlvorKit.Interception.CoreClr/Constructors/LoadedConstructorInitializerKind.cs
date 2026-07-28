namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies the mandatory constructor relation established by one exact call.</summary>
public enum LoadedConstructorInitializerKind
{
    /// <summary>The constructor directly initializes its base-type portion.</summary>
    Base,

    /// <summary>The constructor delegates initialization to another constructor on its own type.</summary>
    This
}
