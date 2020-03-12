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
            double x = random.Next(TotalWeight);
            double cumulativeSum = 0;
            for (int i = 0; i < Table.Count; i++)
            {
                cumulativeSum += Table[i].Item2;
                if (x < cumulativeSum)
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException();
        }

        public override T Sample(Random random)
        {
            int index = GetIndex(random);
            return Table[index].Item1;
        }

        public override T SampleWithoutReplacement(Random random)
        {
            int index = GetIndex(random);
            T Value = Table[index].Item1;
            TotalWeight -= Table[index].Item2;
            Table.RemoveAt(index);
            return Value;
        }
    }
}
