using System;
using System.Collections.Generic;
using System.Linq;

namespace AliasMethod
{
    sealed class BasicWeightTable<T> : WeightTable<T> where T : struct
    {
        public BasicWeightTable() : base() { }
        public BasicWeightTable(ICollection<Tuple<T, int>> valueWeightPairs) : base(valueWeightPairs) { }

        protected override int GetIndex(Random random)
        {
            throw new NotImplementedException();
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
