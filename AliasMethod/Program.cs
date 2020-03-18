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
        static long Counter = 0;

        static async Task Main(string[] args)
        {
            var testTable = new List<(int Value, int Weight)>();
            for (var i = 1; i < 4; i++)
            {
                testTable.Add((Value: i, Weight: i));
            }

            long totalCount = 100000000;
            long processCount = Environment.ProcessorCount - 1;

            var minPerProcess = totalCount / processCount;
            var excess = totalCount % processCount;

            var countPerProcess = new long[processCount];

            for (var i = 0; i < countPerProcess.Length; i++)
            {
                countPerProcess[i] = i < excess ? minPerProcess + 1 : minPerProcess;
            }

            var tasks = new List<Task>();
            Console.WriteLine("Beginning sample run...");
            foreach (int i in countPerProcess)
            {
                var task = Simulate(testTable, i, (x, y) => x * y, (x, y) => x - y, x => x);
                tasks.Add(task);
            }

            using var pbar = new ProgressBar(totalCount);
            while (tasks.Any())
            {
                pbar.Update(Counter);
                var finished = await Task.WhenAny(tasks);
                tasks.Remove(finished);
            }
            pbar.Update(Counter);

            Console.ReadKey();
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
