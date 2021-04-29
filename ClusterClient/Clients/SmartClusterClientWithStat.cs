using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClientWithStat : ClusterClientBase
    {
        public SmartClusterClientWithStat(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var distribution = distributionHelper.SmartShuffle();
            var roundTimeout = timeout / distribution.Count;
            var requests = new List<Task<string>>();

            foreach (var node in distribution)
            {
                var webRequest = CreateRequest(node.ReplicaAddress + "?query=" + query);
                Log.InfoFormat($"Processing {webRequest.RequestUri}");
                var requestTask = ProcessRequestAsync(webRequest);
                var statTask = AttachStat(requestTask, webRequest, roundTimeout);
                requests.Add(requestTask);
                var response = Task.WhenAny(requests);
                await Task.WhenAny(response, Task.Delay(roundTimeout));

                if (response.IsCompleted)
                    return response.Unwrap().Result;
                await statTask;
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClientWithStat));
    }
}
