using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClusterClient.Extensions;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var requests = new HashSet<Task<string>>();
            var globalTimer = Stopwatch.StartNew();
            var count = 0;
            foreach (var webRequest in ReplicaAddresses.Select(x => CreateRequest(x + "?query=" + query)))
            {
                Log.InfoFormat($"Processing {webRequest.RequestUri}");

                var roundTimeout = 
                    (timeout - globalTimer.Elapsed) / (ReplicaAddresses.Length - count++);
                if (roundTimeout < TimeSpan.Zero)
                    throw new TimeoutException();

                requests.Add(ProcessRequestAsync(webRequest));
                var requestTask = await
                    Task.WhenAny(new List<Task>(requests) { Task.Delay(roundTimeout) });

                if (requestTask is Task<string> rt)
                    if (rt.IsCompletedSuccessfully)
                        return rt.Result;
                    else
                        requests.Remove(rt);
            }

            throw new TimeoutException();
        }
        protected override ILog Log => LogManager.GetLogger(typeof(SmartClusterClient));
    }
}
