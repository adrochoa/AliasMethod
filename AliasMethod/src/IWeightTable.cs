using System;
using System.Collections.Generic;

namespace AliasMethod
{
    interface IWeightTable<T> : IEnumerable<T> where T : struct
    {
        void Reset();
        T Sample(Random random);
        T SampleWithoutReplacement(Random random);
        (double Mean, double StandardDeviation) SummaryStats { get; }
    }
}
