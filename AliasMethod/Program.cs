using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;

namespace AliasMethod
{
    class Program
    {
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

            long sampleSize = 10000000;
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
