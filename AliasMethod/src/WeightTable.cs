using System;
using System.Collections.Generic;
using System.Linq;

namespace AliasMethod
{
    abstract class WeightTable<T> : IWeightTable<T> where T : struct
    {
        protected readonly List<Tuple<T, int>> MasterTable = new List<Tuple<T, int>>();
        protected readonly List<Tuple<T, int>> Table = new List<Tuple<T, int>>();
        protected int TotalWeight = 0;

        public WeightTable() { }
        public WeightTable(ICollection<Tuple<T, int>> valueWeightPairs)
        {
            foreach (var vwp in valueWeightPairs)
            {
                MasterTable.Add(new Tuple<T, int>(vwp.Item1, vwp.Item2));
                Table.Add(new Tuple<T, int>(vwp.Item1, vwp.Item2));
            }

            TotalWeight = MasterTable.Aggregate(0, (a, b) => a + b.Item2);
        }

        protected abstract int GetIndex(Random random);

        public virtual void Reset()
        {
            Table.Clear();
            foreach (var vwp in MasterTable)
            {
                Table.Add(new Tuple<T, int>(vwp.Item1, vwp.Item2));
            }

            TotalWeight = MasterTable.Aggregate(0, (a, b) => a + b.Item2);
        }

        public abstract T Sample(Random random);
        public abstract T SampleWithoutReplacement(Random random);
    }
}
