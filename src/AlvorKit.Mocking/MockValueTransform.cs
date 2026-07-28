namespace AlvorKit.Mocking;

/// <summary>Transforms one live field value without routing it through an object carrier.</summary>
/// <typeparam name="T">The exact field value type.</typeparam>
/// <param name="value">The live field value owned by the interception typed frame.</param>
/// <returns>The value that the interception field operation should use.</returns>
public delegate T MockValueTransform<T>(scoped in T value)
    where T : allows ref struct;
