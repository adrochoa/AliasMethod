﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AliasMethod
{
    class DualArray<T> : IEnumerable<T> where T : struct
    {
        readonly List<List<T>> Dual = new List<List<T>>();

        public T this[int i, int j]
        {
            get => Dual[i][j];
            set => Dual[i][j] = value;
        }

        IEnumerable<T> Flattened
        {
            get
            {
                foreach (var list in Dual)
                {
                    foreach (var elem in list)
                    {
                        yield return elem;
                    }
                }
            }
        }

        public DualArray(IEnumerable<IEnumerable<T>> dual)
        {
            foreach (var list in dual)
            {
                Dual.Add(new List<T>());
                foreach (var elem in list)
                {
                    Dual[Dual.Count - 1].Add(elem);
                }
            }
        }

        public void Replace(Predicate<T> predicate, T replacement)
        {
            for (int i = 0; i < Dual.Count; i++)
            {
                for (int j = 0; j < Dual[i].Count; j++)
                {
                    if (predicate(Dual[i][j]))
                    {
                        Dual[i][j] = replacement;
                    }
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Flattened.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}