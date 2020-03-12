using System;
using System.Collections.Generic;

namespace AliasMethod
{
    abstract class WeightTable<T> : IWeightTable<T> where T : struct
    {
        protected readonly List<Tuple<T, int>> MasterTable = new List<Tuple<T, int>>();

        public WeightTable() { }
        public WeightTable(ICollection<Tuple<T, int>> ValueWeightPairs)
        {
            foreach (var kvp in ValueWeightPairs)
            {
                MasterTable.Add(new Tuple<T, int>(kvp.Item1, kvp.Item2));
            }
        }

        public abstract void Reset();
        public abstract T Sample(Random random);
        public abstract T SampleWithoutReplacement(Random random);
    }
}
