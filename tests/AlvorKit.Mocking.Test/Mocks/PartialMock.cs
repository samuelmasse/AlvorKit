namespace AlvorKit.Mocking.Test;

public unsafe abstract class PartialMock : IMockTarget
{
    private int val = 40;
    private int* ptr;

    public abstract event EventHandler? OnEvent;
    public abstract event Action<int>? OnActionEvent;

    public abstract int this[string key] { get; set; }

    public abstract Action? Action { get; }
    public abstract List<int> Values { get; }
    public abstract int[] Numbers { get; }

    public abstract int Property { get; }
    public virtual int* PtrProperty => ptr;
    public virtual ref int RefProperty => ref val;
    public virtual ref int* RefPtrProperty => ref ptr;

    public abstract IMockTarget ChildTarget { get; }
    public abstract ClassMock Model { get; }

    public abstract int GetValue();
    public virtual int* GetPtrValue() => ptr;
    public virtual int ComputeSum(int a, int b) => -1234;
    public virtual int ComputeSumOpen<A, B>(A a, B b) => 2;
    public virtual int ComputeSumWithSpan(int a, int b, Span<int> ints) => a + b;
    public virtual int ComputeSumWithSpanOut(int a, int b, out Span<int> ints) { ints = default; return 0; }
    public virtual int ComputeSumWithSpanRef(int a, int b, ref Span<int> ints) => 0;
    public abstract Span<int> ComputeSumWithSpanReturn(int a, int b);
    public virtual ref Span<int> ComputeSumWithSpanRefReturn(int a, int b) => throw new NotImplementedException();

    public virtual void Read(out int val) => val = 43;
    public virtual void ReadPtr(out int* val) => val = ptr;
    public abstract void RaiseEvent();
    public abstract void Write(ref int val);
    public virtual void WritePtr(ref int* val) { }

    public abstract ref int GetRef();
    public virtual ref int* GetRefPtr() => ref ptr;
}
