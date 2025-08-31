using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Task;

internal sealed class CustomQueue<T> : IEnumerable<T>
{
    private readonly IEqualityComparer<T> _comparer;
    private T[] _buffer;
    private int _head;
    private int _tail;
    private int _count;
    private int _version;

    public CustomQueue()
    {
        _buffer = new T[16];
        _comparer = EqualityComparer<T>.Default;
    }

    public CustomQueue(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _buffer = new T[capacity == 0 ? 16 : capacity];
        _comparer = EqualityComparer<T>.Default;
    }

    public CustomQueue(IEnumerable<T>? collection)
        : this(16)
    {
        ArgumentNullException.ThrowIfNull(collection);
        var count = collection is ICollection<T> coll ? coll.Count : 0;
        _buffer = new T[Math.Max(count, 16)];
        foreach (var item in collection)
        {
            Enqueue(item);
        }
    }

    public int Count => _count;

    public bool IsEmpty => _count == 0;

    public void Enqueue(T item)
    {
        if (_count == _buffer.Length)
        {
            Resize(_count + 1);
        }

        _buffer[_tail] = item;
        _tail = (_tail + 1) % _buffer.Length;
        _count++;
        _version++;
    }

    public T Dequeue()
    {
        var head = _head;
        var array = _buffer;
        if (IsEmpty)
        {
            throw new InvalidOperationException("Очередь пуста");
        }

        var remowed = array[head];
        _head = (head + 1) % array.Length;
        _count--;
        _version++;
        return remowed;
    }

    public T Peek()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("Очередь пуста");
        }

        return _buffer[_head];
    }

    public bool TryDequeue([MaybeNull] out T result)
    {
        if (IsEmpty)
        {
            result = default;
            return false;
        }

        result = Dequeue();
        return true;
    }

    public bool TryPeek([MaybeNull] out T result)
    {
        if (IsEmpty)
        {
            result = default;
            return false;
        }

        result = _buffer[_head];
        return true;
    }

    public void Clear()
    {
        if (_count != 0)
        {
            if (_head < _tail)
            {
                Array.Clear(_buffer, _head, _count);
            }

            Array.Clear(_buffer, _head, _count);
            Array.Clear(_buffer, 0, _tail);
            _count = 0;
        }

        _head = 0;
        _tail = 0;
        _version++;
    }

    public bool Contains(T item)
    {
        for (var i = 0; i < _count; i++)
        {
            var index = (_head + i) % _buffer.Length;
            if (item == null && _buffer[index] == null)
            {
                return true;
            }

            if (item != null && _comparer.Equals(_buffer[index], item))
            {
                return true;
            }
        }

        return false;
    }

    public T[] ToArray()
    {
        if (IsEmpty)
        {
            return Array.Empty<T>();
        }

        var result = new T[_count];
        if (_head < _tail)
        {
            Array.Copy(_buffer, _head, result, 0, _count);
        }
        else
        {
            var firstSegmentLength = _buffer.Length - _head;
            Array.Copy(_buffer, _head, result, 0, firstSegmentLength);
            Array.Copy(_buffer, 0, result, firstSegmentLength, _tail);
        }

        return result;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var version = _version;
        for (var i = 0; i < _count; i++)
        {
            if (version != _version)
            {
                throw new InvalidOperationException("Коллекция была изменена; операция перечисления не может быть выполнена.");
            }

            var index = (_head + i) % _buffer.Length;
            yield return _buffer[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void Resize(int minCapacity)
    {
        var newCapacity = _buffer.Length == 0 ? 16 : _buffer.Length * 2;
        newCapacity = Math.Max(newCapacity, minCapacity);

        if ((uint)newCapacity > Array.MaxLength)
        {
            newCapacity = Array.MaxLength;
            if (newCapacity < minCapacity)
            {
                throw new InvalidOperationException("Превышена максимальная емкость массива.");
            }
        }

        var newBuffer = new T[newCapacity];
        if (_count > 0)
        {
            if (_head < _tail)
            {
                Array.Copy(_buffer, _head, newBuffer, 0, _count);
            }
            else
            {
                var firstSegmentLength = _buffer.Length - _head;
                Array.Copy(_buffer, _head, newBuffer, 0, firstSegmentLength);
                Array.Copy(_buffer, 0, newBuffer, firstSegmentLength, _tail);
            }
        }

        _buffer = newBuffer;
        _head = 0;
        _tail = _count;
        _version++;
    }
}
