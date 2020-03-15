using System;
using System.Collections.Generic;

namespace AliasMethod
{
    sealed class LinearWeightTable<T> : AbstractWeightTable<T> where T : struct
    {
        public LinearWeightTable(ICollection<Tuple<T, int>> valueWeightPairs, Func<T, int, double> multiply, Func<T, double, double> subtract) : base(valueWeightPairs, multiply, subtract) { }

        protected override int Index
        {
            get
            {
                double x = StaticRandom.Next(TotalWeight);
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
        }

        public override T Sample
        {
            get
            {
                return Table[Index].Item1;
            }
        }

        public override T SampleWithoutReplacement
        {
            get
            {
                int index = Index;
                T value = Table[index].Item1;
                TotalWeight -= Table[index].Item2;
                Table.RemoveAt(index);
                return value;
            }
        }
    }
}
