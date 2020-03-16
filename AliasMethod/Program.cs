﻿using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace AliasMethod
{
    class Program
    {
        static readonly ConcurrentBag<(double Average, long Count)> Bag = new ConcurrentBag<(double Average, long Count)>();
        static long CurrentCount = 0;

        static void ConcurrentMain(string[] args)
        {
            var testTable = new List<(int Value, int Weight)>();
            for (int i = 1; i < 4; i++)
            {
                testTable.Add((Value: i, Weight: i));
            }

            long totalSample = 100000000;
            long processCount = Environment.ProcessorCount;

            long minPerProcess = totalSample / processCount;
            long excess = totalSample % processCount;

            var countPerProcess = new long[processCount];

            for (int i = 0; i < countPerProcess.Length; i++)
            {
                countPerProcess[i] = i < excess ? minPerProcess + 1 : minPerProcess;
            }

            if (countPerProcess.Sum() != totalSample)
            {
                throw new InvalidProgramException();
            }

            List<Task> tasks = new List<Task>();
            Console.WriteLine("Beginning sample run...");
            foreach (int i in countPerProcess)
            {
                Task.Run(() => Simulate(testTable, i, (x, y) => x * y, (x, y) => x - y, x => x));
            }

            //long currentCount;
            //while ((currentCount = Interlocked.Read(ref CurrentCount)) < totalSample) { }
            using (var pbar = new ProgressBar(totalSample))
            {
                long currentCount;
                while ((currentCount = Interlocked.Read(ref CurrentCount)) < totalSample)
                {
                    pbar.Update(currentCount);
                }
            }
            Console.ReadKey();
        }

        static void Simulate<T>(IEnumerable<(T Value, int Weight)> valueWeightPairs, long count, Func<T, int, double> multiply, Func<T, double, double> subtract, Func<T, double> toDouble) where T : struct
        {
            var aliasWeightTable = new AliasWeightTable<T>(valueWeightPairs, multiply, subtract);
            double average = 0;
            for (long i = 0; i < count; i++)
            {
                T sample = aliasWeightTable.Sample;
                average = (average * i + toDouble(sample)) / (i + 1);
                Interlocked.Increment(ref CurrentCount);
            }
            Bag.Add((Average: average, Count: count));
        }

        static void Main(string[] args)
        {
            var testTable = new List<(int Value, int Weight)>();
            for (int i = 1; i < 4; i++)
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

            var aliasSummaryStats = aliasTable.SummaryStats;
            var aliasScore = -Math.Abs(aliasSummaryStats.Mean - aliasAverage) / aliasSummaryStats.StandardDeviation * Math.Sqrt(aliasSummaryStats.Count);

            var basicSummaryStats = basicTable.SummaryStats;
            var basicScore = -Math.Abs(basicSummaryStats.Mean - basicAverage) / basicSummaryStats.StandardDeviation * Math.Sqrt(basicSummaryStats.Count);

            var pValue = Normal.CDF(0, 1, basicScore) * 2;

            Console.WriteLine($"Theoretical Average = {aliasSummaryStats.Mean}");
            Console.WriteLine($"Diff = {aliasSummaryStats.Mean - aliasAverage}"); // or basicAverage
            Console.WriteLine($"pValue = {pValue}");
            Console.ReadKey();
        }
    }
}
