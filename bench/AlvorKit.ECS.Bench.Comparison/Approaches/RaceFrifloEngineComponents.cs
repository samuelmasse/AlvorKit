using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;

namespace AlvorKit;

internal static class RaceFrifloEngineComponents
{
    internal struct Component1 : global::Friflo.Engine.ECS.IComponent
    {
        public int Value;
    }

    internal struct Component2 : global::Friflo.Engine.ECS.IComponent
    {
        public int Value;
    }

    internal struct Component3 : global::Friflo.Engine.ECS.IComponent
    {
        public int Value;
    }
}
