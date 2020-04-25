using System;

namespace Cardgame.Server
{
    // why?
    public class CommandException : Exception
    {
        public CommandException(string message) : base(message) { }
    }
}