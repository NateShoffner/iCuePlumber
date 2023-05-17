using System;
using System.Diagnostics;
using Topshelf;

namespace iCuePlumber
{
    partial class Program
    {
        static void Main(string[] args)
        {
            EventLog eventLog = new EventLog
            {
                Source = "iCUEPlumber",
                Log = "Application"
            };

            string serviceName = "CorsairService";
            double pollingRate = 10 * 60 * 1000;
            long memoryLimit = 500000;

            var exitCode = HostFactory.Run(x =>
            {
                x.RunAsLocalSystem();

                x.SetDescription("Monitors iCUE for memory leaks and automatically restarts the service.");
                x.SetDisplayName("iCUE Plumber");
                x.SetServiceName("iCUEPlumber");

                x.AddCommandLineDefinition("s", v => serviceName = v);
                x.AddCommandLineDefinition("m", v => memoryLimit = long.Parse(v));
                x.AddCommandLineDefinition("r", v => pollingRate = double.Parse(v));
                x.ApplyCommandLine();

                x.Service<ServiceWatcher>(s =>
                {
                    s.ConstructUsing(name => new ServiceWatcher(serviceName, pollingRate, memoryLimit, eventLog));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());

                    Console.WriteLine($"Memory Limit: {memoryLimit}KB");
                    Console.WriteLine($"Polling Rate: {pollingRate}ms");

                    eventLog.WriteEntry($"iCUE Plumber started with memory limit {memoryLimit}KB and polling rate {pollingRate}ms.", EventLogEntryType.Information);
                });
            });

            var exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;

            eventLog.WriteEntry($"iCUE Plumber exited with code {exitCodeValue}.", EventLogEntryType.Information);
        }

    }
}
