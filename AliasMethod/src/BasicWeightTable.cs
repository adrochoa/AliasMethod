using System;
using System.Collections.Generic;
using System.Linq;

namespace AliasMethod
{
    sealed class BasicWeightTable<T> : WeightTable<T> where T : struct
    {
        readonly List<Tuple<T, int>> Table = new List<Tuple<T, int>>();
        int TotalWeight = 0;

        public BasicWeightTable() : base() { }
        public BasicWeightTable(ICollection<Tuple<T, int>> ValueWeightPair) : base(ValueWeightPair)
        {
            foreach (var kvp in ValueWeightPair)
            {
                Table.Add(new Tuple<T, int>(kvp.Item1, kvp.Item2));
            }
            TotalWeight = MasterTable.Aggregate(0, (a, b) => a + b.Item2);
        }

        public override void Reset()
        {
            Table.Clear();
            foreach (var vwp in MasterTable)
            {
                Table.Add(new Tuple<T, int>(vwp.Item1, vwp.Item2));
            }

            TotalWeight = MasterTable.Aggregate(0, (a, b) => a + b.Item2);
        }

        public override T Sample(Random random)
        {
            double x = random.Next(TotalWeight);
            double cumulativeSum = 0;
            foreach (var vwp in Table)
            {
                cumulativeSum += vwp.Item2;
                if (x < cumulativeSum)
                {
                    return vwp.Item1;
                }
            }

            throw new IndexOutOfRangeException();
        }

        public override T SampleWithoutReplacement(Random random)
        {
            double x = random.Next(TotalWeight);
            double cumulativeSum = 0;

            T Value = default;
            for (int i = 0; i < Table.Count; i++)
            {
                cumulativeSum += Table[i].Item2;
                if (x < cumulativeSum)
                {
                    Value = Table[i].Item1;
                    TotalWeight -= Table[i].Item2;
                    Table.RemoveAt(i);
                }
            }

            return Value;
        }
    }
}
