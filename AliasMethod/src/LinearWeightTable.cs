using System;
using System.Collections.Generic;

namespace AliasMethod
{
    sealed class LinearWeightTable<T> : AbstractWeightTable<T> where T : struct
    {
        public LinearWeightTable(IEnumerable<(T Value, int Weight)> valueWeightPairs, Func<T, int, double> multiply, Func<T, double, double> subtract) : base(valueWeightPairs, multiply, subtract) { }

        protected override int Index
        {
            get
            {
                double x = StaticRandom.Next(TotalWeight);
                double cumulativeSum = 0;
                for (var i = 0; i < Table.Count; i++)
                {
                    cumulativeSum += Table[i].Weight;
                    if (x < cumulativeSum)
                    {
                        return i;
                    }
                }

                throw new IndexOutOfRangeException();
            }
        }

        public override T SampleWithoutReplacement
        {
            get
            {
                var index = Index;
                var value = Table[index].Value;
                TotalWeight -= Table[index].Weight;
                Table.RemoveAt(index);
                return value;
            }
        }
    }
}
