using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AliasMethod
{
    abstract class AbstractWeightTable<T> : IWeightTable<T> where T : struct
    {
        //protected readonly List<Tuple<T, int>> MasterTable = new List<Tuple<T, int>>();
        protected readonly List<(T Value, int Weight)> MasterTable = new List<(T Value, int Weight)>();
        protected readonly List<(T Value, int Weight)> Table = new List<(T Value, int Weight)>();
        protected int TotalWeight = 0;
        readonly Func<T, int, double> Multiply;
        readonly Func<T, double, double> Subtract;

        public AbstractWeightTable(IEnumerable<(T Value, int Weight)> valueWeightPairs, Func<T, int, double> multiply, Func<T, double, double> subtract)
        {
            foreach (var vwp in valueWeightPairs)
            {
                MasterTable.Add(vwp);
                Table.Add(vwp);
            }

            TotalWeight = MasterTable.Aggregate(0, (a, b) => a + b.Weight);
            Multiply = multiply;
            Subtract = subtract;
        }

        protected abstract int Index { get; }
        public virtual void Reset()
        {
            Table.Clear();
            foreach (var vwp in MasterTable)
            {
                Table.Add(vwp);
            }

            TotalWeight = MasterTable.Aggregate(0, (a, b) => a + b.Weight);
        }

        public abstract T Sample { get; }
        public abstract T SampleWithoutReplacement { get; }

        IEnumerable<T> Enumerate
        {
            get
            {
                foreach (var (value, weight) in MasterTable)
                {
                    for (int i = 0; i < weight; i++)
                    {
                        yield return value;
                    }
                }
            }
        }

        double Average
        {
            get
            {
                double average = 0;
                double totalWeight = Table.Aggregate(0, (a, b) => a + b.Weight);
                foreach (var (value, weight) in Table)
                {
                    average += Multiply(value, weight) / totalWeight;
                }

                return average;
            }
        }

        double StandardDeviation(double average)
        {
            double variance = 0;
            double totalWeight = Table.Aggregate(0, (a, b) => a + b.Weight);
            foreach (var (value, weight) in Table)
            {
                variance += weight * Math.Pow(Subtract(value, average), 2) / totalWeight;
            }

            return Math.Sqrt(variance);
        }

        public (double Mean, double StandardDeviation, int Count) SummaryStats
        {
            get
            {
                var average = Average;
                var standardDeviation = StandardDeviation(average);
                return (Mean: average, StandardDeviation: standardDeviation, Count: TotalWeight);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
