using System;

namespace Cardgame.Backend
{
    // why?
    public class CommandException : Exception
    {
        public CommandException(string message) : base(message) { }
    }
}