using System;
using System.Collections.Generic;
using System.Linq;

namespace AliasMethod
{
    sealed class AliasWeightTable<T> : AbstractWeightTable<T> where T : struct
    {
        readonly List<int> Alias = new List<int>();
        readonly List<double> Probability = new List<double>();

        public AliasWeightTable(IEnumerable<(T Value, int Weight)> valueWeightPairs, Func<T, int, double> multiply, Func<T, double, double> subtract) : base(valueWeightPairs, multiply, subtract)
        {
            SetTables(valueWeightPairs);
        }

        protected override int Index
        {
            get
            {
                var column = StaticRandom.Next(Probability.Count);
                var coinToss = StaticRandom.NextDouble() < Probability[column];
                return coinToss ? column : Alias[column];
            }
        }

        public override void Reset()
        {
            base.Reset();
            SetTables(MasterTable);
        }

        public override T SampleWithoutReplacement
        {
            get
            {
                var index = Index;
                var value = Table[index].Value;
                Table.RemoveAt(index);
                SetTables(Table);
                return value;
            }
        }

        private void SetTables(IEnumerable<(T Value, int Weight)> valueWeightPairs)
        {
            Alias.Clear();
            Probability.Clear();

            var probabilities = new List<double>();

            TotalWeight = valueWeightPairs.Aggregate(0, (a, b) => a + b.Weight);

            var count = 0;
            foreach (var (value, weight) in valueWeightPairs)
            {
                probabilities.Add((double)weight / TotalWeight);
                Probability.Add((double)weight / TotalWeight);
                Alias.Add(0);
                ++count;
            }

            var average = 1d / count;

            var small = new Stack<int>();
            var large = new Stack<int>();

            for (var i = 0; i < probabilities.Count; ++i)
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
                var less = small.Pop();
                var more = large.Pop();

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