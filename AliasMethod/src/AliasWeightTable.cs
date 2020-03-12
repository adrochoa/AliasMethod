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

        public AliasWeightTable(ICollection<Tuple<T, int>> ValueWeightPairs) : base(ValueWeightPairs)
        {
            double average = 1.0 / ValueWeightPairs.Count;

            var Probabilities = new List<double>();

            double TotalWeight = ValueWeightPairs.Aggregate(0, (a, b) => a + b.Item2);
            foreach (var kvp in ValueWeightPairs)
            {
                Probabilities.Add(kvp.Item2 / TotalWeight);
                Values.Add(kvp.Item1);
            }

            var small = new Stack<int>();
            var large = new Stack<int>();

            for (int i = 0; i < Probabilities.Count; ++i)
            {
                if (Probabilities[i] >= average)
                {
                    large.Push(i);
                }
                else
                {
                    small.Push(i);
                }
            }

            while (!(small.Count > 0) && !(large.Count > 0))
            {
                int less = small.Pop();
                int more = large.Pop();

                Probability[less] = Probabilities[less] * Probabilities.Count;
                Alias[less] = more;

                Probabilities[more] = Probabilities[more] + Probabilities[less] - average;

                if (Probabilities[more] >= 1.0 / Probabilities.Count)
                {
                    large.Push(more);
                }
                else
                {
                    small.Push(more);
                }
            }

            while (!(small.Count > 0))
            {
                Probability[small.Pop()] = 1;
            }
            while (!(large.Count > 0))
            {
                Probability[large.Pop()] = 1;
            }
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override T Sample(Random random)
        {
            int column = random.Next(Probability.Count);

            bool coinToss = random.NextDouble() < Probability[column];

            return coinToss ? Values[column] : Values[Alias[column]];
        }

        public override T SampleWithoutReplacement(Random random)
        {
            throw new NotImplementedException();
        }
    }
}