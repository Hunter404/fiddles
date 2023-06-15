namespace TestEngine.Sequence.Common;

public class AsyncEventHandler<T>
{
    private readonly HashSet<Func<object, T, Task>> _invocationList = new ();
    private readonly object _lock = new();

    public async Task InvokeAsync(object sender, T eventArgs)
    {
        HashSet<Func<object, T, Task>> invocations;
        lock (_lock)
        {
            invocations = new HashSet<Func<object, T, Task>>(_invocationList);
        }

        foreach (var callback in invocations)
        {
            await callback(sender, eventArgs);
        }
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

        HashSet<Func<object, T, Task>> otherCallbacks;

        lock (other._lock)
        {
            otherCallbacks = new HashSet<Func<object, T, Task>>(other._invocationList);
        }

        lock (e._lock)
        {
            foreach (var callback in otherCallbacks)
            {
                e._invocationList.Add(callback);
            }
        }

        return e;
    }

    public static AsyncEventHandler<T>? operator -(AsyncEventHandler<T>? e, Func<object, T, Task> callback)
    {
        if (callback == null)
        {
            throw new NullReferenceException("Callback can't be null");
        }

        if (e == null)
        {
            return null;
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

        HashSet<Func<object, T, Task>> otherCallbacks;

        lock (other._lock)
        {
            otherCallbacks = new HashSet<Func<object, T, Task>>(other._invocationList);
        }

        lock (e._lock)
        {
            foreach (var callback in otherCallbacks)
            {
                e._invocationList.Remove(callback);
            }
        }

        return e;
    }
}
