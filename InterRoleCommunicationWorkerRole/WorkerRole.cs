using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace InterRoleCommunicationWorkerRole
{
    using System.IO;
    using System.Text;

    public class WorkerRole : RoleEntryPoint
    {
        private HttpListener _httpListener;

        public override void Run()
        {
            Trace.WriteLine("InterRoleCommunicationWorkerRole entry point called", "Information");
            Trace.TraceInformation("HttpListener started and running");

            try
            {
                for (; ; )
                {
                    HttpListenerContext ctx = _httpListener.GetContext();
                    new Thread(new Worker(ctx).ProcessRequest).Start();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;

            this.SetupDiagnostics();

            this.StartHttpListener();

            return base.OnStart();
        }

        private void SetupDiagnostics()
        {
            // DiagnosticMonitorConfiguration : http://msdn.microsoft.com/en-us/library/ee773135.aspx

            DiagnosticMonitorConfiguration diagConfig = DiagnosticMonitor.GetDefaultInitialConfiguration();

            // Windows event logs
            diagConfig.WindowsEventLog.DataSources.Add("System!*");
            diagConfig.WindowsEventLog.DataSources.Add("Application!*");
            diagConfig.WindowsEventLog.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            diagConfig.WindowsEventLog.ScheduledTransferPeriod = TimeSpan.FromMinutes(1);

            // Azure application logs
            diagConfig.Logs.ScheduledTransferLogLevelFilter = LogLevel.Information;
            diagConfig.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1);

            // Performance counters
            diagConfig.PerformanceCounters.DataSources.Add(
                new PerformanceCounterConfiguration()
                {
                    CounterSpecifier = @"\Processor(_Total)\% Processor Time",
                    SampleRate = TimeSpan.FromMinutes(1)
                });
            diagConfig.PerformanceCounters.DataSources.Add(
                new PerformanceCounterConfiguration()
                {
                    CounterSpecifier = @"\Memory\Available Mbytes",
                    SampleRate = TimeSpan.FromMinutes(1)
                });
            diagConfig.PerformanceCounters.ScheduledTransferPeriod = TimeSpan.FromMinutes(1);
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", diagConfig);

            // use Azure configuration as setting publisher
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
            });
        }

        public override void OnStop()
        {
            _httpListener.Stop();
            Trace.TraceInformation("HttpListener stopped");
            base.OnStop();
        }

        private void StartHttpListener()
        {
            string ipAddress = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WorkerRoleEndpoint"].IPEndpoint.Address.ToString();
            int port = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WorkerRoleEndpoint"].IPEndpoint.Port;
            //string uri = @"http://" + ipAddress + ":" + port + "/";
            string uri = @"http://+:" + port + "/";

            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(uri);
                _httpListener.Start();
                Trace.TraceInformation("HttpListener started");
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }
        }
    }

    internal class Worker
    {
        private HttpListenerContext context;

        private CloudBlobClient blobStorage;

        public Worker(HttpListenerContext context)
        {
            this.context = context;

            // read account configuration settings
            var storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");

            // create blob container for images
            blobStorage = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobStorage.GetContainerReference("workermsgs");
            container.CreateIfNotExist();
        }

        public void ProcessRequest()
        {
            string msg = context.Request.HttpMethod + " " + context.Request.Url;
            Trace.TraceInformation(msg);
            var request = context.Request;
            string text;

            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                text = reader.ReadToEnd();
                Trace.TraceInformation(text);
                StorageMessage(text);
            }

            byte[] b = Encoding.UTF8.GetBytes("Message received");
            context.Response.ContentLength64 = b.Length;
            context.Response.OutputStream.Write(b, 0, b.Length);
            context.Response.StatusCode = 200;
            context.Response.OutputStream.Close();
        }

        private void StorageMessage(string message)
        {
            var container = blobStorage.GetContainerReference("workermsgs");
            string uniqueBlobName = string.Format("msg_{0}.xml", Guid.NewGuid().ToString());
            var blob = container.GetBlockBlobReference(uniqueBlobName);
            blob.Properties.ContentType = "application/octet-stream";
            blob.UploadText(message);
        }
    }
}
