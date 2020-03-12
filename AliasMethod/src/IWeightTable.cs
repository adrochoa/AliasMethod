using System;

namespace AliasMethod
{
    interface IWeightTable<T> where T : struct
    {
        void Reset();
        T Sample(Random random);
        T SampleWithoutReplacement(Random random);
    }
}
