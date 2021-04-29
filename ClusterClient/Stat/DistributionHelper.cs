using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterClient.Stat
{
    public class DistributionHelper
    {
        private readonly StatProvider statProvider;

        public DistributionHelper(StatProvider statProvider)
        {
            this.statProvider = statProvider;
        }

        public List<Node> SmartShuffle()
        {
            Node[] nodes;
            lock (statProvider.Nodes)
            {
                 nodes = statProvider.Nodes.Values.ToArray();
            }

            var result = new List<Node>();
            var byIndex = new Dictionary<Node, int>();
            foreach (var (node, index) in nodes.Select((e, i) => (e, i)))
                byIndex[node] = index;
            var totalWeight = nodes.Sum(n => n.SpeedMetric);
            var tree = new SegmentTree<Node>(nodes);
            var rnd = new Random();

            for (int i = 0; i < nodes.Length; i++)
            {
                var dst = rnd.Next(0, (int)totalWeight);
                var node = tree.Find(Node.ByMetric(dst));
                result.Add(node);
                totalWeight -= dst;
                tree.Update(byIndex[node], node.InvertNode());
            }
            return result;
        }
    }
}
