using Frent;
using Frent.Core;
using static AlvorKit.RaceFrentBaseContext;

namespace AlvorKit;

internal static class RaceFrentCreate1
{
    private static readonly EntityType _entityTypeStorage = EntityType.EntityTypeOf([Component<Component1>.ID], []);

    internal static ref readonly EntityType _entityType => ref _entityTypeStorage;
}
