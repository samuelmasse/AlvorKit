namespace AlvorKit.ECS.Indexed;

internal abstract class EntIdxBagIndex<N> : IComponent
{
        public static EntComponent Component => new(typeof(int), typeof(EntIdxBagIndex<N>));
}

internal abstract class EntIdxGatedBagIndex<N, TGate> : IComponent
{
        public static EntComponent Component => new(typeof(int), typeof(EntIdxGatedBagIndex<N, TGate>));
}
