using System;
using System.Collections.Generic;

namespace AliasMethod
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            var testTable = new List<Tuple<int, int>>();
            for (int i = 1; i < 4; i++)
            {
                testTable.Add(new Tuple<int, int>(i, i));
            }

            //var aliasTable = new AliasWeightTable<int>(testTable);
            var basicTable = new BasicWeightTable<int>(testTable);

            //double aliasAverage = 0;
            double basicAverage = 0;

            for (int i = 0; i < 1000000; i++)
            {
                //int aliasSample = aliasTable.Sample(random);
                int basicSample = basicTable.Sample(random);
                //aliasAverage = (aliasAverage * i + aliasSample) / (i + 1);
                basicAverage = (basicAverage * i + basicSample) / (i + 1);
            }

            Console.WriteLine($"basicAvergage = {basicAverage}");
            double theo = 14d / 6;
            Console.WriteLine($"theoAverage = {theo}");
            Console.WriteLine($"diff = {theo - basicAverage}");
            Console.ReadKey();
        }
    }
}
