namespace AlvorKit.Mocking;

/// <summary>Normalizes a task-like return and prepares its later slot-only observation.</summary>
internal sealed class MockAsyncReturn
{
    private static readonly ConditionalWeakTable<Type, MockValueTaskAdapter>
        ValueTaskAdapters = [];
    private static readonly Lock ValueTaskAdapterGate = new();
    private readonly Task task;

    private MockAsyncReturn(object returnValue, Task task)
    {
        ReturnValue = returnValue;
        this.task = task;
    }

    /// <summary>Gets the caller-visible task or preserved value task.</summary>
    internal object ReturnValue { get; }

    /// <summary>Prepares one of the four supported task-like return shapes.</summary>
    internal static MockAsyncReturn? Prepare(
        Type declaredType,
        object? returnValue)
    {
        if (MockTypeShape.MayBeByRefLike(declaredType))
            return null;
        if (typeof(Task).IsAssignableFrom(declaredType)
            && returnValue is Task task)
            return new(returnValue, task);
        if (returnValue is null)
            return null;

        bool genericValueTask =
            declaredType.IsGenericType &&
            declaredType.GetGenericTypeDefinition() ==
                typeof(ValueTask<>);
        if (genericValueTask &&
            !RuntimeFeature.IsDynamicCodeSupported)
        {
            throw new MockException(
                $"Async completion observation for '{declaredType}' requires " +
                "runtime code generation.");
        }

        try
        {
            if (declaredType == typeof(ValueTask))
            {
                ValueTask preserved = ((ValueTask)returnValue).Preserve();
                return new(preserved, preserved.AsTask());
            }

            if (!genericValueTask)
                return null;

            MockValueTaskAdapter adapter;
            lock (ValueTaskAdapterGate)
            {
                if (!ValueTaskAdapters.TryGetValue(
                        declaredType,
                        out adapter!))
                {
                    adapter = CreateDynamicValueTaskAdapter(
                        declaredType);
                    ValueTaskAdapters.Add(
                        declaredType,
                        adapter);
                }
            }
            object preservedValue = adapter.Preserve(
                returnValue,
                out Task preservedTask);
            return new(preservedValue, preservedTask);
        }
        catch
        {
            return null;
        }
    }

    [UnconditionalSuppressMessage(
        "Aot",
        "IL3050",
        Justification =
            "The caller checks dynamic-code support before entering this " +
            "closed adapter factory.")]
    private static MockValueTaskAdapter CreateDynamicValueTaskAdapter(
        Type declaredType)
    {
        Type adapterType = typeof(MockValueTaskAdapter<>)
            .MakeGenericType(
                declaredType.GetGenericArguments()[0]);
        return (MockValueTaskAdapter)Activator.CreateInstance(
            adapterType,
            nonPublic: true)!;
    }

    /// <summary>Observes completion while retaining only the exact invocation slot.</summary>
    internal void Observe(MockInvocationSlot slot)
    {
        ArgumentNullException.ThrowIfNull(slot);
        new MockAsyncCompletionRegistration(task, slot).Register();
    }
}

/// <summary>Preserves one boxed generic value task without reflection invocation.</summary>
internal abstract class MockValueTaskAdapter
{
    /// <summary>Returns a caller-safe preserved value and its shared task.</summary>
    internal abstract object Preserve(
        object value,
        out Task task);
}

/// <summary>Preserves one exact <see cref="ValueTask{TResult}"/> construction.</summary>
internal sealed class MockValueTaskAdapter<T> : MockValueTaskAdapter
{
    /// <inheritdoc />
    internal override object Preserve(
        object value,
        out Task task)
    {
        ValueTask<T> preserved = ((ValueTask<T>)value).Preserve();
        Task<T> preservedTask = preserved.AsTask();
        task = preservedTask;
        return preserved;
    }
}

/// <summary>Publishes one later task outcome without retaining its mock or history owner.</summary>
internal sealed class MockAsyncCompletionRegistration(
    Task task,
    MockInvocationSlot slot)
{
    /// <summary>Completes immediately or registers without flowing execution context.</summary>
    internal void Register()
    {
        if (task.IsCompleted)
        {
            Complete();
            return;
        }

        task.ConfigureAwait(false)
            .GetAwaiter()
            .UnsafeOnCompleted(Complete);
    }

    private void Complete()
    {
        MockInvocationAsyncCompletion completion;
        if (task.IsCanceled)
        {
            completion = new(MockInvocationAsyncCompletionKind.Canceled);
        }
        else if (task.IsFaulted)
        {
            completion = new(
                MockInvocationAsyncCompletionKind.Faulted,
                GetAwaitFailure());
        }
        else
        {
            completion = new(MockInvocationAsyncCompletionKind.Succeeded);
        }

        slot.CompleteAsync(completion);
    }

    private Exception GetAwaitFailure()
    {
        try
        {
            task.GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            return exception;
        }

        throw new InvalidOperationException(
            "A faulted task completed without an await-observed exception.");
    }
}
