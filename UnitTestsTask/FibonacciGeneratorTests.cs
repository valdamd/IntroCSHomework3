using System.Diagnostics;
using FluentAssertions;
using Task;
using Xunit;

namespace UnitTestsTask;

public sealed class FibonacciGeneratorTests
{
    [Fact]
    public void GenerateFibonacciSequence_WithSmallLength_ShouldReturnCorrectSequence()
    {
        const long length = 10;
        var expectedSequence = new long[]
        {
            0, 1, 1, 2, 3, 5, 8, 13, 21, 34,
        };

        var result = FibonacciGenerator.GenerateFibonacciSequence(length);

        var enumerable = result.ToList();
        enumerable.Should().NotBeNull();
        enumerable.Should().HaveCount(10);
        enumerable.Should().Equal(expectedSequence);
    }

    [Fact]
    public void GenerateFibonacciSequence_WithMediumLength_ShouldFollowFibonacciRule()
    {
        const long length = 20;

        var result = FibonacciGenerator.GenerateFibonacciSequence(length).ToArray();

        result.Should().HaveCount(20);

        for (var i = 2; i < result.Length; i++)
        {
            var expected = result[i - 1] + result[i - 2];
            result[i].Should().Be(expected,
                $"F({i}) должно равняться F({i - 1}) + F({i - 2}) = {result[i - 1]} + {result[i - 2]}");
        }
    }

    [Theory]
    [InlineData(1, new long[]
    {
        0,
    })]
    [InlineData(2, new long[]
    {
        0, 1,
    })]
    [InlineData(3, new long[]
    {
        0, 1, 1,
    })]
    [InlineData(5, new long[]
    {
        0, 1, 1, 2, 3,
    })]
    public void GenerateFibonacciSequence_WithVariousLengths_ShouldReturnExpectedResults(
        long length, long[] expected)
    {
        var result = FibonacciGenerator.GenerateFibonacciSequence(length);

        var enumerable = result.ToList();
        enumerable.Should().Equal(expected);
        enumerable.Should().HaveCount((int)length);
    }

    [Fact]
    public void GetSequence_ShouldStartWithZeroAndOne()
    {
        var result = FibonacciGenerator.GetSequence().Take(2).ToArray();

        result.Should().HaveCount(2);
        result[0].Should().Be(0, "первое число Фибоначчи должно быть 0");
        result[1].Should().Be(1, "второе число Фибоначчи должно быть 1");
    }

    [Fact]
    public void GetSequence_ShouldGenerateCorrectLargeNumbers()
    {
        var knownValues = new Dictionary<int, long>
        {
            [10] = 55,
            [15] = 610,
            [20] = 6765,
            [30] = 832040,
            [40] = 102334155,
        };

        var sequence = FibonacciGenerator.GetSequence().Take(41).ToArray();

        foreach (var kvp in knownValues)
        {
            sequence[kvp.Key].Should().Be(kvp.Value,
                $"F({kvp.Key}) должно равняться {kvp.Value}");
        }
    }

    [Fact]
    public void GenerateFibonacciSequence_WithZeroLength_ShouldThrowArgumentOutOfRangeException()
    {
        Action act = () => FibonacciGenerator.GenerateFibonacciSequence(0);

        act.Should().Throw<ArgumentOutOfRangeException>(
                "нулевая длина должна приводить к ArgumentOutOfRangeException, " +
                "так как код использует ThrowIfNegativeOrZero")
            .And.ParamName.Should().Be("sequencesLength");
    }

    [Fact]
    public void GenerateFibonacciSequence_WithLengthOne_ShouldReturnOnlyZero()
    {
        var result = FibonacciGenerator.GenerateFibonacciSequence(1);

        var enumerable = result.ToList();
        enumerable.Should().NotBeNull();
        enumerable.Should().HaveCount(1);
        enumerable.Should().ContainSingle(x => x == 0);
        enumerable.First().Should().Be(0);
    }

    [Fact]
    public void GenerateFibonacciSequence_WithLengthTwo_ShouldReturnZeroAndOne()
    {
        var result = FibonacciGenerator.GenerateFibonacciSequence(2);

        var enumerable = result.ToList();
        enumerable.Should().NotBeNull();
        enumerable.Should().HaveCount(2);
        enumerable.Should().Equal(0, 1);
    }

    [Fact]
    public void GetSequence_WithLargeIterations_ShouldNotThrowOverflow()
    {
        const int largeCount = 50;

        Action act = () => FibonacciGenerator.GetSequence().Take(largeCount).ToList();

        act.Should().NotThrow("генератор должен корректно работать с большими числами в разумных пределах");
    }

    [Fact]
    public void GetSequence_ShouldProduceIncreasingSequence()
    {
        const int count = 20;

        var result = FibonacciGenerator.GetSequence().Take(count).ToArray();

        result.Should().HaveCount(count);

        for (var i = 2; i < result.Length - 1; i++)
        {
            result[i + 1].Should().BeGreaterThan(result[i],
                $"F({i + 1}) должно быть больше F({i}) начиная с индекса 2");
        }
    }

    [Fact]
    public void GenerateFibonacciSequence_WithNegativeLength_CurrentBehavior_ShouldThrowArgumentOutOfRangeException()
    {
        const long negativeLength = -5;

        Action act = () => FibonacciGenerator.GenerateFibonacciSequence(negativeLength);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>(
            "ArgumentOutOfRangeException.ThrowIfNegative корректно обрабатывает отрицательные значения");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(long.MinValue)]
    public void GenerateFibonacciSequence_WithNegativeLengths_ShouldThrowArgumentOutOfRangeException(long negativeLength)
    {
        Action act = () => FibonacciGenerator.GenerateFibonacciSequence(negativeLength);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>(
            $"отрицательная длина {negativeLength} должна приводить к ArgumentOutOfRangeException");
    }

    [Fact]
    public void GenerateFibonacciSequence_WithVeryLargeLength_ShouldEventuallyOverflow()
    {
        const int overflowTestCount = 100;

        var sequence = FibonacciGenerator.GetSequence().Take(overflowTestCount).ToList();

        sequence.Should().HaveCount(overflowTestCount, "должны получить запрошенное количество элементов");

        var lastNumbers = sequence.Skip(90).ToArray();
        var hasOverflowIndicators = lastNumbers.Any(x => x < 0);

        hasOverflowIndicators.Should().BeTrue(
            "при генерации 100 чисел Фибоначчи должно произойти переполнение long, " +
            "что приведет к отрицательным значениям в конце последовательности");
    }

    [Fact]
    public void GetSequence_MultipleEnumerations_ShouldProduceSameResults()
    {
        var sequence = FibonacciGenerator.GetSequence();

        var enumerable = sequence.ToList();
        var firstEnumeration = enumerable.Take(10).ToArray();
        var secondEnumeration = enumerable.Take(10).ToArray();

        firstEnumeration.Should().Equal(secondEnumeration,
            "повторные перечисления должны давать одинаковые результаты");
    }

    [Fact]
    public void GenerateFibonacciSequence_WithModerateLengths_ShouldCompleteInReasonableTime()
    {
        const long moderateLength = 1000;
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var result = FibonacciGenerator.GenerateFibonacciSequence(moderateLength);
        var materializedResult = result.ToList();
        stopwatch.Stop();

        materializedResult.Should().HaveCount(1000);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000,
            "генерация 1000 чисел должна занимать менее 1 секунды");
    }

    [Fact]
    public void GenerateFibonacciSequence_ResultType_ShouldBeCorrect()
    {
        var result = FibonacciGenerator.GenerateFibonacciSequence(5);

        var enumerable = result.ToList();
        enumerable.Should().BeAssignableTo<IEnumerable<long>>(
            "результат должен быть совместим с IEnumerable<long>");
        enumerable.Should().BeAssignableTo<ICollection<long>>(
            "результат должен быть коллекцией");

        enumerable.Should().AllBeOfType<long>("все элементы должны быть типа long");
    }

    [Fact]
    public void GenerateFibonacciSequence_WithLargeValidLength_ShouldReturnCorrectCount()
    {
        const long largeLength = 10000;

        var result = FibonacciGenerator.GenerateFibonacciSequence(largeLength);

        result.Should().HaveCount(10000, "количество элементов должно соответствовать запрошенной длине");
    }

    [Fact]
    public void GetSequence_ShouldBeEnumerableMultipleTimes()
    {
        var sequence = FibonacciGenerator.GetSequence();

        var enumerable = sequence.ToList();
        var first5 = enumerable.Take(5).ToArray();
        first5.Should().Equal(0, 1, 1, 2, 3);

        var second3 = enumerable.Take(3).ToArray();
        second3.Should().Equal(0, 1, 1);
    }

    [Fact]
    public void GenerateFibonacciSequence_MemoryUsage_ShouldBeReasonable()
    {
        const long length = 1000;
        var initialMemory = GC.GetTotalMemory(forceFullCollection: true);

        var result = FibonacciGenerator.GenerateFibonacciSequence(length);
        _ = result.ToList();

        var finalMemory = GC.GetTotalMemory(forceFullCollection: false);
        var memoryUsed = finalMemory - initialMemory;

        memoryUsed.Should().BeLessThan(1024 * 1024,
            $"использование памяти для 1000 элементов должно быть разумным, использовано: {memoryUsed} байт");
    }

    [Fact]
    public void FibonacciSequence_ShouldHaveCorrectMathematicalProperties()
    {
        const int testLength = 15;
        var sequence = FibonacciGenerator.GenerateFibonacciSequence(testLength).ToArray();

        sequence[0].Should().Be(0);
        sequence[1].Should().Be(1);

        for (var i = 10; i < sequence.Length - 1; i++)
        {
            if (sequence[i] != 0)
            {
                var ratio = (double)sequence[i + 1] / sequence[i];
                ratio.Should().BeApproximately(1.618, 0.01,
                    $"отношение F({i + 1})/F({i}) должно приближаться к золотому сечению");
            }
        }

        var sumFirst10 = sequence.Take(10).Sum();
        var f12MinusOne = sequence[11] - 1;
        sumFirst10.Should().Be(f12MinusOne,
            "сумма первых 10 чисел должна равняться F(12) - 1");
    }
}
