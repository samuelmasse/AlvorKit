using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;

namespace AlvorKit;

internal static class RaceFrifloUpdate3
{
    internal class FrifloEngineEcsContext(int entCount, int padding)
        : RaceFrifloEngineEcsBaseContext(entCount, padding, ComponentTypes.Get<Component1, Component2, Component3>())
    {
    }

    internal static void Update(ref Component1 c1, ref Component2 c2, ref Component3 c3)
    {
        c1.Value += c2.Value + c3.Value;
    }
}
