using System.Collections.Generic;

namespace Cardgame.Engine
{
    public class LogRecord
    {
        public readonly int Index;
        public readonly string Header;
        public readonly List<string> Lines;

        public LogRecord(int index, string header)
        {
            Index = index;
            Header = header;
            Lines = new List<string>();
        }
    }
}