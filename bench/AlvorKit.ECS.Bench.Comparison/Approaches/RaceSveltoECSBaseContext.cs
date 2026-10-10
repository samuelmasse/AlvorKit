using Svelto.ECS;
using Svelto.ECS.Schedulers;

namespace AlvorKit;

internal class RaceSveltoECSBaseContext : IDisposable
{
    private static readonly ExclusiveGroup _group = new();
    private readonly EntitiesSubmissionScheduler _scheduler;
    private readonly EnginesRoot _root;
    private readonly IEntityFactory _factory;

    public static ExclusiveGroup Group => _group;
    public EntitiesSubmissionScheduler Scheduler => _scheduler;
    public EnginesRoot Root => _root;
    public IEntityFactory Factory => _factory;

    public RaceSveltoECSBaseContext()
    {
        _scheduler = new EntitiesSubmissionScheduler();
        _root = new EnginesRoot(Scheduler);
        _factory = Root.GenerateEntityFactory();
    }

    public struct Component1 : IEntityComponent
    {
        public int Value;
    }

    public struct Component2 : IEntityComponent
    {
        public int Value;
    }

    public struct Component3 : IEntityComponent
    {
        public int Value;
    }

    public virtual void Dispose()
    {
        Root.Dispose();
        Scheduler.Dispose();
    }
}
