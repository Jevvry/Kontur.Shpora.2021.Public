using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClusterClient.Stat
{
    public class StatProvider
    {
        private readonly Regex pattern =
            new(@"(http://\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d+?.*?)\?query=(.+?)\s+?.+?(\d+)\s*?ms",
                RegexOptions.Compiled);
        private readonly Regex addressPattern =
            new(@"(http://\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d+?.*?)\?query=",
                RegexOptions.Compiled);

        public Dictionary<string, Node> Nodes { get; private set; } = new();

        public StatProvider(string[] replicaAddresses)
        {
            foreach (var address in replicaAddresses)
            {
                Nodes[address] = new Node(address, 360000);
            }
        }

        public void Update(Task requestTask, WebRequest webRequest, long latency)
        {
            var address = addressPattern.Match(webRequest.RequestUri.AbsoluteUri).Groups[1].Value;
            lock (Nodes)
            {
                if (Nodes.TryGetValue(address, out var node))
                    Nodes[address] = node.Update(latency);
            }
        }


        public async Task UpdateStatByLog(string[] replicaAddresses, string logPath)
        {
            var data = await File.ReadAllLinesAsync(logPath);
            var nodes = replicaAddresses.ToDictionary(a => a, a => new Node(a, int.MaxValue));
            foreach (var line in data)
                foreach (Match match in pattern.Matches(line).Where(m => m.Success))
                {
                    var address = match.Groups[1].Value;
                    var query = match.Groups[2].Value;
                    var responseTime = int.Parse(match.Groups[3].Value);

                    if (nodes.TryGetValue(address, out var node))
                        nodes[address] = node.Update(responseTime);
                    else
                        nodes[query] = new Node(address, responseTime);
                }
            Nodes = nodes;
        }
    }
}
