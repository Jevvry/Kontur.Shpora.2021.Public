using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class ParallelClusterClient : ClusterClientBase
    {
        public ParallelClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var tasks = ReplicaAddresses.Select(uri =>
           {
               var webRequest = CreateRequest(uri + "?query=" + query);
               Log.InfoFormat($"Processing {webRequest.RequestUri}");
               return ProcessRequestAsync(webRequest);
           }).ToList();
            var requestTasks = WaitForAnyNonFaultedTaskAsync(tasks);
            await Task.WhenAny(requestTasks, Task.Delay(timeout));
            if (!requestTasks.IsCompleted)
                throw new TimeoutException();
            return requestTasks.Unwrap().Result;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(ParallelClusterClient));
    }
}
