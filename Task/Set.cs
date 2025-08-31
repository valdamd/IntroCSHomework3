using System.Collections;

namespace Task;

internal sealed class Set<T> : ISet<T>
{
    private readonly IEqualityComparer<T> _comparer;
    private T[] _items;
    private int _count;
    private int _version;

    public Set()
    {
        _items = new T[16];
        _count = 0;
        _version = 0;
        _comparer = EqualityComparer<T>.Default;
    }

    public Set(IEnumerable<T> collection)
        : this()
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (collection is ICollection<T> { Count: > 0 } coll)
        {
            EnsureCapacity(coll.Count);
        }

        foreach (var item in collection)
        {
            Add(item);
        }
    }

    public int Count => _count;

    public bool IsReadOnly => false;

    public bool Add(T item)
    {
        if (IndexOf(item) >= 0)
        {
            return false;
        }

        EnsureCapacity();
        _items[_count++] = item;
        _version++;
        return true;
    }

    void ICollection<T>.Add(T item) => Add(item);

    public void UnionWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            return;
        }

        if (other is ICollection<T> coll && coll.Count > 0)
        {
            EnsureCapacity(_count + coll.Count);
        }

        foreach (var item in other)
        {
            Add(item);
        }
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            return;
        }

        var otherSet = new HashSet<T>(other, _comparer);
        var result = new T[_count];
        var newCount = 0;

        for (var i = 0; i < _count; i++)
        {
            if (otherSet.Contains(_items[i]))
            {
                result[newCount++] = _items[i];
            }
        }

        _items = result;
        _count = newCount;
        _version++;
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            Clear();
            return;
        }

        foreach (var item in other)
        {
            Remove(item);
        }
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            Clear();
            return;
        }

        foreach (var item in other)
        {
            if (!Remove(item))
            {
                Add(item);
            }
        }
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            return true;
        }

        if (_count == 0)
        {
            return true;
        }

        if (other is ICollection<T> coll && coll.Count <= 4)
        {
            return this.All(item => coll.Contains(item));
        }

        var otherSet = new HashSet<T>(other, _comparer);
        return _count <= otherSet.Count && this.All(item => otherSet.Contains(item));
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            return true;
        }

        if (other is ICollection<T> coll && coll.Count <= 4)
        {
            return coll.All(item => Contains(item));
        }

        return other.All(item => Contains(item));
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            return false;
        }

        var otherSet = new HashSet<T>(other, _comparer);
        return _count < otherSet.Count && IsSubsetOf(otherSet);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            return false;
        }

        var otherSet = new HashSet<T>(other, _comparer);
        return _count > otherSet.Count && IsSupersetOf(otherSet);
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            return _count > 0;
        }

        return other.Any(item => Contains(item));
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(other, this))
        {
            return true;
        }

        var otherSet = new HashSet<T>(other, _comparer);
        return _count == otherSet.Count && this.All(item => otherSet.Contains(item));
    }

    public bool Contains(T item) => IndexOf(item) >= 0;

    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index is out of negative.");
        }

        if (arrayIndex + _count > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index is out of range or array is too small.");
        }

        Array.Copy(_items, 0, array, arrayIndex, _count);
    }

    public bool Remove(T item)
    {
        var i = IndexOf(item);
        if (i < 0)
        {
            return false;
        }

        RemoveAt(i);
        return true;
    }

    public void Clear()
    {
        if (_count > 0)
        {
            Array.Clear(_items, 0, _count);
            _count = 0;
            _version++;
        }
    }

    public void TrimExcess()
    {
        if (_count < _items.Length)
        {
            var newItems = new T[_count];
            Array.Copy(_items, 0, newItems, 0, _count);
            _items = newItems;
            _version++;
        }
    }

    public int GetTrimExcessThreshold()
    {
        return this._items.Length > 16 ? (int)(this._items.Length * 0.5) : 16;
    }

    // IEnumerable
    public IEnumerator<T> GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private int IndexOf(T item)
    {
        for (var i = 0; i < _count; i++)
        {
            if (_comparer.Equals(_items[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    private void EnsureCapacity()
    {
        if (_count < _items.Length)
        {
            return;
        }

        var newCap = _items.Length == 0 ? 16 : _items.Length * 2;

        Array.Resize(ref _items, newCap);
        _version++;
    }

    private void EnsureCapacity(int min)
    {
        var required = min == 0 ? _count + 1 : min;
        if (_items.Length >= required)
        {
            return;
        }

        var newCapacity = _items.Length == 0 ? 16 : _items.Length * 2;
        newCapacity = Math.Max(newCapacity, required);

        if ((uint)newCapacity > Array.MaxLength)
        {
            newCapacity = Array.MaxLength;
            if (newCapacity < required)
            {
                throw new InvalidOperationException("Превышена максимальная емкость массива.");
            }
        }

        var newItems = new T[newCapacity];
        if (_count > 0)
        {
            Array.Copy(_items, 0, newItems, 0, _count);
        }

        _items = newItems;
        _version++;
    }

    private void RemoveAt(int index)
    {
        if (index < 0 || index >= _count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _count--;
        if (index < _count)
        {
            Array.Copy(_items, index + 1, _items, index, _count - index);
        }

        _version++;
        if (_count < GetTrimExcessThreshold())
        {
            TrimExcess();
        }
    }

    private struct Enumerator : IEnumerator<T>
    {
        private readonly Set<T> _set;
        private readonly int _version;
        private int _index;
        private T _current;

        public Enumerator(Set<T> set)
        {
            this._set = set;
            this._version = set._version;
            this._index = 0;
            this._current = default!;
        }

        public T Current => this._current;

        object? IEnumerator.Current => this.Current;

        public bool MoveNext()
        {
            // Проверка версии — как в BCL
            if (this._version != this._set._version)
            {
                throw new InvalidOperationException("Коллекция была изменена.");
            }

            if (this._index < this._set._count)
            {
                this._current = this._set._items[this._index++];
                return true;
            }

            this._current = default!;
            return false;
        }

        public void Reset()
        {
            if (this._version != this._set._version)
            {
                throw new InvalidOperationException("Коллекция была изменена.");
            }

            this._index = 0;
            this._current = default!;
        }

        public void Dispose()
        {
            // Method intentionally left empty.
        }
    }
}
