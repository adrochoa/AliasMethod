using System.Collections.Generic;

namespace AliasMethod
{
    interface IWeightTable<T> : IEnumerable<T> where T : struct
    {
        void Reset();
        T Sample { get; }
        T SampleWithoutReplacement { get; }
        (double Mean, double StandardDeviation) SummaryStats { get; }
    }
}
