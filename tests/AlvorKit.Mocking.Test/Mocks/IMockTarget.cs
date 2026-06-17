namespace AlvorKit.Mocking.Test;

public unsafe interface IMockTarget
{
    event EventHandler OnEvent;
    event Action<int> OnActionEvent;

    int this[string key] { get; set; }

    Action? Action { get; }
    List<int> Values { get; }
    int[] Numbers { get; }

    int Property { get; }
    int* PtrProperty { get; }
    ref int RefProperty { get; }
    ref int* RefPtrProperty { get; }

    IMockTarget ChildTarget { get; }
    ClassMock Model { get; }

    int GetValue();
    int* GetPtrValue();
    int ComputeSum(int a, int b) => -2134;
    int ComputeSumOpen<A, B>(A a, B b);
    int ComputeSumWithSpan(int a, int b, Span<int> ints);
    int ComputeSumWithSpanOut(int a, int b, out Span<int> ints);
    int ComputeSumWithSpanRef(int a, int b, ref Span<int> ints);
    Span<int> ComputeSumWithSpanReturn(int a, int b);
    ref Span<int> ComputeSumWithSpanRefReturn(int a, int b);

    void RaiseEvent();
    void Read(out int val);
    void ReadPtr(out int* val);
    void Write(ref int val);
    void WritePtr(ref int* val);

    ref int GetRef();
    ref int* GetRefPtr();
}
