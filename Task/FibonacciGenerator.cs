using BenchmarkDotNet.Attributes;

namespace Task;

public static class FibonacciGenerator
{
    private static readonly long SequenceLength = 10000;

    [Benchmark]
    public static IEnumerable<long> GenerateFibonacciSequence() => GenerateFibonacciSequence(SequenceLength);

    public static IEnumerable<long> GenerateFibonacciSequence(long sequencesLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequencesLength);

        long index = 0;
        var fibonacciSequence = new List<long>();
        foreach (var generate in GetSequence())
        {
           fibonacciSequence.Add(generate);
           index++;
           if (index == sequencesLength)
           {
               break;
           }
        }

        return fibonacciSequence;
    }

    public static IEnumerable<long> GetSequence()
    {
        long first = 0;
        long second = 1;

        yield return first;
        yield return second;
        for (long i = 2; i < SequenceLength; i++)
        {
            var temp = first;
            first = second;
            second = temp + second;
            yield return second;
        }
    }
}
