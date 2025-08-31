using FluentAssertions;
using Task;
using Xunit;

namespace UnitTestsTask;

public sealed class CustomQueueTests
{
    [Fact]
    public void Enqueue_Dequeue_SingleItem_ReturnsSameItem()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(42);

        var result = queue.Dequeue();

        result.Should().Be(42);
        queue.Count.Should().Be(0);
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Enqueue_MultipleItems_DequeuesInCorrectOrder()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        queue.Dequeue().Should().Be(1);
        queue.Dequeue().Should().Be(2);
        queue.Dequeue().Should().Be(3);
        queue.Count.Should().Be(0);
    }

    [Fact]
    public void Peek_SingleItem_ReturnsItemWithoutRemoving()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(42);

        var result = queue.Peek();

        result.Should().Be(42);
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void TryDequeue_EmptyQueue_ReturnsFalse()
    {
        var queue = new CustomQueue<int>();

        var success = queue.TryDequeue(out var result);

        success.Should().BeFalse();
        result.Should().Be(default(int));
    }

    [Fact]
    public void TryPeek_EmptyQueue_ReturnsFalse()
    {
        var queue = new CustomQueue<int>();

        var success = queue.TryPeek(out var result);

        success.Should().BeFalse();
        result.Should().Be(default(int));
    }

    [Fact]
    public void Dequeue_EmptyQueue_ThrowsInvalidOperationException()
    {
        var queue = new CustomQueue<int>();

        Action act = () => queue.Dequeue();

        act.Should().Throw<InvalidOperationException>().WithMessage("Очередь пуста");
    }

    [Fact]
    public void Peek_EmptyQueue_ThrowsInvalidOperationException()
    {
        var queue = new CustomQueue<int>();

        Action act = () => queue.Peek();

        act.Should().Throw<InvalidOperationException>().WithMessage("Очередь пуста");
    }

    [Fact]
    public void Constructor_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => new CustomQueue<int>(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WithCollection_InitializesCorrectly()
    {
        var collection = new List<int>
        {
            1, 2, 3,
        };
        var queue = new CustomQueue<int>(collection);

        queue.Count.Should().Be(3);
        queue.Dequeue().Should().Be(1);
        queue.Dequeue().Should().Be(2);
        queue.Dequeue().Should().Be(3);
    }

    [Fact]
    public void Constructor_WithNullCollection_ThrowsArgumentNullException()
    {
        Action act = () => new CustomQueue<int>(collection: null);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Clear_NonEmptyQueue_ResetsQueue()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);

        queue.Clear();

        queue.Count.Should().Be(0);
        queue.IsEmpty.Should().BeTrue();
        queue.TryDequeue(out _).Should().BeFalse();
    }

    [Fact]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(42);

        var result = queue.Contains(42);

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_NonExistingItem_ReturnsFalse()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(42);

        var result = queue.Contains(99);

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_NullItemWithNullInQueue_ReturnsTrue()
    {
        var queue = new CustomQueue<string?>();
        queue.Enqueue(item: null);

        var result = queue.Contains(item: null);

        result.Should().BeTrue();
    }

    [Fact]
    public void ToArray_NonEmptyQueue_ReturnsCorrectArray()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        var result = queue.ToArray();

        result.Should().BeEquivalentTo([
            1, 2, 3,
        ]);
    }

    [Fact]
    public void ToArray_EmptyQueue_ReturnsEmptyArray()
    {
        var queue = new CustomQueue<int>();

        var result = queue.ToArray();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Enumerator_CollectionModifiedDuringEnumeration_ThrowsInvalidOperationException()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        var enumerator = queue.GetEnumerator();
        enumerator.MoveNext();

        queue.Enqueue(2);

        Action act = () => enumerator.MoveNext();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Коллекция была изменена; операция перечисления не может быть выполнена.");
    }

    [Fact]
    public void Enqueue_ManyItems_TriggersResize()
    {
        var queue = new CustomQueue<int>(4);
        for (var i = 0; i < 17; i++)
        {
            queue.Enqueue(i);
        }

        queue.Count.Should().Be(17);
        queue.Dequeue().Should().Be(0);
        queue.Dequeue().Should().Be(1);
    }

    [Fact]
    public void EnqueueDequeue_StringType_WorksCorrectly()
    {
        var queue = new CustomQueue<string>();
        queue.Enqueue("hello");
        queue.Enqueue("world");

        queue.Dequeue().Should().Be("hello");
        queue.Dequeue().Should().Be("world");
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void EnqueueDequeue_NullableType_WorksCorrectly()
    {
        var queue = new CustomQueue<int?>();
        queue.Enqueue(42);
        queue.Enqueue(item: null);

        queue.Dequeue().Should().Be(42);
        queue.Dequeue().Should().BeNull();
    }

    [Fact]
    public void Constructor_ZeroCapacity_InitializesWithDefaultCapacity()
    {
        var queue = new CustomQueue<int>(0);

        queue.Enqueue(1);
        queue.Count.Should().Be(1);
        queue.Dequeue().Should().Be(1);
    }

    [Fact]
    public void Enqueue_ExactlyAtCapacity_DoesNotResize()
    {
        var queue = new CustomQueue<int>(4);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);

        queue.Count.Should().Be(4);
        queue.Dequeue().Should().Be(1);
    }

    [Fact]
    public void TryDequeue_NonEmptyQueue_ReturnsTrueAndItem()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(42);

        var success = queue.TryDequeue(out var result);

        success.Should().BeTrue();
        result.Should().Be(42);
        queue.Count.Should().Be(0);
    }

    [Fact]
    public void TryPeek_NonEmptyQueue_ReturnsTrueAndItem()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(42);

        var success = queue.TryPeek(out var result);

        success.Should().BeTrue();
        result.Should().Be(42);
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void Clear_EmptyQueue_NoEffect()
    {
        var queue = new CustomQueue<int>();

        queue.Clear();

        queue.Count.Should().Be(0);
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Contains_EmptyQueue_ReturnsFalse()
    {
        var queue = new CustomQueue<int>();

        var result = queue.Contains(42);

        result.Should().BeFalse();
    }

    [Fact]
    public void EnqueueDequeue_WrapAroundBuffer_WorksCorrectly()
    {
        var queue = new CustomQueue<int>(4);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Dequeue();
        queue.Dequeue();
        queue.Enqueue(3);
        queue.Enqueue(4);

        queue.Dequeue().Should().Be(3);
        queue.Dequeue().Should().Be(4);
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void ToArray_WrapAroundBuffer_ReturnsCorrectOrder()
    {
        var queue = new CustomQueue<int>(4);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Dequeue();
        queue.Enqueue(3);

        var result = queue.ToArray();

        result.Should().BeEquivalentTo([
            2, 3,
        ]);
    }

    [Fact]
    public void Enumerator_EmptyQueue_DoesNotEnumerate()
    {
        var queue = new CustomQueue<int>();
        var enumerator = queue.GetEnumerator();

        var hasNext = enumerator.MoveNext();

        hasNext.Should().BeFalse();
    }

    [Fact]
    public void Enumerator_SingleItem_EnumeratesCorrectly()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(30);
        var items = queue.ToList();

        items.Should().BeEquivalentTo([30]);
    }

    [Fact]
    public void Enqueue_AfterClear_WorksCorrectly()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Clear();
        queue.Enqueue(2);

        queue.Dequeue().Should().Be(2);
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Constructor_LargeCollection_InitializesCorrectly()
    {
        var collection = Enumerable.Range(0, 100).ToList();
        var queue = new CustomQueue<int>(collection);

        queue.Count.Should().Be(100);
        queue.Dequeue().Should().Be(0);
        queue.Dequeue().Should().Be(1);
    }

    [Fact]
    public void Contains_MultipleOccurrences_ReturnsTrue()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(37);
        queue.Enqueue(37);

        var result = queue.Contains(37);

        result.Should().BeTrue();
    }

    [Fact]
    public void Dequeue_AfterMultipleEnqueuesAndDequeues_MaintainsOrder()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Dequeue();
        queue.Enqueue(3);
        queue.Dequeue();
        queue.Enqueue(4);

        queue.Dequeue().Should().Be(3);
        queue.Dequeue().Should().Be(4);
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Peek_AfterMultipleEnqueuesAndDequeues_ReturnsCorrectItem()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Dequeue();
        queue.Enqueue(3);

        queue.Peek().Should().Be(2);
        queue.Count.Should().Be(2);
    }

    [Fact]
    public void TryDequeue_AfterMultipleEnqueuesAndDequeues_WorksCorrectly()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Dequeue();
        queue.Enqueue(3);

        var success = queue.TryDequeue(out var result);

        success.Should().BeTrue();
        result.Should().Be(2);
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void TryPeek_AfterMultipleEnqueuesAndDequeues_WorksCorrectly()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Dequeue();
        queue.Enqueue(3);

        var success = queue.TryPeek(out var result);

        success.Should().BeTrue();
        result.Should().Be(2);
        queue.Count.Should().Be(2);
    }

    [Fact]
    public void Contains_CaseSensitiveString_WorksCorrectly()
    {
        var queue = new CustomQueue<string>();
        queue.Enqueue("Hello");

        queue.Contains("hello").Should().BeFalse();
        queue.Contains("Hello").Should().BeTrue();
    }

    [Fact]
    public void EnqueueDequeue_LargeNumberOfItems_WorksCorrectly()
    {
        var queue = new CustomQueue<int>();
        for (var i = 0; i < 1000; i++)
        {
            queue.Enqueue(i);
        }

        queue.Count.Should().Be(1000);
        queue.Dequeue().Should().Be(0);
        queue.Dequeue().Should().Be(1);
        queue.Count.Should().Be(998);
    }

    [Fact]
    public void ToArray_AfterMultipleOperations_ReturnsCorrectOrder()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Dequeue();
        queue.Enqueue(3);
        queue.Enqueue(4);

        var result = queue.ToArray();

        result.Should().BeEquivalentTo([2, 3, 4,]);
    }

    [Fact]
    public void Enumerator_MultipleItems_EnumeratesInCorrectOrder()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        var items = new List<int>();
        foreach (var item in queue)
        {
            items.Add(item);
        }

        items.Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void Clear_AfterMultipleOperations_ResetsCorrectly()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(1);
        queue.Dequeue();
        queue.Enqueue(2);
        queue.Enqueue(3);

        queue.Clear();

        queue.Count.Should().Be(0);
        queue.IsEmpty.Should().BeTrue();
        queue.TryDequeue(out _).Should().BeFalse();
    }

    [Fact]
    public void Constructor_EmptyCollection_InitializesEmptyQueue()
    {
        var queue = new CustomQueue<int>(new List<int>());

        queue.Count.Should().Be(0);
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Enqueue_NullReferenceType_WorksCorrectly()
    {
        var queue = new CustomQueue<string?>();
        queue.Enqueue(item: null);
        queue.Enqueue("test");

        queue.Dequeue().Should().BeNull();
        queue.Dequeue().Should().Be("test");
    }

    [Fact]
    public void Contains_AfterClear_ReturnsFalse()
    {
        var queue = new CustomQueue<int>();
        queue.Enqueue(42);
        queue.Clear();

        queue.Contains(42).Should().BeFalse();
    }

    [Fact]
    public void EnqueueDequeue_ComplexType_WorksCorrectly()
    {
        var queue = new CustomQueue<(int, string)>();
        queue.Enqueue((1, "one"));
        queue.Enqueue((2, "two"));

        queue.Dequeue().Should().Be((1, "one"));
        queue.Dequeue().Should().Be((2, "two"));
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void EnqueueDequeue_CharType_WorksCorrectly()
    {
        var queue = new CustomQueue<char>();
        queue.Enqueue('A');
        queue.Enqueue('B');

        queue.Dequeue().Should().Be('A');
        queue.Dequeue().Should().Be('B');
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Contains_CharType_ReturnsCorrectResult()
    {
        var queue = new CustomQueue<char>();
        queue.Enqueue('X');

        queue.Contains('X').Should().BeTrue();
        queue.Contains('Y').Should().BeFalse();
    }

    [Fact]
    public void ToArray_CharType_ReturnsCorrectArray()
    {
        var queue = new CustomQueue<char>();
        queue.Enqueue('A');
        queue.Enqueue('B');
        queue.Enqueue('C');

        var result = queue.ToArray();

        result.Should().BeEquivalentTo(['A', 'B', 'C']);
    }

    [Fact]
    public void TryDequeue_CharType_WorksCorrectly()
    {
        var queue = new CustomQueue<char>();
        queue.Enqueue('Z');

        var success = queue.TryDequeue(out var result);

        success.Should().BeTrue();
        result.Should().Be('Z');
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void TryPeek_CharType_WorksCorrectly()
    {
        var queue = new CustomQueue<char>();
        queue.Enqueue('Z');

        var success = queue.TryPeek(out var result);

        success.Should().BeTrue();
        result.Should().Be('Z');
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void EnqueueDequeue_BoolType_WorksCorrectly()
    {
        var queue = new CustomQueue<bool>();
        queue.Enqueue(item: true);
        queue.Enqueue(item: false);

        queue.Dequeue().Should().BeTrue();
        queue.Dequeue().Should().BeFalse();
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Contains_BoolType_ReturnsCorrectResult()
    {
        var queue = new CustomQueue<bool>();
        queue.Enqueue(item: true);

        queue.Contains(item: true).Should().BeTrue();
        queue.Contains(item: false).Should().BeFalse();
    }

    [Fact]
    public void ToArray_BoolType_ReturnsCorrectArray()
    {
        var queue = new CustomQueue<bool>();
        queue.Enqueue(item: true);
        queue.Enqueue(item: false);
        queue.Enqueue(item: true);

        var result = queue.ToArray();

        result.Should().BeEquivalentTo([true, false, true]);
    }

    [Fact]
    public void TryDequeue_BoolType_WorksCorrectly()
    {
        var queue = new CustomQueue<bool>();
        queue.Enqueue(item: false);

        var success = queue.TryDequeue(out var result);

        success.Should().BeTrue();
        result.Should().BeFalse();
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void TryPeek_BoolType_WorksCorrectly()
    {
        var queue = new CustomQueue<bool>();
        queue.Enqueue(item: true);

        var success = queue.TryPeek(out var result);

        success.Should().BeTrue();
        result.Should().BeTrue();
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void EnqueueDequeue_DoubleType_WorksCorrectly()
    {
        var queue = new CustomQueue<double>();
        queue.Enqueue(3.14);
        queue.Enqueue(2.718);

        queue.Dequeue().Should().Be(3.14);
        queue.Dequeue().Should().Be(2.718);
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Contains_DoubleType_ReturnsCorrectResult()
    {
        var queue = new CustomQueue<double>();
        queue.Enqueue(1.23);

        queue.Contains(1.23).Should().BeTrue();
        queue.Contains(4.56).Should().BeFalse();
    }

    [Fact]
    public void ToArray_DoubleType_ReturnsCorrectArray()
    {
        var queue = new CustomQueue<double>();
        queue.Enqueue(1.1);
        queue.Enqueue(2.2);
        queue.Enqueue(3.3);

        var result = queue.ToArray();

        result.Should().BeEquivalentTo([1.1, 2.2, 3.3]);
    }

    [Fact]
    public void TryDequeue_DoubleType_WorksCorrectly()
    {
        var queue = new CustomQueue<double>();
        queue.Enqueue(5.55);

        var success = queue.TryDequeue(out var result);

        success.Should().BeTrue();
        result.Should().Be(5.55);
        queue.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void TryPeek_DoubleType_WorksCorrectly()
    {
        var queue = new CustomQueue<double>();
        queue.Enqueue(7.77);

        var success = queue.TryPeek(out var result);

        success.Should().BeTrue();
        result.Should().Be(7.77);
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_CharCollection_InitializesCorrectly()
    {
        var collection = new List<char>
        {
            'A', 'B', 'C',
        };
        var queue = new CustomQueue<char>(collection);

        queue.Count.Should().Be(3);
        queue.Dequeue().Should().Be('A');
        queue.Dequeue().Should().Be('B');
        queue.Dequeue().Should().Be('C');
    }

    [Fact]
    public void Constructor_BoolCollection_InitializesCorrectly()
    {
        var collection = new List<bool>
        {
            true, false, true,
        };
        var queue = new CustomQueue<bool>(collection);

        queue.Count.Should().Be(3);
        queue.Dequeue().Should().BeTrue();
        queue.Dequeue().Should().BeFalse();
        queue.Dequeue().Should().BeTrue();
    }

    [Fact]
    public void Constructor_DoubleCollection_InitializesCorrectly()
    {
        var collection = new List<double>
        {
            1.5, 2.5, 3.5,
        };
        var queue = new CustomQueue<double>(collection);

        queue.Count.Should().Be(3);
        queue.Dequeue().Should().Be(1.5);
        queue.Dequeue().Should().Be(2.5);
        queue.Dequeue().Should().Be(3.5);
    }

    [Fact]
    public void Enumerator_CharType_EnumeratesCorrectly()
    {
        var queue = new CustomQueue<char>();
        queue.Enqueue('X');
        queue.Enqueue('Y');
        var items = new List<char>();
        foreach (var item in queue)
        {
            items.Add(item);
        }

        items.Should().BeEquivalentTo(['X', 'Y']);
    }

    [Fact]
    public void Enumerator_BoolType_EnumeratesCorrectly()
    {
        var queue = new CustomQueue<bool>();
        queue.Enqueue(item: true);
        queue.Enqueue(item: false);
        var items = new List<bool>();
        foreach (var item in queue)
        {
            items.Add(item);
        }

        items.Should().BeEquivalentTo([true, false]);
    }

    [Fact]
    public void Enumerator_DoubleType_EnumeratesCorrectly()
    {
        var queue = new CustomQueue<double>();
        queue.Enqueue(1.23);
        queue.Enqueue(4.56);
        var items = new List<double>();
        foreach (var item in queue)
        {
            items.Add(item);
        }

        items.Should().BeEquivalentTo([1.23, 4.56]);
    }
}
