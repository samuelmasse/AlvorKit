namespace AlvorKit.Windowing.Test;

[TestClass]
public class PublicApiTest
{
    /// <summary>Verifies public windowing APIs do not expose backend binding implementation types.</summary>
    [TestMethod]
    public void PublicApi_DoesNotExposeOpenTkOrGeneratedGlfwTypes()
    {
        AssertAssembly(typeof(WindowLoop).Assembly, false);
        AssertAssembly(typeof(GlfwWindowHost).Assembly, true);
        AssertAssembly(typeof(AgentGlfwWindowHost).Assembly, true);
    }

    /// <summary>Verifies the host interface remains mockable by the repository mocking library.</summary>
    [TestMethod]
    public void IWindowHost_CanBeMockedWithAlvorKitMocking()
    {
        var host = Mock.Create<IWindowHost>();
        var raised = false;
        host.Closing += () => raised = true;
        Mock.When(() => host.IsFocused).Return(true);

        Mock.Raise(() => host.Closing += null);

        Assert.IsTrue(host.IsFocused);
        Assert.IsTrue(raised);
    }

    private static void AssertNoForbiddenType(Type type, string owner, bool allowGlfwTypes)
    {
        if (type.IsGenericType)
        {
            if (!type.IsGenericTypeDefinition)
                AssertNoForbiddenType(type.GetGenericTypeDefinition(), owner, allowGlfwTypes);

            foreach (var argument in type.GetGenericArguments())
                AssertNoForbiddenType(argument, owner, allowGlfwTypes);

            return;
        }

        if (type.HasElementType)
            AssertNoForbiddenType(type.GetElementType()!, owner, allowGlfwTypes);

        var ns = type.Namespace ?? string.Empty;
        Assert.IsFalse(ns.StartsWith("OpenTK", StringComparison.Ordinal), $"{owner} exposes {type.FullName}.");
        if (!allowGlfwTypes)
            Assert.IsFalse(ns.StartsWith("AlvorKit.GLFW", StringComparison.Ordinal), $"{owner} exposes {type.FullName}.");

        Assert.IsFalse((type.FullName ?? string.Empty).Contains("Wigdow", StringComparison.Ordinal), owner);
    }

    private static void AssertAssembly(Assembly assembly, bool allowGlfwTypes)
    {
        foreach (var type in assembly.GetExportedTypes())
        {
            AssertNoForbiddenType(type, type.FullName ?? type.Name, allowGlfwTypes);

            foreach (var constructor in type.GetConstructors())
            {
                foreach (var parameter in constructor.GetParameters())
                    AssertNoForbiddenType(parameter.ParameterType, constructor.Name, allowGlfwTypes);
            }

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                AssertNoForbiddenType(method.ReturnType, method.Name, allowGlfwTypes);

                foreach (var parameter in method.GetParameters())
                    AssertNoForbiddenType(parameter.ParameterType, method.Name, allowGlfwTypes);
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
                AssertNoForbiddenType(property.PropertyType, property.Name, allowGlfwTypes);

            foreach (var ev in type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
                AssertNoForbiddenType(ev.EventHandlerType!, ev.Name, allowGlfwTypes);

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
                AssertNoForbiddenType(field.FieldType, field.Name, allowGlfwTypes);
        }
    }
}
