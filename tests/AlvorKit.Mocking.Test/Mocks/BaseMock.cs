namespace AlvorKit.Mocking.Test;

#pragma warning disable CS0067

public unsafe class BaseMock : IMockTarget
{
    private int val = 40;
    private int* ptr;

    public event EventHandler? OnEvent;
    public event Action<int>? OnActionEvent;

    public virtual int this[string key] { get => 34; set { } }

    public virtual Action? Action => null;
    public virtual List<int> Values { get; } = null!;
    public virtual int[] Numbers { get; } = null!;

    public virtual int Property { get; }
    public virtual int* PtrProperty => ptr;
    public virtual ref int RefProperty => ref val;
    public virtual ref int* RefPtrProperty => ref ptr;

    public virtual IMockTarget ChildTarget { get; } = null!;
    public virtual ClassMock Model { get; } = null!;

    public virtual int GetValue() => 34;
    public virtual int* GetPtrValue() => ptr;
    public virtual int ComputeSum(int a, int b) => a + b;
    public virtual int ComputeSumOpen<A, B>(A a, B b) => 2;
    public virtual int ComputeSumWithSpan(int a, int b, Span<int> ints) => a + b;
    public virtual int ComputeSumWithSpanOut(int a, int b, out Span<int> ints) { ints = default; return 0; }
    public virtual int ComputeSumWithSpanRef(int a, int b, ref Span<int> ints) => 0;
    public virtual Span<int> ComputeSumWithSpanReturn(int a, int b) => default;
    public virtual ref Span<int> ComputeSumWithSpanRefReturn(int a, int b) => throw new NotImplementedException();

    public virtual void RaiseEvent() { }
    public virtual void Read(out int val) => val = 43;
    public virtual void ReadPtr(out int* val) => val = ptr;
    public virtual void Write(ref int val) { }
    public virtual void WritePtr(ref int* val) { }

    public virtual ref int GetRef() => ref val;
    public virtual ref int* GetRefPtr() => ref ptr;
}

#pragma warning restore CS0067
