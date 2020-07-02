using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.CommandLine;
using System.Linq;
using Process = System.Diagnostics.Process;

namespace DotNetEventPipe
{
    public class PSCommandHandler
    {
        public static void PrintProcessStatus(IConsole console)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Process with Event Pipe available for Diagnostics");
            Console.ResetColor();

            // Ignore current process from the list                
            int currentProcess = Process.GetCurrentProcess().Id;

            var processes = DiagnosticsClient.GetPublishedProcesses()
                .Select(GetProcessById)
                .Where(process => process != null && process.Id != currentProcess);

            foreach (var process in processes)
            {
                Console.WriteLine($"\t{process.Id} {process.ProcessName} {process.MainModule.FileName}");
            }
        }

        private static Process GetProcessById(int processId)
        {
            try
            {
                return Process.GetProcessById(processId);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
