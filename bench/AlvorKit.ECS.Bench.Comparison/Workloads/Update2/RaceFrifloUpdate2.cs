using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;

namespace AlvorKit;

internal static class RaceFrifloUpdate2
{
    internal class FrifloEngineEcsContext(int entCount, int padding)
        : RaceFrifloEngineEcsBaseContext(entCount, padding, ComponentTypes.Get<Component1, Component2>())
    {
    }

    internal static void Update(ref Component1 c1, ref Component2 c2)
    {
        c1.Value += c2.Value;
    }
}
