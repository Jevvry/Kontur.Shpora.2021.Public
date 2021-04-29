using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Stat
{
    public struct Node : IMonoid<Node>, IComparable<Node>,IEquatable<Node>
    {
        private const int Perfect = 3600 * 2 * 1000;

        public string ReplicaAddress { get; }
        public double SpeedMetric { get; }

        public Node(string replicaAddress, double metricLatency)
        {
            ReplicaAddress = replicaAddress;
            SpeedMetric = Perfect / metricLatency;
        }

        public Node Update(double value)
        {
            var latency = Perfect / (value + 0.9 * SpeedMetric);
            return new Node(ReplicaAddress, latency);
        }

        public Node Sum(Node other) =>
            new(ReplicaAddress, SpeedMetric + other.SpeedMetric);

        public Node Subtraction(Node other) =>
            new(ReplicaAddress, SpeedMetric - other.SpeedMetric);

        public int CompareTo(Node other) =>
            SpeedMetric.CompareTo(other.SpeedMetric);

        public Node InvertNode() => new(ReplicaAddress, -SpeedMetric);

        public static Node ByMetric(double metric) =>
            new Node("", metric);

        public bool Equals(Node other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ReplicaAddress == other.ReplicaAddress;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Node) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ReplicaAddress);
        }
    }
}
