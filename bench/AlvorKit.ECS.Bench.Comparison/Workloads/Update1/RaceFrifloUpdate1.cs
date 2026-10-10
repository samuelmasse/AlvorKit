using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;

namespace AlvorKit;

internal static class RaceFrifloUpdate1
{
    internal class FrifloEngineEcsContext(int entCount, int padding)
        : RaceFrifloEngineEcsBaseContext(entCount, padding, ComponentTypes.Get<Component1>())
    {
    }
}
