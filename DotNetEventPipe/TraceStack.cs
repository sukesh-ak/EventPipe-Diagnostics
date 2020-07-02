using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Symbols;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Stacks;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics.Tracing;
using System.IO;
using System.Threading.Tasks;
using TraceLog = Microsoft.Diagnostics.Tracing.Etlx.TraceLog;

namespace DotNetEventPipe
{
    public class TraceStack
    {
        /// <summary>
        /// Trigger a trace for given number of seconds
        /// </summary>
        /// <param name="console"></param>
        /// <param name="processId">Process Id of the process</param>
        /// <param name="duration">Duration of trace in seconds (default:10sec)</param>
        /// <param name="traceFilename">Output filename for the trace</param>
        /// <returns>Returns 0 for success and 1 for failure</returns>
        public static int Collect(IConsole console, int processId,int duration, string traceFilename)
        {
            if (processId == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                console.Error.WriteLine("-p [process-id] Process ID is required.");
                Console.ResetColor();
                return 1;
            }
            if (duration == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                console.Out.WriteLine("Trace duration default set to 10sec.");
                Console.ResetColor();
                console.Out.WriteLine("-d [duration] to set duration.");
                duration = 10;
            }

            if (string.IsNullOrEmpty(traceFilename))
                // Default this to yyyyMMddHHmmss-PID.nettrace 
                traceFilename = Path.Combine(Environment.CurrentDirectory, "traces", DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + processId + ".nettrace");
            else
                traceFilename = Path.Combine(Environment.CurrentDirectory, "traces", traceFilename + ".nettrace");

            console.Out.WriteLine($"Process ID : {processId}");
            console.Out.WriteLine($"Duration   : {duration}");
            console.Out.WriteLine($"Trace file : {traceFilename}");

            try
            {
                var providers = new List<EventPipeProvider>()
                {
                    new EventPipeProvider("Microsoft-DotNETCore-SampleProfiler",
                        EventLevel.Informational, (long)ClrTraceEventParser.Keywords.All),    // 0
                    new EventPipeProvider("Microsoft-Windows-DotNETRuntime",
                        EventLevel.Informational, (long)ClrTraceEventParser.Keywords.Default) // 89142578365
                };
                
                // Create client
                var diagClient = new DiagnosticsClient(processId);

                // Create session
                using (var eventPipeSession = diagClient.StartEventPipeSession(providers))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    console.Out.WriteLine("[Trace started...]");
                    Console.ResetColor();
                    // Write event stream to trace file
                    Task readerTask = Task.Run(async () =>
                    {
                        using (FileStream fs = new FileStream(traceFilename, FileMode.Create, FileAccess.Write))
                        {
                            await eventPipeSession.EventStream.CopyToAsync(fs);
                        }
                    });

                    readerTask.Wait(duration * 1000);
                    eventPipeSession.Stop();
                    Console.ForegroundColor = ConsoleColor.Green;
                    console.Out.WriteLine("[Trace completed.]\n");
                    Console.ResetColor();
                }
                return 0; 
            }
            catch (System.Exception e)
            {
                console.Error.WriteLine($"{ e.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Reads trace and prints managed stacks
        /// </summary>
        /// <param name="console"></param>
        /// <param name="traceFile"></param>
        /// <returns>Returns 0 for success and 1 for failure</returns>
        /// <remarks>This code is adopted from josalem code https://github.com/josalem/DotStack </remarks>
        public static int Analyze(IConsole console, FileInfo traceFile)
        {
            if (string.IsNullOrEmpty(traceFile.FullName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                console.Error.WriteLine("-f [process-id] Process ID is required.");
                Console.ResetColor();
                return 1;
            }
            
            Console.WriteLine("[Trace analysis started...]\n");

            // Both the namespaces have TraceLog, here we use Microsoft.Diagnostics.Tracing.Etlx.TraceLog;
            // The following line creates a etlx file and then does analysis using that.
            string tempEtlxFilename = TraceLog.CreateFromEventPipeDataFile(traceFile.FullName);

            using (var symbolReader = new SymbolReader(System.IO.TextWriter.Null)
            { SymbolPath = SymbolPath.MicrosoftSymbolServerPath })

            using (var eventLog = new TraceLog(tempEtlxFilename))
            {
                var stackSource = new MutableTraceEventStackSource(eventLog)
                {
                    OnlyManagedCodeStacks = true
                };

                var computer = new SampleProfilerThreadTimeComputer(eventLog, symbolReader);
                computer.GenerateThreadTimeStacks(stackSource);

                var samplesForThread = new Dictionary<string, List<StackSourceSample>>();

                stackSource.ForEach((sample) =>
                {
                    var stackIndex = sample.StackIndex;
                    while (!stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false).StartsWith("Thread ("))
                        stackIndex = stackSource.GetCallerIndex(stackIndex);

                    var threadName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
                    if (samplesForThread.TryGetValue(threadName, out var samples))
                    {
                        samples.Add(sample);
                    }
                    else
                    {
                        samplesForThread[threadName] = new List<StackSourceSample>() { sample };
                    }
                });

                foreach (var (threadName, samples) in samplesForThread)
                {
                    PrintStack(threadName, samples[0], stackSource);
                }
            }

            Console.WriteLine("\n[Trace analysis completed...]");
            return 0;
        }

        private static void PrintStack(string threadName, StackSourceSample stackSourceSample, StackSource stackSource)
        {
            Console.WriteLine($"Stack for {threadName}:");
            var stackIndex = stackSourceSample.StackIndex;
            while (!stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false).StartsWith("Thread ("))
            {
                Console.WriteLine($"  {stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false)}");
                stackIndex = stackSource.GetCallerIndex(stackIndex);
            }
            Console.WriteLine();
        }
    }
}
