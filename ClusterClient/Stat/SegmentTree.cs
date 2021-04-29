using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace ClusterClient.Stat
{
    public class SegmentTree<T>
        where T : IMonoid<T>, IComparable<T>
    {
        public readonly T[] tree;
        private readonly int nodeCount;

        public SegmentTree(T[] array)
        {
            nodeCount = array.Length;
            tree = new T[4 * nodeCount];
            Build(array, 1, 0, nodeCount - 1);
        }

        private void Build(T[] array, int currentPosition, int tLeft, int tRight)
        {
            if (tLeft == tRight)
                tree[currentPosition] = array[tLeft];
            else
            {
                int tm = (tLeft + tRight) / 2;
                Build(array, currentPosition * 2, tLeft, tm);
                Build(array, currentPosition * 2 + 1, tm + 1, tRight);
                tree[currentPosition] = tree[currentPosition * 2].Sum(tree[currentPosition * 2 + 1]);
            }
        }

        public void Update(int position, T newValue)
        {
            Update(1, 0, nodeCount - 1, position, newValue);
        }

        private void Update(int currentPosition, int tLeft, int tRight, int position, T newValue)
        {
            if (tLeft == tRight)
                tree[currentPosition] = newValue;
            else
            {
                int tm = (tLeft + tRight) / 2;
                if (position <= tm)
                    Update(currentPosition * 2, tLeft, tm, position, newValue);
                else
                    Update(currentPosition * 2 + 1, tm + 1, tRight, position, newValue);
                tree[currentPosition] = tree[currentPosition * 2].Sum(tree[currentPosition * 2 + 1]);
            }
        }
        public T Find(T value) => Find(1, 0, nodeCount - 1, value);

        private T Find(int currentPosition, int tLeft, int tRight, T value)
        {
            if (tLeft == tRight)
                return tree[currentPosition];
            int tm = (tLeft + tRight) / 2;
            var left = Left(currentPosition);
            if (value.CompareTo(left) < 0)
                return Find(currentPosition * 2, tLeft, tm, value);
            value = value.Subtraction(left);
            return Find(currentPosition * 2 + 1, tm + 1, tRight, value);
        }

        private T Left(int i) => tree[2 * i];
        private T Right(int i) => tree[2 * i + 1];
    }
}
