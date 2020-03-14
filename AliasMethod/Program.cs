﻿using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AliasMethod
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            var testTable = new List<Tuple<double, int>>();
            for (int i = 1; i < 4; i++)
            {
                testTable.Add(new Tuple<double, int>(i, i));
            }

            var aliasTable = new AliasWeightTable<double>(testTable, (x, y) => x * y, (x, y) => x - y);
            var basicTable = new BasicWeightTable<double>(testTable, (x, y) => x * y, (x, y) => x - y);

            double aliasAverage = 0;
            double basicAverage = 0;

            for (int i = 0; i < 100000000; i++)
            {
                double aliasSample = aliasTable.Sample(random);
                double basicSample = basicTable.Sample(random);
                aliasAverage = (aliasAverage * i + aliasSample) / (i + 1);
                basicAverage = (basicAverage * i + basicSample) / (i + 1);
            }

            Console.WriteLine($"basicAvergage = {basicAverage}");
            Console.WriteLine($"aliasAvergage = {aliasAverage}");

            var aliasSummary = new DescriptiveStatistics(aliasTable, true);
            var aliasSD = Statistics.PopulationStandardDeviation(aliasTable);
            var aliasScore = Math.Abs(aliasSummary.Mean - aliasAverage) / aliasSD * Math.Sqrt(aliasSummary.Count);

            var basicSummary = new DescriptiveStatistics(basicTable, true);
            var basicSD = Statistics.PopulationStandardDeviation(basicTable);
            var basicScore = Math.Abs(basicSummary.Mean - basicAverage) / basicSD * Math.Sqrt(basicSummary.Count);

            var other = aliasTable.SummaryStats;
            var sum = basicTable.SummaryStats;

            var pValue = Normal.CDF(0, 1, aliasScore) * 2;

            Console.WriteLine($"Theoretical Average = {aliasSummary.Mean}");
            Console.WriteLine($"Diff = {aliasSummary.Mean - aliasAverage}"); // or basicAverage
            Console.WriteLine($"pValue = {pValue}");
            Console.ReadKey();
        }
    }
}
