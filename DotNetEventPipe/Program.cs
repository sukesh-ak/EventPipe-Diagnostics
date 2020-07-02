using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading.Tasks;

namespace DotNetEventPipe
{
    class Program
    {
        public static Task<int> Main(string[] args)
        {
            var parser = new CommandLineBuilder()
                .AddCommand(PSCommand())
                .AddCommand(CollectCommand())
                .AddCommand(AnalyzeCommand())
                .UseParseErrorReporting()
                .UseDebugDirective()
                .UseDefaults()
                .Build();

            return parser.InvokeAsync(args);
        }

        #region command-line commands
        // ps command and handler
        public static Command PSCommand() =>
        new Command(name: "ps", "Process list with Event Pipe available for Diagnostics")
        {
            Handler = CommandHandler.Create<IConsole>(PSCommandHandler.PrintProcessStatus)
        };

        // Collect command and handler
        private static Command CollectCommand() =>
            new Command(name: "collect", description: "Capture trace from a process using Event Pipe")
            {
                // Handler
                CommandHandler.Create<IConsole, int, int, string>(TraceStack.Collect),
                // Options
                ProcessIdOption(), TraceDurationOption(), TraceFileNameOption()
            };

        // Analyze command and handler
        private static Command AnalyzeCommand() =>
            new Command(name: "analyze", description: "Analyze trace a process and prints managed call stack")
            {
                        // Handler
                        CommandHandler.Create<IConsole, FileInfo>(TraceStack.Analyze),
                        // Options
                        FileNameOption()
            };
        #endregion

        #region command-line options
        // Argument for Collect Command
        private static Option ProcessIdOption() =>
            new Option(
            aliases: new[] { "-p", "--processId" },
            description: "The process id to collect the trace for.")
            {
                Argument = new Argument<int>(name: "pid"), 
                Required = true
            };

        // Argument for Collect Command
        private static Option TraceDurationOption() =>
            new Option(
            aliases: new[] { "-d", "--duration" },
            description: "Duration in seconds to run the trace.")
            {
                Argument = new Argument<int>(name: "duration")
            };

        // Argument for Collect Command
        private static Option TraceFileNameOption() =>
            new Option(
            aliases: new[] { "-f", "--traceFilename" },
            description: "Trace file name (without path).")
            {
                Argument = new Argument<string>(name: "filename")
            };

        // Argument for Analyze Command
        private static Option FileNameOption() =>
            new Option(
            aliases: new[] { "-f", "--traceFile" },
            description: "Trace file name (without path).")
            {
                // Check file exists also
                Argument = new Argument<FileInfo>(name: "filename").ExistingOnly(), 
                Required = true
            };

        #endregion
    }
}

