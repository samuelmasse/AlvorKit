namespace AlvorKit.Mocking.Test;

public unsafe abstract class AbstractMock : IMockTarget
{
    public abstract event EventHandler? OnEvent;
    public abstract event Action<int>? OnActionEvent;

    public abstract int this[string key] { get; set; }

    public abstract Action? Action { get; }
    public abstract List<int> Values { get; }
    public abstract int[] Numbers { get; }

    public abstract int Property { get; }
    public abstract int* PtrProperty { get; }
    public abstract ref int RefProperty { get; }
    public abstract ref int* RefPtrProperty { get; }

    public abstract IMockTarget ChildTarget { get; }
    public abstract ClassMock Model { get; }

    public abstract int GetValue();
    public abstract int* GetPtrValue();
    public abstract int ComputeSum(int a, int b);
    public abstract int ComputeSumOpen<A, B>(A a, B b);
    public abstract int ComputeSumWithSpan(int a, int b, Span<int> ints);
    public abstract int ComputeSumWithSpanOut(int a, int b, out Span<int> ints);
    public abstract int ComputeSumWithSpanRef(int a, int b, ref Span<int> ints);
    public abstract Span<int> ComputeSumWithSpanReturn(int a, int b);
    public abstract ref Span<int> ComputeSumWithSpanRefReturn(int a, int b);

    public abstract void RaiseEvent();
    public abstract void Read(out int val);
    public abstract void ReadPtr(out int* val);
    public abstract void Write(ref int val);
    public abstract void WritePtr(ref int* val);

    public abstract ref int GetRef();
    public abstract ref int* GetRefPtr();
}
