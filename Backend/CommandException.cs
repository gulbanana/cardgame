using System;

namespace Cardgame
{
    public class CommandException : Exception
    {
        public CommandException(string message) : base(message) { }
    }
}