using Entitas;
using static AlvorKit.RaceEntitasContext;
using EntitasComponent = Entitas.IComponent;

namespace AlvorKit;

internal class RaceEntitasUpdateFixture(RaceEntitasContext context, IGroup<Entity> group) : IDisposable
{
    internal Context<Entity> World => context.World;
    internal IGroup<Entity> Group => group;

    internal static RaceEntitasUpdateFixture CreateUpdate(int count, int padding, int components)
    {
        var context = new RaceEntitasContext(4);

        for (var i = 0; i < count; i++)
        {
            for (var j = 0; j < padding; j++)
            {
                var padded = context.World.CreateEntity();
                padded.AddComponent(3, padded.CreateComponent<Padding>(3));
            }

            AddData(context.World.CreateEntity(), components);
        }

        return PrepareGroup(context, components);
    }

    internal static RaceEntitasUpdateFixture CreateMixed(int count)
    {
        var context = new RaceEntitasContext(7);

        for (var i = 0; i < count; i++)
        {
            var ent = context.World.CreateEntity();
            AddData(ent, 2);
            EntitasComponent marker = (i % 4) switch
            {
                0 => ent.CreateComponent<Marker1>(3),
                1 => ent.CreateComponent<Marker2>(4),
                2 => ent.CreateComponent<Marker3>(5),
                _ => ent.CreateComponent<Marker4>(6),
            };
            ent.AddComponent(3 + i % 4, marker);
        }

        return PrepareGroup(context, 2);
    }

    private static void AddData(Entity ent, int components)
    {
        var first = ent.CreateComponent<Component1>(0);
        first.Value = 0;
        ent.AddComponent(0, first);

        if (components >= 2)
        {
            var second = ent.CreateComponent<Component2>(1);
            second.Value = 1;
            ent.AddComponent(1, second);
        }

        if (components == 3)
        {
            var third = ent.CreateComponent<Component3>(2);
            third.Value = 1;
            ent.AddComponent(2, third);
        }
    }

    private static RaceEntitasUpdateFixture PrepareGroup(RaceEntitasContext context, int components)
    {
        var matcher = components switch
        {
            1 => Matcher<Entity>.AllOf(0),
            2 => Matcher<Entity>.AllOf(0, 1),
            3 => Matcher<Entity>.AllOf(0, 1, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(components)),
        };
        var group = context.World.GetGroup(matcher);
        group.GetEntities();
        return new(context, group);
    }

    public void Dispose() => context.Dispose();
}
