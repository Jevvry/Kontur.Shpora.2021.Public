using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Stat;
using log4net;

namespace ClusterClient.Clients
{
    public abstract class ClusterClientBase
    {
        protected DistributionHelper distributionHelper;
        protected StatProvider statProvider;

        protected string[] ReplicaAddresses { get; set; }

        protected ClusterClientBase(string[] replicaAddresses)
        {
            ReplicaAddresses = replicaAddresses;
            statProvider = new StatProvider(replicaAddresses);
            distributionHelper = new DistributionHelper(statProvider);
        }

        public abstract Task<string> ProcessRequestAsync(string query, TimeSpan timeout);
        protected abstract ILog Log { get; }

        protected static HttpWebRequest CreateRequest(string uriStr)
        {
            var request = WebRequest.CreateHttp(Uri.EscapeUriString(uriStr));
            request.Proxy = null;
            request.KeepAlive = true;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 100500;
            return request;
        }

        protected async Task<string> ProcessRequestAsync(WebRequest request)
        {
            var timer = Stopwatch.StartNew();
            using var response = await request.GetResponseAsync();
            var result = await new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEndAsync();
            Log.InfoFormat("Response from {0} received in {1} ms", request.RequestUri, timer.ElapsedMilliseconds);
            return result;

        }

        protected async Task AttachStat(Task<string> requestTask, WebRequest request, TimeSpan timeOut)
        {
            var timer = Stopwatch.StartNew();
            await Task.WhenAny(requestTask, Task.Delay(timeOut));
            statProvider.Update(requestTask, request,
                !requestTask.IsCompletedSuccessfully ? timeOut.Milliseconds : timer.ElapsedMilliseconds);
        }

        protected async Task<Task<T>> WaitForAnyNonFaultedTaskAsync<T>(List<Task<T>> tasks)
        {
            var customTasks = tasks.ToList();
            Task<T> completedTask;
            do
            {
                completedTask = await Task.WhenAny(customTasks);
                customTasks.Remove(completedTask);
            } while (completedTask.IsFaulted && customTasks.Count > 0);

            return completedTask.IsFaulted ? throw new Exception() : completedTask;
        }

        protected async Task<Task<T>> WaitForAnyNonFaultedTaskAsync1<T>(List<Task<T>> tasks)
        {
            Task<T> completedTask;
            do
            {
                completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
            } while (completedTask.IsFaulted && tasks.Count > 0);

            return completedTask.IsFaulted ? null : completedTask;
        }
    }
}