using System;

namespace PromBot.Commands
{
    public enum CommandErrorType { Exception, UnknownCommand, BadArgCount, InvalidInput }
    public class CommandErrorEventArgs : CommandEventArgs
    {
        public CommandErrorType ErrorType { get; }
        public Exception Exception { get; }

        public CommandErrorEventArgs(CommandErrorType errorType, CommandEventArgs baseArgs, Client client, Exception ex)
            : base(baseArgs.Message, baseArgs.Command, client, baseArgs.Args)
        {
            Exception = ex;
            ErrorType = errorType;
        }
    }
}
