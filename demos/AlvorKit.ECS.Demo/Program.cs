const int ComponentCount = 7;
const int ExhaustiveBits = 7;
const int ExhaustiveEntityCount = (1 << ExhaustiveBits) - 1;
const int RandomEntityCount = 4096;
const int MutationRounds = 18;
const int UnsetStride = 16;
const int ParallelArenaCount = 4;
const int ParallelEntityCount = 1024;
const int ParallelMutationRounds = 12;

var stopwatch = Stopwatch.StartNew();
var setters = CreateSetters();
var unsetters = CreateUnsetters();
var getters = CreateGetters();
var has = CreateHas();

Console.WriteLine("AlvorKit ECS archetypal stress");
Console.WriteLine($"components: {ComponentCount}");
Console.WriteLine($"exhaustive subset entities: {ExhaustiveEntityCount}");
Console.WriteLine($"random entities: {RandomEntityCount}");

RunSingleAllocatorStress(setters, unsetters, getters, has);
RunParallelAllocatorStress(setters, unsetters, getters, has);

stopwatch.Stop();
Console.WriteLine($"ok: {stopwatch.ElapsedMilliseconds} ms");

static void RunSingleAllocatorStress(SetComponent[] setters, UnsetComponent[] unsetters, GetComponent[] getters, HasComponent[] has)
{
    int entityCount = ExhaustiveEntityCount + RandomEntityCount;
    var entities = new EntMut[entityCount];
    var masks = new int[entityCount];
    var values = new object?[entityCount, ComponentCount];
    var rng = new XorShift64(0x631D_723A_91E8_4D15UL);
    int entityIndex = 0;

    for (int mask = 1; mask < 1 << ExhaustiveBits; mask++)
    {
        entities[entityIndex] = (EntMut)new EntPtr();

        for (int field = 0; field < ExhaustiveBits; field++)
        {
            if ((mask & (1 << field)) != 0)
                SetExpected(setters, entities, masks, values, entityIndex, field, ValueFor(entityIndex, field, 0));
        }

        entityIndex++;
    }

    UnsetExpected(unsetters, entities, masks, values, 0, 0);

    Span<int> order = stackalloc int[ComponentCount];

    for (int i = 0; i < RandomEntityCount; i++)
    {
        entities[entityIndex] = (EntMut)new EntPtr();
        FillOrder(order);
        Shuffle(order, ref rng);

        int additions = 4 + rng.NextInt(ComponentCount - 3);
        for (int j = 0; j < additions; j++)
            SetExpected(setters, entities, masks, values, entityIndex, order[j], ValueFor(entityIndex, order[j], 1));

        entityIndex++;
    }

    for (int i = UnsetStride; i < entities.Length; i += UnsetStride)
    {
        int field = SelectSetField(masks[i], ref rng);
        UnsetExpected(unsetters, entities, masks, values, i, field);
    }

    for (int round = 0; round < MutationRounds; round++)
    {
        for (int i = 0; i < entities.Length; i++)
        {
            int field = rng.NextInt(ComponentCount);
            SetExpected(setters, entities, masks, values, i, field, ValueFor(i, field, round + 2));

            if ((i & 7) == 0)
            {
                int secondField = (field + 1 + rng.NextInt(ComponentCount - 1)) % ComponentCount;
                SetExpected(setters, entities, masks, values, i, secondField, ValueFor(i, secondField, round + 101));
            }
        }
    }

    VerifyAll(entities, masks, values, getters, has);

    var distinctMasks = new HashSet<int>();
    foreach (var mask in masks)
        distinctMasks.Add(mask);

    Console.WriteLine($"single allocator entities: {entities.Length}");
    Console.WriteLine($"single allocator distinct final shapes: {distinctMasks.Count}");
}

static void RunParallelAllocatorStress(SetComponent[] setters, UnsetComponent[] unsetters, GetComponent[] getters, HasComponent[] has)
{
    Parallel.For(0, ParallelArenaCount, arenaIndex =>
    {
        var arena = new EntArena();
        var entities = new EntMut[ParallelEntityCount];
        var masks = new int[ParallelEntityCount];
        var values = new object?[ParallelEntityCount, ComponentCount];
        var rng = new XorShift64(0x9E37_79B9_7F4A_7C15UL + (ulong)arenaIndex * 0xBF58_476D_1CE4_E5B9UL);
        Span<int> order = stackalloc int[ComponentCount];

        for (int i = 0; i < entities.Length; i++)
        {
            entities[i] = (EntMut)arena.Alloc();
            FillOrder(order);
            Shuffle(order, ref rng);

            int additions = 3 + rng.NextInt(ComponentCount - 2);
            for (int j = 0; j < additions; j++)
                SetExpected(setters, entities, masks, values, i, order[j], ValueFor(i, order[j], arenaIndex));
        }

        for (int i = 0; i < entities.Length; i += UnsetStride)
        {
            int field = SelectSetField(masks[i], ref rng);
            UnsetExpected(unsetters, entities, masks, values, i, field);
        }

        for (int round = 0; round < ParallelMutationRounds; round++)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                int field = rng.NextInt(ComponentCount);
                SetExpected(setters, entities, masks, values, i, field, ValueFor(i, field, round + arenaIndex * 32));
            }
        }

        VerifyAll(entities, masks, values, getters, has);
    });

    Console.WriteLine($"parallel allocator arenas: {ParallelArenaCount}");
    Console.WriteLine($"parallel allocator entities: {ParallelArenaCount * ParallelEntityCount}");
}

static void SetExpected(
    SetComponent[] setters,
    EntMut[] entities,
    int[] masks,
    object?[,] values,
    int entityIndex,
    int field,
    int value)
{
    setters[field](entities[entityIndex], value);
    masks[entityIndex] |= 1 << field;
    values[entityIndex, field] = BoxExpectedValue(field, value);
}

static void UnsetExpected(
    UnsetComponent[] unsetters,
    EntMut[] entities,
    int[] masks,
    object?[,] values,
    int entityIndex,
    int field)
{
    bool actual = unsetters[field](entities[entityIndex]);
    if (!actual)
        throw new InvalidOperationException($"unset failed entity={entityIndex} field={field}");

    masks[entityIndex] &= ~(1 << field);
    values[entityIndex, field] = null;
}

static void VerifyAll(
    EntMut[] entities,
    int[] masks,
    object?[,] values,
    GetComponent[] getters,
    HasComponent[] has)
{
    for (int entity = 0; entity < entities.Length; entity++)
    {
        for (int field = 0; field < ComponentCount; field++)
        {
            bool expectedHas = (masks[entity] & (1 << field)) != 0;
            bool actualHas = has[field](entities[entity]);

            if (actualHas != expectedHas)
            {
                throw new InvalidOperationException(
                    $"has mismatch entity={entity} field={field} expected={expectedHas} actual={actualHas}");
            }

            if (!expectedHas)
            {
                if (IsReferenceField(field) && getters[field](entities[entity]) != null)
                {
                    throw new InvalidOperationException($"missing reference value mismatch entity={entity} field={field}");
                }

                continue;
            }

            object? actualValue = getters[field](entities[entity]);
            object? expectedValue = values[entity, field];

            if (!Equals(actualValue, expectedValue))
            {
                throw new InvalidOperationException(
                    $"value mismatch entity={entity} field={field} expected={expectedValue} actual={actualValue}");
            }
        }
    }
}

static int ValueFor(int entity, int field, int salt) => unchecked((entity + 1) * 73856093 ^ (field + 1) * 19349663 ^ salt * 83492791);

static object BoxExpectedValue(int field, int value) => field switch
{
    6 => $"tag:{value}",
    _ => value,
};

static bool IsReferenceField(int field) => field == 6;

static int SelectSetField(int mask, ref XorShift64 rng)
{
    int ordinal = rng.NextInt(BitOperations.PopCount((uint)mask));

    for (int field = 0; ; field++)
    {
        if ((mask & (1 << field)) != 0 && ordinal-- == 0)
            return field;
    }
}

static void FillOrder(Span<int> order)
{
    for (int i = 0; i < order.Length; i++)
        order[i] = i;
}

static void Shuffle(Span<int> order, ref XorShift64 rng)
{
    for (int i = order.Length - 1; i > 0; i--)
    {
        int j = rng.NextInt(i + 1);
        (order[i], order[j]) = (order[j], order[i]);
    }
}

static SetComponent[] CreateSetters() =>
[
    static (ent, value) => ent.SetArchetypal<int, C00, StressArch>(value),
    static (ent, value) => ent.SetArchetypal<int, C01, StressArch>(value),
    static (ent, value) => ent.SetArchetypal<int, C02, StressArch>(value),
    static (ent, value) => ent.SetArchetypal<int, C03, StressArch>(value),
    static (ent, value) => ent.SetArchetypal<int, C04, StressArch>(value),
    static (ent, value) => ent.SetArchetypal<int, C05, StressArch>(value),
    static (ent, value) => ent.SetArchetypal<string?, C06, StressArch>($"tag:{value}"),
];

static UnsetComponent[] CreateUnsetters() =>
[
    static ent => ent.UnsetArchetypal<int, C00, StressArch>(),
    static ent => ent.UnsetArchetypal<int, C01, StressArch>(),
    static ent => ent.UnsetArchetypal<int, C02, StressArch>(),
    static ent => ent.UnsetArchetypal<int, C03, StressArch>(),
    static ent => ent.UnsetArchetypal<int, C04, StressArch>(),
    static ent => ent.UnsetArchetypal<int, C05, StressArch>(),
    static ent => ent.UnsetArchetypal<string?, C06, StressArch>(),
];

static GetComponent[] CreateGetters() =>
[
    static ent => ent.GetArchetypal<int, C00, StressArch>(),
    static ent => ent.GetArchetypal<int, C01, StressArch>(),
    static ent => ent.GetArchetypal<int, C02, StressArch>(),
    static ent => ent.GetArchetypal<int, C03, StressArch>(),
    static ent => ent.GetArchetypal<int, C04, StressArch>(),
    static ent => ent.GetArchetypal<int, C05, StressArch>(),
    static ent => ent.GetArchetypal<string?, C06, StressArch>(),
];

static HasComponent[] CreateHas() =>
[
    static ent => ent.HasArchetypal<int, C00, StressArch>(),
    static ent => ent.HasArchetypal<int, C01, StressArch>(),
    static ent => ent.HasArchetypal<int, C02, StressArch>(),
    static ent => ent.HasArchetypal<int, C03, StressArch>(),
    static ent => ent.HasArchetypal<int, C04, StressArch>(),
    static ent => ent.HasArchetypal<int, C05, StressArch>(),
    static ent => ent.HasArchetypal<string?, C06, StressArch>(),
];

delegate void SetComponent(EntMut ent, int value);

delegate bool UnsetComponent(EntMut ent);

delegate object? GetComponent(EntMut ent);

delegate bool HasComponent(EntMut ent);

struct XorShift64(ulong state)
{
    private ulong state = state == 0 ? 0xCBF2_9CE4_8422_2325UL : state;

    public int NextInt(int exclusiveMax) => (int)(Next() % (uint)exclusiveMax);

    private ulong Next()
    {
        ulong x = state;
        x ^= x << 7;
        x ^= x >> 9;
        state = x;
        return x;
    }
}

struct StressArch;
struct C00;
struct C01;
struct C02;
struct C03;
struct C04;
struct C05;
struct C06;
