using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var globalTimer = Stopwatch.StartNew();
            var count = 0;
            foreach (var uri in ReplicaAddresses)
            {
                var interRequestsDelay =
                    new TimeSpan((timeout - globalTimer.Elapsed).Ticks /
                                 (ReplicaAddresses.Length - count++));

                if (interRequestsDelay < TimeSpan.Zero)
                    throw new TimeoutException();

                var webRequest = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat($"Processing {webRequest.RequestUri}");
                var requestTask = ProcessRequestAsync(webRequest);
                await Task.WhenAny(requestTask, Task.Delay(interRequestsDelay));

                if (requestTask.IsCompletedSuccessfully)
                    return requestTask.Result;
            }
            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RoundRobinClusterClient));
    }
}
