using System;
using System.Collections.Generic;
using System.Linq;

namespace AliasMethod
{
    sealed class AliasWeightTable<T> : WeightTable<T> where T : struct
    {
        readonly List<int> Alias = new List<int>();
        readonly List<double> Probability = new List<double>();
        readonly List<T> Values = new List<T>();

        public AliasWeightTable(ICollection<Tuple<T, int>> valueWeightPairs) : base(valueWeightPairs)
        {
            SetTables(valueWeightPairs);
        }

        protected override int GetIndex(Random random)
        {
            int column = random.Next(Probability.Count);
            bool coinToss = random.NextDouble() < Probability[column];
            return coinToss ? column : Alias[column];
        }

        public override void Reset()
        {
            base.Reset();
            SetTables(MasterTable);
        }

        public override T Sample(Random random)
        {
            return Values[GetIndex(random)];
        }

        public override T SampleWithoutReplacement(Random random)
        {
            int index = GetIndex(random);
            T Value = Values[index];
            Table.RemoveAt(index);
            SetTables(Table);
            return Value;
        }

        private void SetTables(ICollection<Tuple<T, int>> valueWeightPairs)
        {
            Alias.Clear();
            Probability.Clear();
            Values.Clear();

            double average = 1d / valueWeightPairs.Count;

            var probabilities = new List<double>();

            TotalWeight = valueWeightPairs.Aggregate(0, (a, b) => a + b.Item2);
            foreach (var vwp in valueWeightPairs)
            {
                probabilities.Add(vwp.Item2 / TotalWeight);
                Probability.Add(vwp.Item2 / TotalWeight);
                Alias.Add(0);
                Values.Add(vwp.Item1);
            }

            var small = new Stack<int>();
            var large = new Stack<int>();

            for (int i = 0; i < probabilities.Count; ++i)
            {
                if (probabilities[i] >= average)
                {
                    large.Push(i);
                }
                else
                {
                    small.Push(i);
                }
            }

            while (small.Count > 0 && large.Count > 0)
            {
                int less = small.Pop();
                int more = large.Pop();

                Probability[less] = probabilities[less] * probabilities.Count;
                Alias[less] = more;

                probabilities[more] = probabilities[more] + probabilities[less] - average;

                if (probabilities[more] >= 1d / probabilities.Count)
                {
                    large.Push(more);
                }
                else
                {
                    small.Push(more);
                }
            }

            while (small.Count > 0)
            {
                Probability[small.Pop()] = 1;
            }
            while (large.Count > 0)
            {
                Probability[large.Pop()] = 1;
            }
        }
    }
}