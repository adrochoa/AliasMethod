using System;
using System.Collections.Generic;
using System.Linq;

namespace AliasMethod
{
    sealed class AliasWeightTable<T> : AbstractWeightTable<T> where T : struct
    {
        readonly List<int> Alias = new List<int>();
        readonly List<double> Probability = new List<double>();
        readonly List<T> Values = new List<T>();

        public AliasWeightTable(ICollection<(T Value, int Weight)> valueWeightPairs, Func<T, int, double> multiply, Func<T, double, double> subtract) : base(valueWeightPairs, multiply, subtract)
        {
            SetTables(valueWeightPairs);
        }

        protected override int Index
        {
            get
            {
                int column = StaticRandom.Next(Probability.Count);
                bool coinToss = StaticRandom.NextDouble() < Probability[column];
                return coinToss ? column : Alias[column];
            }
        }

        public override void Reset()
        {
            base.Reset();
            SetTables(MasterTable);
        }

        public override T Sample
        {
            get
            {
                return Values[Index];
            }
        }

        public override T SampleWithoutReplacement
        {
            get
            {
                int index = Index;
                T value = Values[index];
                Table.RemoveAt(index);
                SetTables(Table);
                return value;
            }
        }

        private void SetTables(ICollection<(T Value, int Weight)> valueWeightPairs)
        {
            Alias.Clear();
            Probability.Clear();
            Values.Clear();

            double average = 1d / valueWeightPairs.Count;

            var probabilities = new List<double>();

            TotalWeight = valueWeightPairs.Aggregate(0, (a, b) => a + b.Weight);

            foreach (var (value, weight) in valueWeightPairs)
            {
                probabilities.Add((double)weight / TotalWeight);
                Probability.Add((double)weight / TotalWeight);
                Alias.Add(0);
                Values.Add(value);
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