using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClassLibrary1;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private static Crawler crawler = new Crawler();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");


            while (true)
            {
                CloudQueueMessage commandMessage = StorageManager.CommandQueue().GetMessage();
                if (commandMessage != null)
                {
                    switch (commandMessage.AsString)
                    {
                        case "startcrawling":
                            crawler.Start();
                            StorageManager.CommandQueue().DeleteMessage(commandMessage);
                            break;
                        case "stopcrawling":
                            crawler.Stop();
                            StorageManager.CommandQueue().DeleteMessage(commandMessage);
                            break;
                        case "clear":
                            StorageManager.LinkQueue().Clear();
                            StorageManager.CommandQueue().Clear();
                            StorageManager.HTMLQueue().Clear();
                            StorageManager.GetTable().DeleteIfExists();
                            StorageManager.PerformanceCounterTable().DeleteIfExists();
                            StorageManager.ErrorTable().DeleteIfExists();
                            break;
                        default:
                            break;
                    }


                }
                new Task(crawler.GetPerfCounters).Start();
                if (Crawler.state.Equals("Loading"))
                {
                    new Task(crawler.GetPerfCounters).Start();
                    crawler.CrawlUrl();

                }
                else if (crawler.GetState().Equals("Crawling"))
                {
                    new Task(crawler.GetPerfCounters).Start();
                    crawler.GetHTMLData();
                }

            }
        }


        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");


                await Task.Delay(1000);
            }
        }
    }
}









