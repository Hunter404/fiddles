namespace TestEngine.Sequence.Common;

public class ConcurrentAsyncEventHandler<T>
{
    private readonly HashSet<AsyncEventHandler<T>> _otherEventHandlers = new();
    private readonly HashSet<Func<object, T, Task>> _invocationList = new ();
    private readonly object _lock = new();

    public async Task InvokeAsync(object sender, T eventArgs)
    {
        HashSet<Func<object, T, Task>> invocations;
        lock (_lock)
        {
            invocations = new HashSet<Func<object, T, Task>>(_invocationList);
        }

        var tasks = invocations
            .Select(callback => callback(sender, eventArgs))
            .ToList();

        HashSet<AsyncEventHandler<T>> otherEventHandlers;
        lock (_lock)
        {
            otherEventHandlers = new HashSet<AsyncEventHandler<T>>(_otherEventHandlers);
        }

        tasks
            .AddRange(otherEventHandlers
                .Select(handler => handler.InvokeAsync(sender, eventArgs)));

        await Task.WhenAll(tasks);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _invocationList.Clear();
        }
    }

    public static AsyncEventHandler<T> operator +(AsyncEventHandler<T>? e, Func<object, T, Task> callback)
    {
        if (callback == null)
        {
            throw new NullReferenceException("Callback can't be null");
        }

        e ??= new AsyncEventHandler<T>();

        lock (e._lock)
        {
            e._invocationList.Add(callback);
        }

        return e;
    }

    public static AsyncEventHandler<T> operator +(AsyncEventHandler<T>? e, AsyncEventHandler<T>? other)
    {
        if (other == null)
        {
            throw new NullReferenceException("Event handler can't be null");
        }

        e ??= new AsyncEventHandler<T>();

        lock (e._lock)
        {
            e._otherEventHandlers.Add(other);
        }

        return e;
    }

    public static AsyncEventHandler<T>? operator -(AsyncEventHandler<T>? e, Func<object, T, Task>? callback)
    {
        if (e == null || callback == null)
        {
            return e;
        }

        lock (e._lock)
        {
            e._invocationList.Remove(callback);
        }

        return e;
    }

    public static AsyncEventHandler<T>? operator -(AsyncEventHandler<T>? e, AsyncEventHandler<T>? other)
    {
        if (e == null || other == null)
        {
            return e;
        }

        lock (e._lock)
        {
            e._otherEventHandlers.Remove(other);
        }

        return e;
    }
}
