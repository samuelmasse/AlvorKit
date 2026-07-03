namespace AlvorKit.Engine.Loop;

/// <summary>Builds <see cref="ControlList"/> constructor arguments from root control names.</summary>
[Root]
public class RootControlListInjector(RootControls controls) : InjectorCustomHandler
{
    /// <inheritdoc />
    public override bool Handles(Type type) => type.BaseType == typeof(ControlList);

    /// <inheritdoc />
    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path)
    {
        var constructor = Constructor(type, path);
        var parameters = constructor.GetParameters();
        var values = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.ParameterType != typeof(Control) || parameter.Name is null)
                throw new InjectorException(path, $"Control list '{type.FullName}' constructor parameter '{parameter.Name}' must be a named Control.");

            values[i] = controls[parameter.Name];
        }

        return constructor.Invoke(values);
    }
}
