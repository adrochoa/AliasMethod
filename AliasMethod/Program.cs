using MathNet.Numerics.Distributions;
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

            var aliasTable = new AliasWeightTable<double>(testTable);//, (x, y) => x * y);
            var basicTable = new BasicWeightTable<double>(testTable);//, (x, y) => x * y);

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

            var summaryStats = new DescriptiveStatistics(aliasTable, true);
            var sd = Statistics.PopulationStandardDeviation(aliasTable);
            var score = Math.Abs(summaryStats.Mean - aliasAverage) / sd * Math.Sqrt(summaryStats.Count);

            //var summaryStats = new DescriptiveStatistics(basicTable, true);
            //var sd = Statistics.PopulationStandardDeviation(basicTable);
            //var score = Math.Abs(summaryStats.Mean - basicAverage) / sd * Math.Sqrt(summaryStats.Count);

            var pValue = Normal.CDF(0, 1, score) * 2;

            Console.WriteLine($"Theoretical Average = {summaryStats.Mean}");
            Console.WriteLine($"Diff = {summaryStats.Mean - aliasAverage}"); // or basicAverage
            Console.WriteLine($"pValue = {pValue}");
            Console.ReadKey();
        }
    }
}
