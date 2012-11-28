using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace InterRoleCommunicationWebRole
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            this.SetupDiagnostics();

            return base.OnStart();
        }

        private void SetupDiagnostics()
        {
            // DiagnosticMonitorConfiguration : http://msdn.microsoft.com/en-us/library/ee773135.aspx

            DiagnosticMonitorConfiguration diagConfig = DiagnosticMonitor.GetDefaultInitialConfiguration();

            // Windows event logs
            diagConfig.WindowsEventLog.DataSources.Add("System!*");
            diagConfig.WindowsEventLog.DataSources.Add("Application!*");
            diagConfig.WindowsEventLog.ScheduledTransferLogLevelFilter = LogLevel.Information;
            diagConfig.WindowsEventLog.ScheduledTransferPeriod = TimeSpan.FromMinutes(1);

            // Azure application logs
            diagConfig.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
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
    }
}
