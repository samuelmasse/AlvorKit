using Svelto.ECS;

namespace AlvorKit;

internal class ComparisonSveltoObserver : IQueryingEntitiesEngine
{
    private EntitiesDB database = null!;

    public EntitiesDB entitiesDB { get => database; set => database = value; }
    public void Ready()
    {
    }
}
