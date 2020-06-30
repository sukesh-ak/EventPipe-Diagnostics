using System.CommandLine;
using System.CommandLine.Invocation;

namespace DotNetEventPipe
{
    public static class CommandExtensions
    {
        /// <summary>
        /// Allows the command handler to be included in the collection initializer.
        /// </summary>
        public static void Add(this Command command, ICommandHandler handler)
        {
            command.Handler = handler;
        }
    }
}
