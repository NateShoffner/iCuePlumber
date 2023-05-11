﻿using CommandLine;
using System;
using Topshelf;

namespace iCuePlumber.Console
{
    partial class Program
    {
        public class Options
        {
            [Option('n', "name", Required = false, Default = "CorsiarService", HelpText = "The name of the service to monitor.")]
            public string ServiceName { get; set; }

            [Option('m', "memlimit", Required = false, Default = 20000, HelpText = "The memory limit (in kilobytes) at which to interrupt the service.")]
            public long MemoryLimit { get; set; }

            [Option('r', "rate", Required = false, Default = 5000, HelpText = "The rate (in milliseconds) at which to poll the service.")]
            public double PollingRate { get; set; }
        }

        static void Main(string[] args)
        {
            var options = Parser.Default.ParseArguments<Options>(args);

            System.Console.WriteLine($"Memory Limit: {options.Value.MemoryLimit}KB");
            System.Console.WriteLine($"Polling Rate: {options.Value.PollingRate}ms");

            var exitCode = HostFactory.Run(x =>
            {
                x.Service<ServiceWatcher>(s =>
                {
                    s.ConstructUsing(name => new ServiceWatcher(options.Value.ServiceName, options.Value.PollingRate, options.Value.MemoryLimit));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();

                x.SetDescription("Monitors iCUE for memory leaks and automatically restarts the service.");
                x.SetDisplayName("iCUE Plumber");
                x.SetServiceName("iCUEPlumber");
            });

            var exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }

    }
}
