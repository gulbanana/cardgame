using System;

namespace Cardgame.Engine
{
    // why?
    public class CommandException : Exception
    {
        public CommandException(string message) : base(message) { }
    }
}