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
        static string dumpFilePath = string.Empty;

        public static Task<int> Main(string[] args)
        {
            var parser = new CommandLineBuilder()
                .AddCommand(CollectCommand())
                .AddCommand(AnalyzeCommand())
                .AddCommand(ProcessStatusCommandHandler.ProcessStatusCommand("Process list with Event Pipe available for Diagnostics"))
                .UseParseErrorReporting()
                .UseDebugDirective()
                .UseDefaults()
                .Build();

            return parser.InvokeAsync(args);
        }

        #region command-line commands
        // Collect command and handler
        private static Command CollectCommand() =>
            new Command(name: "collect", description: "Capture trace from a process using Event Pipe")
            {
                // Handler
                CommandHandler.Create<IConsole, int, int, string>(new TraceStack().Collect),
                // Options
                ProcessIdOption(), TraceDurationOption(), TraceFileNameOption()
            };

        // Analyze command and handler
        private static Command AnalyzeCommand() =>
            new Command(name: "analyze", description: "Analyze trace a process")
            {
                        // Handler
                        CommandHandler.Create<IConsole, FileInfo>(new TraceStack().Analyze),
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

