using Entitas;
using EntitasComponent = Entitas.IComponent;

namespace AlvorKit;

internal class RaceEntitasContext(int componentCount) : IDisposable
{
    private readonly Context<Entity> _world = new(componentCount, static () => new Entity());

    internal Context<Entity> World => _world;

    public void Dispose() => _world.DestroyAllEntities();

    public class Component1 : EntitasComponent
    {
        public int Value;
    }

    public class Component2 : EntitasComponent
    {
        public int Value;
    }

    public class Component3 : EntitasComponent
    {
        public int Value;
    }

    public class Padding : EntitasComponent;
    public class Marker1 : EntitasComponent;
    public class Marker2 : EntitasComponent;
    public class Marker3 : EntitasComponent;
    public class Marker4 : EntitasComponent;
}
