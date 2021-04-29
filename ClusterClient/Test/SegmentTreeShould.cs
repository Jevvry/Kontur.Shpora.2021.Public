using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.Stat;
using NUnit.Framework;

namespace ClusterClient.Test
{
    [TestFixture]
    public class SegmentTreeShould
    {
        [Test]
        public void FindElementByAmbit()
        {
            var a = new A[] {new(1), new(2), new(3), new(4)};
            var tree = new SegmentTree<A>(a);
            Assert.AreEqual(tree.Find(new(8)).Value,4);
        }
    }

    public class A : IMonoid<A>, IComparable<A>
    {
        public int Value { get; set; }

        public A(int value)
        {
            Value = value;
        }
        public A Sum(A other)
        {
          return new A(Value + other.Value);
        }

        public A Subtraction(A other)
        {
            return new A(Value - other.Value);
        }

        public int CompareTo(A? other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}
