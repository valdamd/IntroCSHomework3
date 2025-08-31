using System.Collections;
using FluentAssertions;
using Task;
using Xunit;

namespace UnitTestsTask;

public sealed class SetTests
{
    [Fact]
    public void Add_EmptyStringToStringSet_ShouldAddSuccessfully()
    {
        var set = new Set<string>();

        var result = set.Add(string.Empty);

        result.Should().BeTrue();
        set.Should().Contain(string.Empty);
        set.Count.Should().Be(1);
    }

    [Fact]
    public void Add_DuplicateEmptyStringToStringSet_ShouldNotAddAgain()
    {
        var set = new Set<string>();
        set.Add(string.Empty);

        var result = set.Add(string.Empty);

        result.Should().BeFalse();
        set.Should().Contain(string.Empty);
        set.Count.Should().Be(1);
    }

    [Fact]
    public void Add_EmptyStringToNullableStringSet_ShouldAddSuccessfully()
    {
        var set = new Set<string?>();

        var result = set.Add(string.Empty);

        result.Should().BeTrue();
        set.Should().Contain(string.Empty);
        set.Count.Should().Be(1);
    }

    [Fact]
    public void Add_EmptyStringAndNullToNullableStringSet_ShouldTreatAsDifferentElements()
    {
        var set = new Set<string?>();

        var addEmptyString = set.Add(string.Empty);
        var addNull = set.Add(item: null);

        addEmptyString.Should().BeTrue();
        addNull.Should().BeTrue();
        set.Should().Contain(string.Empty);
        set.Should().Contain((string?)null);
        set.Count.Should().Be(2);
    }

    [Fact]
    public void Constructor_DefaultConstructor_ShouldCreateEmptySet()
    {
        var set = new Set<int>();

        set.Count.Should().Be(0);
        set.IsReadOnly.Should().BeFalse();
        set.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullCollection_ShouldThrowArgumentNullException()
    {
        Action act = () => new Set<int>(null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("collection");
    }

    [Fact]
    public void Constructor_WithEmptyCollection_ShouldCreateEmptySet()
    {
        var emptyCollection = Array.Empty<int>();

        var set = new Set<int>(emptyCollection);

        set.Count.Should().Be(0);
        set.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithSingleElementCollection_ShouldContainOneElement()
    {
        var singleElementCollection = new[]
        {
            42,
        };

        var set = new Set<int>(singleElementCollection);

        set.Count.Should().Be(1);
        set.Should().ContainSingle().Which.Should().Be(42);
    }

    [Fact]
    public void Constructor_WithCollection_ShouldAddAllUniqueElements()
    {
        var collection = new[]
        {
            1, 2, 3, 2, 1, 4,
        };

        var set = new Set<int>(collection);

        set.Count.Should().Be(4);
        set.Should().BeEquivalentTo([
            1, 2, 3, 4,
        ]);
    }

    [Fact]
    public void Constructor_WithLargeCollection_ShouldHandleCapacityCorrectly()
    {
        var largeCollection = Enumerable.Range(1, 100).ToArray();

        var set = new Set<int>(largeCollection);

        set.Count.Should().Be(100);
        set.Should().OnlyHaveUniqueItems();
        set.Should().BeEquivalentTo(largeCollection);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Add_NewElement_ShouldReturnTrueAndIncreaseCount(int item)
    {
        var set = new Set<int>();
        var initialCount = set.Count;

        var result = set.Add(item);

        result.Should().BeTrue("adding a new element should return true");
        set.Count.Should().Be(initialCount + 1);
        set.Should().Contain(item);
    }

    [Fact]
    public void Add_NullElement_ShouldHandleNullCorrectly()
    {
        var set = new Set<string?>();

        var result = set.Add(item: null);

        result.Should().BeTrue();
        set.Should().Contain((string?)null);
        set.Count.Should().Be(1);
    }

    [Fact]
    public void Add_ExistingElement_ShouldReturnFalseAndKeepSameCount()
    {
        var set = new Set<int>();
        set.Add(5);
        var initialCount = set.Count;

        var result = set.Add(5);

        result.Should().BeFalse("adding duplicate element should return false");
        set.Count.Should().Be(initialCount);
    }

    [Fact]
    public void Add_ManyElements_ShouldExpandCapacityCorrectly()
    {
        var set = new Set<int>();
        var elements = Enumerable.Range(1, 50);

        var expectation = elements.ToList();
        foreach (var element in expectation)
        {
            set.Add(element);
        }

        set.Count.Should().Be(50);
        set.Should().OnlyHaveUniqueItems();
        set.Should().BeEquivalentTo(expectation);
    }

    [Fact]
    public void ICollectionAdd_ShouldDelegateToAdd()
    {
        var set = new Set<int>();
        ICollection<int> collection = set;

        collection.Add(5);

        set.Count.Should().Be(1);
        set.Should().Contain(5);
    }

    [Theory]
    [InlineData(1)]
    [InlineData("test")]
    [InlineData(true)]
    [InlineData(false)]
    public void Contains_ExistingElement_ShouldReturnTrue<T>(T item)
    {
        var set = new Set<T>
        {
            item,
        };

        set.Contains(item).Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData("test")]
    [InlineData(true)]
    public void Contains_NonExistingElement_ShouldReturnFalse<T>(T item)
    {
        var set = new Set<T>();

        set.Contains(item).Should().BeFalse();
    }

    [Fact]
    public void Contains_NullInReferenceTypeSet_ShouldWorkCorrectly()
    {
        var set = new Set<string?>();
        set.Add(item: null);

        set.Contains(item: null).Should().BeTrue();
    }

    [Fact]
    public void Contains_EmptySet_ShouldReturnFalse()
    {
        var emptySet = new Set<int>();

        emptySet.Contains(42).Should().BeFalse();
    }

    [Fact]
    public void Remove_ExistingElement_ShouldReturnTrueAndDecreaseCount()
    {
        var set = new Set<int>
        {
            5,
        };
        var initialCount = set.Count;

        var result = set.Remove(5);

        result.Should().BeTrue("removing existing element should return true");
        set.Count.Should().Be(initialCount - 1);
        set.Should().NotContain(5);
    }

    [Fact]
    public void Remove_NonExistingElement_ShouldReturnFalseAndKeepSameCount()
    {
        var set = new Set<int>
        {
            5,
        };
        var initialCount = set.Count;

        var result = set.Remove(10);

        result.Should().BeFalse("removing non-existing element should return false");
        set.Count.Should().Be(initialCount);
    }

    [Fact]
    public void Remove_FromEmptySet_ShouldReturnFalse()
    {
        var set = new Set<int>();

        var result = set.Remove(99);

        result.Should().BeFalse();
        set.Count.Should().Be(0);
    }

    [Fact]
    public void Remove_LastElement_ShouldMakeSetEmpty()
    {
        var set = new Set<int>
        {
            13,
        };

        var result = set.Remove(13);

        result.Should().BeTrue();
        set.Should().BeEmpty();
        set.Count.Should().Be(0);
    }

    [Fact]
    public void Remove_MultipleElements_ShouldMaintainCorrectOrder()
    {
        var set = new Set<int>();
        foreach (var element in new[]
                 {
                     1, 2, 3, 4, 5,
                 })
        {
            set.Add(element);
        }

        set.Remove(3);
        set.Remove(1);

        set.Count.Should().Be(3);
        set.Should().BeEquivalentTo([
            2, 4, 5,
        ]);
    }

    [Fact]
    public void Clear_NonEmptySet_ShouldRemoveAllElements()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };

        set.Clear();

        set.Count.Should().Be(0);
        set.Should().BeEmpty();
        set.Should().NotContain([
            1, 2, 3,
        ]);
    }

    [Fact]
    public void Clear_EmptySet_ShouldDoNothing()
    {
        var set = new Set<int>();

        set.Clear();

        set.Count.Should().Be(0);
        set.Should().BeEmpty();
    }

    [Fact]
    public void Clear_AfterClear_ShouldAllowNewElements()
    {
        var set = new Set<int>
        {
            1, 2,
        };
        set.Clear();

        set.Add(3);

        set.Count.Should().Be(1);
        set.Should().ContainSingle().Which.Should().Be(3);
    }

    [Fact]
    public void CopyTo_ValidArray_ShouldCopyAllElements()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };
        var array = new int[5];

        set.CopyTo(array, 1);

        array[0].Should().Be(0);
        array.Skip(1).Take(3).Should().BeEquivalentTo([1, 2, 3]);
        array[4].Should().Be(0);
    }

    [Fact]
    public void CopyTo_EmptySet_ShouldNotModifyArray()
    {
        var set = new Set<int>();
        var array = new int[3]
        {
            1, 2, 3,
        };
        var originalArray = array.ToArray();

        set.CopyTo(array, 0);

        array.Should().BeEquivalentTo(originalArray);
    }

    [Fact]
    public void CopyTo_NullArray_ShouldThrowArgumentNullException()
    {
        var set = new Set<int>
        {
            1,
        };

        Action act = () => set.CopyTo(null!, 0);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("array");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void CopyTo_InvalidArrayIndex_ShouldThrowArgumentOutOfRangeException(int arrayIndex)
    {
        var set = new Set<int>
        {
            1,
        };
        var array = new int[3];

        Action act = () => set.CopyTo(array, arrayIndex);

        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName(nameof(arrayIndex));
    }

    [Fact]
    public void CopyTo_InsufficientSpace_ShouldThrowArgumentOutOfRangeException()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };
        var array = new int[3];

        Action act = () => set.CopyTo(array, 2);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UnionWith_NullCollection_ShouldThrowArgumentNullException()
    {
        var set = new Set<int>();

        Action act = () => set.UnionWith(null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("other");
    }

    [Fact]
    public void UnionWith_ValidCollection_ShouldAddUniqueElements()
    {
        var set = new Set<int>
        {
            1, 2,
        };
        var other = new[]
        {
            2, 3, 4,
        };

        set.UnionWith(other);

        set.Count.Should().Be(4);
        set.Should().BeEquivalentTo([
            1, 2, 3, 4,
        ]);
    }

    [Fact]
    public void IntersectWith_NullCollection_ShouldThrowArgumentNullException()
    {
        var set = new Set<int>();

        Action act = () => set.IntersectWith(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IntersectWith_EmptyCollection_ShouldMakeSetEmpty()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };

        set.IntersectWith([]);

        set.Should().BeEmpty();
    }

    [Fact]
    public void IntersectWith_ValidCollection_ShouldKeepOnlyCommonElements()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };
        var other = new[]
        {
            2, 3, 4,
        };

        set.IntersectWith(other);

        set.Count.Should().Be(2);
        set.Should().BeEquivalentTo([
            2, 3,
        ]);
        set.Should().NotContain([
            1, 4,
        ]);
    }

    [Fact]
    public void ExceptWith_NullCollection_ShouldThrowArgumentNullException()
    {
        var set = new Set<int>();

        Action act = () => set.ExceptWith(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExceptWith_EmptyCollection_ShouldNotChangeSet()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };
        var originalElements = set.ToArray();

        set.ExceptWith([]);

        set.Should().BeEquivalentTo(originalElements);
    }

    [Fact]
    public void ExceptWith_ValidCollection_ShouldRemoveSpecifiedElements()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };
        var other = new[]
        {
            2, 4,
        };

        set.ExceptWith(other);

        set.Count.Should().Be(2);
        set.Should().BeEquivalentTo([
            1, 3,
        ]);
        set.Should().NotContain(2);
    }

    [Fact]
    public void SymmetricExceptWith_NullCollection_ShouldThrowArgumentNullException()
    {
        var set = new Set<int>();

        Action act = () => set.SymmetricExceptWith(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SymmetricExceptWith_EmptyCollection_ShouldNotChangeSet()
    {
        var set = new Set<int>
        {
            1, 2,
        };
        var originalElements = set.ToArray();

        set.SymmetricExceptWith([]);

        set.Should().BeEquivalentTo(originalElements);
    }

    [Fact]
    public void SymmetricExceptWith_ValidCollection_ShouldKeepElementsInEitherButNotBoth()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };
        var other = new[]
        {
            2, 3, 4,
        };

        set.SymmetricExceptWith(other);

        set.Count.Should().Be(2);
        set.Should().BeEquivalentTo([
            1, 4,
        ]);
        set.Should().NotContain([
            2, 3,
        ]);
    }

    [Theory]
    [InlineData(new int[0], new[] { 1, 2, 3 }, true)]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2, 3 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }, true)]
    [InlineData(new[] { 1, 4 }, new[] { 1, 2, 3 }, false)]
    [InlineData(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3 }, false)]
    public void IsSubsetOf_Various_ShouldReturnCorrectResult(int[] setElements, int[] otherElements, bool expected)
    {
        var set = new Set<int>(setElements);

        set.IsSubsetOf(otherElements).Should().Be(expected);
    }

    [Fact]
    public void IsSubsetOf_NullCollection_ShouldThrowArgumentNullException()
    {
        var set = new Set<int>();

        Action act = () => set.IsSubsetOf(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new int[0], true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 4 }, false)]
    [InlineData(new int[0], new[] { 1, 2, 3 }, false)]
    public void IsSupersetOf_Various_ShouldReturnCorrectResult(int[] setElements, int[] otherElements, bool expected)
    {
        var set = new Set<int>(setElements);

        set.IsSupersetOf(otherElements).Should().Be(expected);
    }

    [Theory]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2, 3 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }, false)]
    [InlineData(new[] { 1, 4 }, new[] { 1, 2, 3 }, false)]
    [InlineData(new int[0], new[] { 1, 2, 3 }, true)]
    [InlineData(new int[0], new int[0], false)]
    public void IsProperSubsetOf_Various_ShouldReturnCorrectResult(int[] setElements, int[] otherElements, bool expected)
    {
        var set = new Set<int>(setElements);

        set.IsProperSubsetOf(otherElements).Should().Be(expected);
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }, false)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 4, 5 }, false)]
    [InlineData(new[] { 1, 2, 3 }, new int[0], true)]
    [InlineData(new int[0], new int[0], false)]
    public void IsProperSupersetOf_Various_ShouldReturnCorrectResult(int[] setElements, int[] otherElements, bool expected)
    {
        var set = new Set<int>(setElements);

        set.IsProperSupersetOf(otherElements).Should().Be(expected);
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new[] { 3, 4, 5 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 4, 5, 6 }, false)]
    [InlineData(new int[0], new[] { 1, 2, 3 }, false)]
    [InlineData(new[] { 1, 2, 3 }, new int[0], false)]
    [InlineData(new int[0], new int[0], false)]
    public void Overlaps_Various_ShouldReturnCorrectResult(int[] setElements, int[] otherElements, bool expected)
    {
        var set = new Set<int>(setElements);

        set.Overlaps(otherElements).Should().Be(expected);
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new[] { 3, 2, 1 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 3, 3 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 4 }, false)]
    [InlineData(new int[0], new int[0], true)]
    [InlineData(new[] { 1 }, new int[0], false)]
    public void SetEquals_Various_ShouldReturnCorrectResult(int[] setElements, int[] otherElements, bool expected)
    {
        var set = new Set<int>(setElements);

        set.SetEquals(otherElements).Should().Be(expected);
    }

    [Fact]
    public void GetEnumerator_EmptySet_ShouldNotIterate()
    {
        var set = new Set<int>();
        var result = set.ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_SingleElement_ShouldIterateOnce()
    {
        var set = new Set<int>();
        set.Add(42);
        var result = set.ToList();

        result.Should().ContainSingle().Which.Should().Be(42);
    }

    [Fact]
    public void GetEnumerator_NonEmptySet_ShouldIterateAllElements()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };
        var result = set.ToList();

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo([
            1, 2, 3,
        ]);
    }

    [Fact]
    public void GetEnumerator_ModifiedDuringIteration_ShouldThrowInvalidOperationException()
    {
        var set = new Set<int>
        {
            1, 2, 3,
        };

        Action act = () =>
        {
            foreach (var item in set)
            {
                if (item == 2)
                {
                    set.Add(4);
                }
            }
        };

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Коллекция была изменена.");
    }

    [Fact]
    public void GetEnumerator_Reset_ShouldRestartIteration()
    {
        var set = new Set<int>
        {
            1, 2,
        };
        using var enumerator = set.GetEnumerator();

        enumerator.MoveNext();
        var firstValue = enumerator.Current;
        enumerator.Reset();
        enumerator.MoveNext();
        var resetValue = enumerator.Current;

        resetValue.Should().Be(firstValue);
    }

    [Fact]
    public void GetEnumerator_ResetAfterModification_ShouldThrowInvalidOperationException()
    {
        var set = new Set<int>
        {
            1,
        };
        using var enumerator = set.GetEnumerator();
        set.Add(2);

        var act = () => enumerator.Reset();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetEnumerator_NonGeneric_ShouldWorkCorrectly()
    {
        IEnumerable nonGenericSet = new Set<int>
        {
            1, 2,
        };
        var result = nonGenericSet.Cast<object?>().ToList();

        result.Should().HaveCount(2);
        result.Should().AllBeOfType<int>();
    }

    [Fact]
    public void Set_WithStrings_ShouldWorkCorrectly()
    {
        var set = new Set<string>
        {
            "Hello", "World", "Hello",
        };

        set.Count.Should().Be(2);
        set.Should().BeEquivalentTo("Hello", "World");
    }

    [Fact]
    public void Set_WithBooleans_ShouldWorkCorrectly()
    {
        var set = new Set<bool>();

        var addTrue1 = set.Add(item: true);
        var addFalse = set.Add(item: false);
        var addTrue2 = set.Add(item: true);

        addTrue1.Should().BeTrue();
        addFalse.Should().BeTrue();
        addTrue2.Should().BeFalse();
        set.Count.Should().Be(2);
        set.Should().BeEquivalentTo([
            true, false,
        ]);
    }

    [Fact]
    public void Set_WithFloatingPoint_ShouldWorkCorrectly()
    {
        var set = new Set<double>
        {
            3.14, 2.71, 3.14,
        };

        set.Count.Should().Be(2);
        set.Should().BeEquivalentTo([
            3.14, 2.71,
        ]);
    }

    [Fact]
    public void Set_WithNullableTypes_ShouldWorkCorrectly()
    {
        var set = new Set<int?>
        {
            null, 15, null,
        };

        set.Count.Should().Be(2);
        set.Should().BeEquivalentTo(new int?[]
        {
            null, 15,
        });
    }

    [Fact]
    public void Set_WithCustomObjects_ShouldUseDefaultEqualityComparer()
    {
        var set = new Set<PersonTest>();
        var person1 = new PersonTest
        {
            Id = 1, Name = "John",
        };
        var person2 = new PersonTest
        {
            Id = 1, Name = "John",
        };
        var person3 = new PersonTest
        {
            Id = 2, Name = "Jane",
        };

        set.Add(person1);
        set.Add(person2);
        set.Add(person3);

        set.Count.Should().Be(3);
        set.Should().Contain(person1);
        set.Should().Contain(person2);
        set.Should().Contain(person3);
    }

    [Fact]
    public void TrimExcess_OverAllocatedSet_ShouldReduceCapacity()
    {
        var set = new Set<int>();
        for (var i = 0; i < 20; i++)
        {
            set.Add(i);
        }

        for (var i = 5; i < 20; i++)
        {
            set.Remove(i);
        }

        set.TrimExcess();

        set.Count.Should().Be(5);
        set.Should().BeEquivalentTo([
            0, 1, 2, 3, 4,
        ]);
    }

    [Fact]
    public void TrimExcess_EmptySet_ShouldNotThrow()
    {
        var set = new Set<int>();

        Action act = () => set.TrimExcess();

        act.Should().NotThrow();
    }

    [Fact]
    public void TrimExcess_SingleElementSet_ShouldNotThrow()
    {
        var set = new Set<int>();
        set.Add(42);

        Action act = () => set.TrimExcess();

        act.Should().NotThrow();
        set.Should().ContainSingle().Which.Should().Be(42);
    }

    [Fact]
    public void GetTrimExcessThreshold_VariousCapacities_ShouldReturnCorrectThreshold()
    {
        var set = new Set<int>();

        set.GetTrimExcessThreshold().Should().Be(16);

        for (var i = 0; i < 30; i++)
        {
            set.Add(i);
        }

        set.GetTrimExcessThreshold().Should().Be(16);
    }

    [Fact]
    public void Set_WithZeroElements_ShouldBeEmpty()
    {
        var set = new Set<int>();

        set.Count.Should().Be(0);
        set.Should().BeEmpty();
        set.Should().NotContain(0);
        set.Should().NotContain(1);
        set.Should().NotContain(-1);
    }

    [Fact]
    public void Set_AddRemoveSameElement_ShouldEndUpEmpty()
    {
        var set = new Set<int>();

        var added = set.Add(12);
        var removed = set.Remove(12);

        added.Should().BeTrue();
        removed.Should().BeTrue();
        set.Should().BeEmpty();
        set.Count.Should().Be(0);
    }

    [Fact]
    public void Set_AddMaxIntValue_ShouldWork()
    {
        var set = new Set<int>();

        var result = set.Add(int.MaxValue);

        result.Should().BeTrue();
        set.Should().Contain(int.MaxValue);
        set.Count.Should().Be(1);
    }

    [Fact]
    public void Set_AddMinIntValue_ShouldWork()
    {
        var set = new Set<int>();

        var result = set.Add(int.MinValue);

        result.Should().BeTrue();
        set.Should().Contain(int.MinValue);
        set.Count.Should().Be(1);
    }

    [Fact]
    public void Set_WithLargeCapacity_ShouldHandleCorrectly()
    {
        var set = new Set<int>();
        var largeRange = Enumerable.Range(0, 1000);

        var expectation = largeRange.ToList();
        foreach (var item in expectation)
        {
            set.Add(item);
        }

        set.Count.Should().Be(1000);
        set.Should().OnlyHaveUniqueItems();
        set.Should().BeEquivalentTo(expectation);
    }

    [Fact]
    public void StressTest_LargeNumberOfOperations_ShouldMaintainConsistency()
    {
        var set = new Set<int>();
        var random = new Random(45);
        var expectedElements = new HashSet<int>();

        for (var i = 0; i < 1000; i++)
        {
            var value = random.Next(0, 500);
            var added = set.Add(value);
            var expectedAdded = expectedElements.Add(value);

            added.Should().Be(expectedAdded, $"Add operation for value {value} should match expected result");
        }

        set.Count.Should().Be(expectedElements.Count);
        foreach (var element in expectedElements)
        {
            set.Should().Contain(element, $"Set should contain element {element}");
        }
    }

    [Fact]
    public void StressTest_AddRemoveOperations_ShouldMaintainConsistency()
    {
        var set = new Set<int>();
        var random = new Random(123);
        const int operations = 1000;

        for (var i = 0; i < operations; i++)
        {
            var value = random.Next(0, 100);
            var operation = random.Next(0, 2);

            if (operation == 0)
            {
                set.Add(value);
                var shouldContain = set.Contains(value);
                shouldContain.Should().BeTrue($"After adding {value}, set should contain it");
            }
            else
            {
                var countBefore = set.Count;
                var wasRemoved = set.Remove(value);
                var shouldNotContain = !set.Contains(value);

                shouldNotContain.Should().BeTrue($"After removing {value}, set should not contain it");

                if (wasRemoved)
                {
                    set.Count.Should().Be(countBefore - 1, "Count should decrease by 1 when element is removed");
                }
                else
                {
                    set.Count.Should().Be(countBefore, "Count should not change when removing non-existent element");
                }
            }
        }
    }

    [Fact]
    public void NullHandling_WithReferenceTypes_ShouldWorkCorrectly()
    {
        var set = new Set<string?>();

        set.Add(item: null).Should().BeTrue("adding null to empty set should return true");
        set.Add(item: null).Should().BeFalse("adding duplicate null should return false");
        set.Contains(item: null).Should().BeTrue("set should contain null after adding it");
        set.Remove(item: null).Should().BeTrue("removing existing null should return true");
        set.Contains(item: null).Should().BeFalse("set should not contain null after removing it");
    }

    [Fact]
    public void NullHandling_MixedWithActualValues_ShouldWorkCorrectly()
    {
        var set = new Set<string?>
        {
            "hello", null, "world", null,
        };

        set.Count.Should().Be(3);
        set.Should().BeEquivalentTo("hello", null, "world");
    }

    [Fact]
    public void Set_StringComparison_ShouldBeCaseSensitive()
    {
        var set = new Set<string>
        {
            "Hello", "HELLO", "hello",
        };

        set.Count.Should().Be(3, "string comparison should be case-sensitive by default");
        set.Should().BeEquivalentTo("Hello", "HELLO", "hello");
    }

    [Fact]
    public void Set_NumericTypes_ShouldDistinguishTypes()
    {
        var intSet = new Set<int>();
        var doubleSet = new Set<double>();

        intSet.Add(52);
        doubleSet.Add(52.0);

        intSet.Should().Contain(52);
        doubleSet.Should().Contain(52.0);
        intSet.Count.Should().Be(1);
        doubleSet.Count.Should().Be(1);
    }

    [Fact]
    public void Set_Operations_WithNullArguments_ShouldThrowAppropriateExceptions()
    {
        var set = new Set<int>();

        var unionAct = () => set.UnionWith(null!);
        unionAct.Should().Throw<ArgumentNullException>().WithParameterName("other");

        var intersectAct = () => set.IntersectWith(null!);
        intersectAct.Should().Throw<ArgumentNullException>();

        var exceptAct = () => set.ExceptWith(null!);
        exceptAct.Should().Throw<ArgumentNullException>();

        var symmetricExceptAct = () => set.SymmetricExceptWith(null!);
        symmetricExceptAct.Should().Throw<ArgumentNullException>();

        var isSubsetAct = () => set.IsSubsetOf(null!);
        isSubsetAct.Should().Throw<ArgumentNullException>();

        var isSupersetAct = () => set.IsSupersetOf(null!);
        isSupersetAct.Should().Throw<ArgumentNullException>();

        var isProperSubsetAct = () => set.IsProperSubsetOf(null!);
        isProperSubsetAct.Should().Throw<ArgumentNullException>();

        var isProperSupersetAct = () => set.IsProperSupersetOf(null!);
        isProperSupersetAct.Should().Throw<ArgumentNullException>();

        var overlapsAct = () => set.Overlaps(null!);
        overlapsAct.Should().Throw<ArgumentNullException>();

        var setEqualsAct = () => set.SetEquals(null!);
        setEqualsAct.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BooleanSet_ShouldHandleAllLogicalValues()
    {
        var set = new Set<bool>();

        var addTrue = set.Add(item: true);
        var addFalse = set.Add(item: false);
        var addTrueDuplicate = set.Add(item: true);

        addTrue.Should().BeTrue("adding true to empty set should succeed");
        addFalse.Should().BeTrue("adding false to set should succeed");
        addTrueDuplicate.Should().BeFalse("adding duplicate true should fail");

        set.Count.Should().Be(2);
        set.Should().Contain(expected: true);
        set.Should().Contain(expected: false);

        set.Contains(item: true).Should().BeTrue();
        set.Contains(item: false).Should().BeTrue();
    }

    [Fact]
    public void BooleanSet_Operations_ShouldWorkLogically()
    {
        var set1 = new Set<bool>();
        var set2 = new Set<bool>();

        set1.Add(item: true);
        set2.Add(item: false);
        set2.Add(item: true);

        set1.UnionWith(set2);

        set1.Count.Should().Be(2);
        set1.Should().BeEquivalentTo([
            true, false,
        ]);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1000000)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000000)]
    [InlineData(int.MaxValue)]
    public void NumericSet_ShouldHandleFullIntRange(int value)
    {
        var set = new Set<int>();

        var added = set.Add(value);

        added.Should().BeTrue();
        set.Should().Contain(value);
        set.Count.Should().Be(1);
    }

    [Fact]
    public void NumericSet_WithPositiveAndNegativeNumbers_ShouldDistinguishCorrectly()
    {
        var set = new Set<int>
        {
            5, -5, 0, 5,
        };

        set.Count.Should().Be(3);
        set.Should().BeEquivalentTo([
            5, -5, 0,
        ]);

        set.Contains(5).Should().BeTrue();
        set.Contains(-5).Should().BeTrue();
        set.Contains(0).Should().BeTrue();
    }

    [Fact]
    public void FloatingPointSet_ShouldHandleSpecialValues()
    {
        var set = new Set<double>
        {
            double.NaN, double.PositiveInfinity, double.NegativeInfinity, 0.0, -0.0, double.NaN,
        };

        set.Count.Should().Be(4);
        set.Should().Contain(double.NaN);
        set.Should().Contain(double.PositiveInfinity);
        set.Should().Contain(double.NegativeInfinity);
        set.Should().Contain(0.0);
    }

    [Fact]
    public void Set_ModificationDuringEnumeration_ShouldFailFast()
    {
        var set = new Set<int>();
        for (var i = 0; i < 10; i++)
        {
            set.Add(i);
        }

        var act = () =>
        {
            foreach (var item in set)
            {
                if (item == 5)
                {
                    set.Remove(item);
                }
            }
        };

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*была изменена*");
    }

    [Fact]
    public void Set_ClearDuringEnumeration_ShouldFailFast()
    {
        var set = new Set<int>();
        for (var i = 0; i < 5; i++)
        {
            set.Add(i);
        }

        var act = () =>
        {
            foreach (var item in set)
            {
                if (item == 2)
                {
                    set.Clear();
                }
            }
        };

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ComplexScenario_MultipleSetOperations_ShouldMaintainConsistency()
    {
        var set1 = new Set<int>([
            1, 2, 3, 4, 5,
        ]);
        var set2 = new Set<int>([
            4, 5, 6, 7, 8,
        ]);
        var set3 = new Set<int>([
            1, 3, 5, 7, 9,
        ]);

        set1.IntersectWith(set2);
        set1.UnionWith(set3);
        set1.ExceptWith([
            3, 7,
        ]);

        set1.Count.Should().Be(4);
        set1.Should().BeEquivalentTo([
            1, 4, 5, 9,
        ]);
    }

    [Fact]
    public void ComplexScenario_SetRelationships_ShouldBeCorrect()
    {
        var universalSet = new Set<int>(Enumerable.Range(1, 10));
        var evenSet = new Set<int>([
            2, 4, 6, 8, 10,
        ]);
        var smallSet = new Set<int>([2, 4]);

        smallSet.IsSubsetOf(evenSet).Should().BeTrue();
        smallSet.IsProperSubsetOf(evenSet).Should().BeTrue();
        evenSet.IsSupersetOf(smallSet).Should().BeTrue();
        evenSet.IsProperSupersetOf(smallSet).Should().BeTrue();

        universalSet.IsSupersetOf(evenSet).Should().BeTrue();
        evenSet.IsSubsetOf(universalSet).Should().BeTrue();

        evenSet.Overlaps(universalSet).Should().BeTrue();
        smallSet.Overlaps(evenSet).Should().BeTrue();

        var oddSet = new Set<int>([1, 3, 5, 7, 9]);
        evenSet.Overlaps(oddSet).Should().BeFalse();
    }
}
#pragma warning disable MA0048
public sealed class PersonTest
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
#pragma warning restore MA0048
