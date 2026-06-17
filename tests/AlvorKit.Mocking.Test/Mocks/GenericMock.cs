namespace AlvorKit.Mocking.Test;

#pragma warning disable CS0067

public unsafe class GenericMock<T> : IMockTarget
{
    private int val = 40;
    private int* ptr;

    public event EventHandler? OnEvent;
    public event Action<int>? OnActionEvent;

    public int this[string key] { get => 34; set { } }

    public Action? Action => null;
    public List<int> Values { get; } = null!;
    public int[] Numbers { get; } = null!;

    public int Property { get; }
    public int* PtrProperty => ptr;
    public ref int RefProperty => ref val;
    public ref int* RefPtrProperty => ref ptr;

    public IMockTarget ChildTarget { get; } = null!;
    public ClassMock Model { get; } = null!;

    public int GetValue() => 34;
    public int* GetPtrValue() => ptr;
    public int ComputeSum(int a, int b) => a + b;
    public int ComputeSumOpen<A, B>(A a, B b) => 2;
    public int ComputeSumWithSpan(int a, int b, Span<int> ints) => a + b;
    public int ComputeSumWithSpanOut(int a, int b, out Span<int> ints) { ints = default; return 0; }
    public int ComputeSumWithSpanRef(int a, int b, ref Span<int> ints) => 0;
    public Span<int> ComputeSumWithSpanReturn(int a, int b) => default;
    public ref Span<int> ComputeSumWithSpanRefReturn(int a, int b) => throw new NotImplementedException();

    public void RaiseEvent() { }
    public void Read(out int val) => val = 43;
    public void ReadPtr(out int* val) => val = ptr;
    public void Write(ref int val) { }
    public void WritePtr(ref int* val) { }

    public ref int GetRef() => ref val;
    public ref int* GetRefPtr() => ref ptr;
}

#pragma warning restore CS0067
