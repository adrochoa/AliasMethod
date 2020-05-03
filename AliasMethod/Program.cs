using MathNet.Numerics.Distributions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AliasMethod
{
    class Program
    {
        static readonly ConcurrentBag<(double Average, long Count)> Bag = new ConcurrentBag<(double Average, long Count)>();
        static readonly ConcurrentDictionary<long, long> Dictionary = new ConcurrentDictionary<long, long>();
        static long Counter = 0;

        static void Main(string[] args)
        {
            const int COLOR_COUNT = 3;
            const int BALL_COUNT = 10;

            var list = new List<List<List<int>>>();
            list.Add(new List<List<int>>());

            var comparer = new SetComparer<int>();
            for (int count = 1; count < BALL_COUNT + 1; count++)
            {
                var e = GetPermutations(Enumerable.Range(0, COLOR_COUNT), count, comparer);
                list.Add(e);
            }

            for (int i = 0; i <list.Count; i++)
            {
                Console.WriteLine();
                foreach (var l in list[i])
                {
                    for (int j = 0; j < l.Count; j++)
                    {
                        Console.Write($"{l[j]}\t");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }

        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1)
            {
                return list.Select(t => new T[] { t });
            }
            return GetPermutations(list, length - 1).SelectMany(t => list, (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        static List<List<T>> GetPermutations<T>(IEnumerable<T> list, int length, IEqualityComparer<IEnumerable<T>> comparer)
        {
            var temp = GetPermutations<T>(list, length);
            var result = new List<List<T>>();
            foreach (var l in temp)
            {
                var test = l.ToList();
                test.Sort();
                if (!result.Contains(test, comparer))
                {
                    result.Add(test);
                }
            }

            return result;
        }

        static List<List<int>> ListCombinations(int itemCount, int choiceCount, IEqualityComparer<List<int>> comparer)
        {
            var list = new List<List<int>>();
            int combinationCount = Power(choiceCount, itemCount);
            int[] choices = new int[itemCount];
            int k = 0;
            for (int i = 0; i < combinationCount; i++)
            {
                var newList = choices.ToList();
                newList.Sort();
                if (!list.Contains(newList, comparer))
                {
                    list.Add(newList);
                }
                choices[k]++;
                if (choices[k] > choiceCount)
                {
                    choices[k] = 0;
                    k++;
                    k %= choices.Length;
                }
            }
            return list;
        }

        static int Power(int b, int exponent)
        {
            int val = 1;
            for (int i = 0; i < exponent; i++)
            {
                val *= b;
            }
            return val;
        }

        static async Task AsyncMain(string[] args)
        {
            var valueWeightPairs = new List<(int Value, int Weight)>();
            for (var i = 1; i < 4; i++)
            {
                valueWeightPairs.Add((Value: i, Weight: i));
            }

            long totalCount = 100000000;
            long processCount = Environment.ProcessorCount;

            var minPerProcess = totalCount / processCount;
            var excess = totalCount % processCount;

            var countPerProcess = new long[processCount];

            for (var i = 0; i < countPerProcess.Length; i++)
            {
                countPerProcess[i] = i < excess ? minPerProcess + 1 : minPerProcess;
            }

            Console.WriteLine("Beginning sample run...");
            var tasks = EnumerateTasks<int>(valueWeightPairs, (x, y) => x * y, (x, y) => x - y, x => x, countPerProcess);

            await Task.WhenAll(tasks);

            Console.ReadKey();
        }

        static async Task MemoryIntenseMain(string[] args)
        {
            var valueWeightPairs = new List<(int Value, int Weight)>();
            for (var i = 1; i < 4; i++)
            {
                valueWeightPairs.Add((Value: i, Weight: i));
            }

            long totalCount = 1000000;

            Console.WriteLine("Beginning sample run...");

            var tasks = EnumerateTasks<int>(valueWeightPairs, (x, y) => x * y, (x, y) => x - y, x => x, totalCount);

            await Task.WhenAll(tasks);

            Console.ReadKey();
        }

        static IEnumerable<Task> EnumerateTasks<T>(IEnumerable<(T Value, int Weight)> valueWeightPairs, Func<T, int, double> multiply, Func<T, double, double> subtract, Func<T, long> toLong, long count) where T : struct
        {
            for (long i = 0; i < count; i++)
            {
               yield return Sample(valueWeightPairs, multiply, subtract, toLong);
            }
        }

        static IEnumerable<Task> EnumerateTasks<T>(IEnumerable<(T Value, int Weight)> valueWeightPairs, Func<T, int, double> multiply, Func<T, double, double> subtract, Func<T, double> toDouble, long[] countPerProcess) where T : struct
        {
            foreach (long count in countPerProcess)
            {
                yield return Simulate(valueWeightPairs, count, multiply, subtract, toDouble);
            }
        }

        static async Task Simulate<T>(IEnumerable<(T Value, int Weight)> valueWeightPairs, long count, Func<T, int, double> multiply, Func<T, double, double> subtract, Func<T, double> toDouble) where T : struct
        {
            await Task.Run(() =>
            {
                var aliasWeightTable = new AliasWeightTable<T>(valueWeightPairs, multiply, subtract);
                double average = 0;
                for (long i = 0; i < count; i++)
                {
                    var sample = aliasWeightTable.Sample;
                    average = (average * i + toDouble(sample)) / (i + 1);
                    Interlocked.Increment(ref Counter);
                }
                Bag.Add((Average: average, Count: count));
            });
        }

        static async Task Sample<T>(IEnumerable<(T Value, int Weight)> valueWeightPairs, Func<T, int, double> multiply, Func<T, double, double> subtract, Func<T, long> toLong) where T : struct
        {
            await Task.Run(() =>
            {
                var aliasWeightTable = new AliasWeightTable<T>(valueWeightPairs, multiply, subtract);
                var sample = toLong(aliasWeightTable.Sample);
                Dictionary.AddOrUpdate(sample, 1, (x, y) => x + y);
                Interlocked.Increment(ref Counter);
            });
        }

        static void SychronousMain(string[] args)
        {
            var testTable = new List<(int Value, int Weight)>();
            for (var i = 1; i < 4; i++)
            {
                testTable.Add((Value: i, Weight: i));
            }

            var aliasTables = new List<AliasWeightTable<int>>();

            var aliasTable = new AliasWeightTable<int>(testTable, (x, y) => x * y, (x, y) => x - y);
            var basicTable = new LinearWeightTable<int>(testTable, (x, y) => x * y, (x, y) => x - y);

            double aliasAverage = 0;
            double basicAverage = 0;

            long sampleSize = 100000000;
            using (var pbar = new ProgressBar(sampleSize))
            {
                for (long i = 0; i < sampleSize; i++)
                {
                    double aliasSample = aliasTable.Sample;
                    double basicSample = basicTable.Sample;
                    aliasAverage = (aliasAverage * i + aliasSample) / (i + 1);
                    basicAverage = (basicAverage * i + basicSample) / (i + 1);
                    pbar.Update(i + 1);
                }
            }

            Console.WriteLine($"basicAvergage = {basicAverage}");
            Console.WriteLine($"aliasAvergage = {aliasAverage}");

            var (aliasMean, aliasStandardDeviation, aliasCount) = aliasTable.SummaryStats;
            var aliasScore = -Math.Abs(aliasMean - aliasAverage) / aliasStandardDeviation * Math.Sqrt(aliasCount);

            var (basicMean, basicStandardDeviation, basicCount) = basicTable.SummaryStats;
            var basicScore = -Math.Abs(basicMean - basicAverage) / basicStandardDeviation * Math.Sqrt(basicCount);

            var pValue = Normal.CDF(0, 1, basicScore) * 2;

            Console.WriteLine($"Theoretical Average = {aliasMean}");
            Console.WriteLine($"Diff = {aliasMean - aliasAverage}"); // or basicAverage
            Console.WriteLine($"pValue = {pValue}");
            Console.ReadKey();
        }
    }
}
