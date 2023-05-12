using CommandLine;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Timers;

namespace iCuePlumber
{
    partial class Program
    {
        public class ServiceWatcher
        {
            private readonly Timer _timer;
            private readonly string _serviceName;
            private readonly long _memoryThreshold;

            public ServiceWatcher(string serviceName, double pollingRate, long memoryThreshold)
            {
                _serviceName = serviceName;
                _memoryThreshold = memoryThreshold;
                _timer = new Timer(pollingRate) { AutoReset = true };
                _timer.Elapsed += OnTimerElapsed;

            }

            private void OnTimerElapsed(object sender, EventArgs e)
            {
                CheckService();
            }

            public void Start()
            {
                _timer.Start();
                CheckService();
            }

            public void Stop()
            {
                _timer.Stop();
            }

            private void CheckService()
            {
                ServiceController service = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == _serviceName);

                if (service == null)
                {
                    Console.WriteLine($"Could not find service: {_serviceName}");
                    return;
                }

                var process = GetServiceProcess(service);

                if (process == null)
                {
                    Console.WriteLine($"Could not find process for service: {_serviceName}");
                    return;
                }

                var memUsage = process.WorkingSet64;

                var kb = memUsage / 1024f;

                if (kb < _memoryThreshold)
                {
                    Console.WriteLine($"Memory usage for service {service.DisplayName} is below limit. {kb}KB < {_memoryThreshold}KB");
                    return;
                }

                Console.WriteLine($"Memory usage for service {service.DisplayName} is above limit. {kb}KB > {_memoryThreshold}KB");

                Console.WriteLine("Stopping service...");

                try
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping service: {ex.Message}");
                    return;
                }

                Console.WriteLine("Starting service...");

                try
                {
                    service.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting service: {ex.Message}");
                    return;
                }

                Console.WriteLine("Service restarted.");


            }

            private Process GetServiceProcess(ServiceController service)
            {
                using (var managementBaseObject = new ManagementObjectSearcher(new SelectQuery(string.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", service.ServiceName))).Get())
                {
                    ManagementObject mo = managementBaseObject.Cast<ManagementObject>().FirstOrDefault();
                    if (mo == null)
                        return null;

                    if (mo["ProcessId"] == null)
                        return null;

                    var processID = int.Parse(mo["ProcessId"].ToString());

                    return Process.GetProcessById(processID);
                }
            }
        }

    }
}
