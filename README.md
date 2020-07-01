# EventPipe-Diagnostics
A simple trace and analyze application to showcase, how EventPipe in .NET Core works and how to extract thread stack from the resulting trace.

### Using .NET Event Pipe for diagnostics

EventPipe technology of .NET Core runtime is a cross-platform alternative to ETW on Windows and LTTng on Linux since 
they work only on a single platform. EventPipe deliver the same experience on Windows, Linux and macOS. 

EventPipe can also be used on any .NET Core applications running on .NET Core 3.0 Preview 5 or later.

Diagnostic IPC Protocol in .NET Core runtime listens and communicates over a platform-specific transport. 
On Unix/Linux based platforms, a Unix Domain Socket will be used, and on Windows, a Named Pipe will be used.

### Why EventPipe for Production Diagnostics
EventPipe is the best cross-platform an alternative to ETW/LTTng and also the functionality is built-into .NET Core framework so you don't need any external components.


Its very useful for analyzing CPU usage, IO, lock contention, allocation rate, etc where you might want to capture a performance trace. 
This trace can then be moved to a developer machine where it can be analyzed with profiling tools such as PerfView/VisualStudio or visualized as a flame graph with speedscope.

### Documentation
EventPipe-Diagnostics project here contains three functionalities as commands.
  1. PS (lists all process running with EventPipe enabled)
  2. Collect trace (collects trace for a defined period of time)
  3. Analyze trace (prints out the manage stack trace)

#### Did someone say Help!!! 
You can use -? option to see all the command line options
```
> DotNetEventPipe -?
Usage:
  DotNetEventPipe [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  collect    Capture trace from a process using Event Pipe
  analyze    Analyze trace a process and prints managed call stack
  ps         Process list with Event Pipe available for Diagnostics
```

#### What does ps command option do?
The command ps shows all the process with EventPipe listener available to connect
```
> DotNetEventPipe ps
Process with Event Pipe available for Diagnostics
        29624 iisexpress C:\Program Files\IIS Express\iisexpress.exe
```


#### What does Collect command option do?
Collect option runs a trace on a selected .NET process which supports EventPipe for a specific duration.

```
DotNetEventPipe collect -?
collect:
  Capture trace from a process using Event Pipe

Usage:
  DotNetEventPipe collect [options]

Options:
  -p, --processId <pid> (REQUIRED)    The process id to collect the trace for.
  -d, --duration <duration>           Duration in seconds to run the trace.
  -f, --traceFilename <filename>      Trace file name (without path).
  -?, -h, --help                      Show help and usage information
```

###### Typical usage of Collect command below
Here we are tracing a process with pid 29624 for a duration of 20 seconds and writing to trace filename 'newtrace.nettrace'
> DotNetEventPipe collect -p 29624 -d 20 -f "newtrace" 

Output looks like below
```
> DotNetEventPipe collect -p 29624 -d 20 -f "newtrace"
Process ID : 29624
Duration   : 20
Trace file : C:\DotNetEventPipe\traces\newtrace.nettrace
[Trace started...]
[Trace completed.]
```

#### What does Analyze command option do?
Analyze option reads the trace and prints out managed stack on console

```
> DotNetEventPipe analyze -?
analyze:
  Analyze trace a process and prints managed call stack

Usage:
  DotNetEventPipe analyze [options]

Options:
  -f, --traceFile <filename> (REQUIRED)    Trace file name (without path).
  -?, -h, --help                           Show help and usage information
```

###### Typical usage of Analyze command below
Here we pass the trace file for the analysis
> DotNetEventPipe analyze -f C:\DotNetEventPipe\traces\newtrace.nettrace

Let us see the output of the above command
```
> DotNetEventPipe analyze -f C:\DotNetEventPipe\traces\newtrace.nettrace
[Trace analysis started...]

Stack for Thread (4692):
  UNMANAGED_CODE_TIME
  System.Private.CoreLib.il!System.Threading.ManualResetEventSlim.Wait(int32,value class System.Threading.CancellationToken)
  System.Private.CoreLib.il!System.Threading.Tasks.Task.SpinThenBlockingWait(int32,value class System.Threading.CancellationToken)
  System.Private.CoreLib.il!System.Threading.Tasks.Task.InternalWaitCore(int32,value class System.Threading.CancellationToken)
  System.Private.CoreLib.il!System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(class System.Threading.Tasks.Task)
  System.Private.CoreLib.il!System.Runtime.CompilerServices.TaskAwaiter.GetResult()
  Microsoft.Extensions.Hosting.Abstractions.il!Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.Run(class Microsoft.Extensions.Hosting.IHost)
  ProblemScenarios!ProblemScenarios.Program.Main(class System.String[])

Stack for Thread (39440):
  UNMANAGED_CODE_TIME
  System.Private.CoreLib.il!System.Threading.SemaphoreSlim.WaitUntilCountOrTimeout(int32,unsigned int32,value class System.Threading.CancellationToken)
  System.Private.CoreLib.il!System.Threading.SemaphoreSlim.Wait(int32,value class System.Threading.CancellationToken)
  System.Collections.Concurrent.il!System.Collections.Concurrent.BlockingCollection`1[Microsoft.Extensions.Logging.Console.LogMessageEntry].TryTakeWithNoTimeValidation(!0&,int32,value class System.Threading.CancellationToken,class System.Threading.CancellationTokenSource)
  System.Collections.Concurrent.il!System.Collections.Concurrent.BlockingCollection`1+<GetConsumingEnumerable>d__68[Microsoft.Extensions.Logging.Console.LogMessageEntry].MoveNext()
  Microsoft.Extensions.Logging.Console.il!Microsoft.Extensions.Logging.Console.ConsoleLoggerProcessor.ProcessLogQueue()
  System.Private.CoreLib.il!System.Threading.ThreadHelper.ThreadStart_Context(class System.Object)
  System.Private.CoreLib.il!System.Threading.ExecutionContext.RunInternal(class System.Threading.ExecutionContext,class System.Threading.ContextCallback,class System.Object)
  System.Private.CoreLib.il!System.Threading.ThreadHelper.ThreadStart()

Stack for Thread (18720):
  UNMANAGED_CODE_TIME
  System.Private.CoreLib.il!System.Threading.Thread.Join()
  ProblemScenarios!testwebapi.Controllers.DiagScenarioController.deadlock()
  Anonymously Hosted DynamicMethods Assembly!dynamicClass.lambda_method(pMT: 00007FFEE69552C8,class System.Object,class System.Object[])
  Microsoft.AspNetCore.Mvc.Core.il!Microsoft.Extensions.Internal.ObjectMethodExecutor.Execute(class System.Object,class System.Object[])
  ...
  System.Private.CoreLib.il!System.Runtime.CompilerServices.AsyncMethodBuilderCore.Start(!!0&)
  System.Private.CoreLib.il!System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[System.Boolean].Start(!!0&)
  Microsoft.AspNetCore.Server.IIS.il!Microsoft.AspNetCore.Server.IIS.Core.IISHttpContextOfT`1[System.__Canon].ProcessRequestAsync()
  Microsoft.AspNetCore.Server.IIS.il!Microsoft.AspNetCore.Server.IIS.Core.IISHttpContext+<HandleRequest>d__165.MoveNext()
  System.Private.CoreLib.il!System.Runtime.CompilerServices.AsyncMethodBuilderCore.Start(!!0&)
  System.Private.CoreLib.il!System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start(!!0&)
  Microsoft.AspNetCore.Server.IIS.il!Microsoft.AspNetCore.Server.IIS.Core.IISHttpContext.HandleRequest()
  Microsoft.AspNetCore.Server.IIS.il!Microsoft.AspNetCore.Server.IIS.Core.IISHttpContext.Execute()
  System.Private.CoreLib.il!System.Threading.ThreadPoolWorkQueue.Dispatch()

Stack for Thread (10064):
  UNMANAGED_CODE_TIME
  ProblemScenarios!testwebapi.Controllers.DiagScenarioController.<deadlock>b__3_1()
  System.Private.CoreLib.il!System.Threading.ExecutionContext.RunInternal(class System.Threading.ExecutionContext,class System.Threading.ContextCallback,class System.Object)

Stack for Thread (33240):
  UNMANAGED_CODE_TIME
  ProblemScenarios!testwebapi.Controllers.DiagScenarioController.<deadlock>b__3_1()
  System.Private.CoreLib.il!System.Threading.ExecutionContext.RunInternal(class System.Threading.ExecutionContext,class System.Threading.ContextCallback,class System.Object)

Stack for Thread (10900):
  UNMANAGED_CODE_TIME
  System.Private.CoreLib.il!System.Threading.WaitHandle.WaitOneNoCheck(int32)
  System.Private.CoreLib.il!System.Diagnostics.Tracing.CounterGroup.PollForValues()

[Trace analysis completed...]
```

#### Graphical UI for viewing the trace
On Windows, .nettrace files can be viewed on [PerfView](https://github.com/microsoft/perfview) for analysis. 
For traces collected on other platforms, the trace file can be moved to a Windows machine to be viewed on PerfView.

This is a more focused version provided in source for experiment with the API. 
For full options to take traces, please check official CLI [dotnet-trace](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)

### Feedback
Feel free to provide feedback on how it would help and what needs to be changed/added
- Open issues
- Send PR
- Other ideas and suggestions are welcome


