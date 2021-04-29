using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClientWithStat : ClusterClientBase
    {
        public RoundRobinClusterClientWithStat(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var distribution = distributionHelper.SmartShuffle();
            var roundTimeout = timeout / distribution.Count;

            foreach (var node in distribution)
            {
                var webRequest = CreateRequest(node.ReplicaAddress + "?query=" + query);
                Log.InfoFormat($"Processing {webRequest.RequestUri}");
                var requestTask = ProcessRequestAsync(webRequest);
                var statTask = AttachStat(requestTask, webRequest, roundTimeout);
                await Task.WhenAny(requestTask, Task.Delay(roundTimeout));
                if (requestTask.IsCompleted)
                    return requestTask.Result;
                await statTask;
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClientWithStat));
    }
}
