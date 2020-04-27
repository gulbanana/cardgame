using System;
using System.Collections.Generic;

namespace Cardgame.Engine.Logging
{
    internal class LogManager
    {
        private readonly List<string> managedEntries;

        public LogManager(List<string> managedEntries)
        {
            this.managedEntries = managedEntries;
        }

        public void LogBasicEvent(string eventText)
        {
            managedEntries.Add(eventText);
        }

        public Record LogComplexEvent(string eventText)
        {
            var record = new Record(managedEntries.Count, eventText, UpdateEntry);
            managedEntries.Add(record.Header);
            return record;
        }
        
        public void ClearEntry(Record record)
        {
            managedEntries.RemoveAt(record.Index);
        }

        public void UpdateEntry(Record record)
        {
            var lines = new List<string>();
            lines.Add(record.Header);
            lines.AddRange(GetLogLines(record, 1));

            var partialXML = string.Join(Environment.NewLine, lines);
            var finalXML = $@"<lines>
                {partialXML}
            </lines>";            
            managedEntries[record.Index] = finalXML.ToString();
        }

        private IEnumerable<string> GetLogLines(Subrecord subrecord, int indent)
        {
            foreach (var section in subrecord.Sections)
            {
                if (section.Chunk != null)
                {
                    foreach (var line in section.Chunk)
                    {
                        yield return $@"<spans>
                            <indent level='{indent}' />
                            {line}
                        </spans>";
                    }
                }
                else
                {
                    foreach (var indentedLine in GetLogLines(section.Subrecord, indent + 1))
                    {
                        yield return indentedLine;
                    }
                }
            }
        }
    }
}