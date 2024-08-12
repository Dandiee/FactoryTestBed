using System.Collections;

namespace FactoryOptimizer;

public interface IDataStructure<T> : IEnumerable<T>
{
    public int Count { get; }
    public void Add(T item);
    public T Get();
}

public class MyStack<T> : IDataStructure<T>
{
    private readonly Stack<T> _stack = new();

    public MyStack(T item)
    {
        _stack.Push(item);
    }

    public int Count => _stack.Count;
    public void Add(T item) => _stack.Push(item);
    public T Get() => _stack.Pop();
    public IEnumerator<T> GetEnumerator() => _stack.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _stack.GetEnumerator();
}

public class MyQueue<T> : IDataStructure<T>
{
    private readonly Queue<T> _queue = new();

    public MyQueue(T item)
    {
        _queue.Enqueue(item);
    }

    public int Count => _queue.Count;
    public void Add(T item) => _queue.Enqueue(item);
    public T Get() => _queue.Dequeue();
    public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();
}