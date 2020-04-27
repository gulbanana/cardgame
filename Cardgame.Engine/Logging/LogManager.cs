using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Cardgame.API;

namespace Cardgame.Engine.Logging
{
    internal class LogManager
    {
        private readonly List<string> managedEntries;
        private readonly Func<string> getActivePlayer;

        public LogManager(List<string> managedEntries, Func<string> getActivePlayer)
        {
            this.managedEntries = managedEntries;
            this.getActivePlayer = getActivePlayer;
        }

        public void LogBasicEvent(string eventText)
        {
            managedEntries.Add(eventText);
        }

        public Record LogComplexEvent(string actor, string eventText)
        {
            var record = new Record(managedEntries.Count, actor, eventText, UpdateEntry);
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
            lines.AddRange(GetRecordLines(record, 1));

            var partialXML = string.Join(Environment.NewLine, lines);
            var finalXML = $@"<lines>
                {partialXML}
            </lines>";            
            managedEntries[record.Index] = finalXML.ToString();
        }

        private IEnumerable<string> GetRecordLines(Subrecord subrecord, int indent)
        {
            foreach (var section in subrecord.Sections)
            {
                if (section.Chunk != null)
                {
                    foreach (var line in GetChunkLines(section.Chunk))
                    {
                        yield return $@"<spans>
                            <indent level='{indent}' />
                            {line}
                        </spans>";
                    }
                }
                else
                {
                    foreach (var indentedLine in GetRecordLines(section.Subrecord, section.Subrecord.Inline ? indent : indent + 1))
                    {
                        yield return indentedLine;
                    }
                }
            }
        }

        private IEnumerable<string> GetChunkLines(Chunk chunk)
        {            
            // custom text
            foreach (var line in chunk.Lines)
            {
                yield return line;
            }

            // ordered list of card movements, which will precede all vanilla in a chunk
            foreach (var movement in chunk.Movements)
            {
                var builder = new StringBuilder();
                switch (movement.Type)
                {
                    case Motion.Discard:
                        builder.Append(FormatInitialVerb(chunk.Actor, "discard", "discards", "discarding"));
                        builder.Append(FormatCardList(movement.Cards));
                        if (movement.From != Zone.Hand)
                        {
                            builder.Append("<run>from</run>");
                            builder.Append(FormatZone(chunk.Actor, movement.From));
                        }
                        break;

                    default:
                        throw new NotSupportedException($"Unknown log motion {movement.Type}");
                }
                yield return Terminate(builder.ToString());
            }

            // vanilla bonuses, potentially consequences of the movements
            var vanilla = new StringBuilder();
            if (chunk.AddedCards > 0)
            {
                vanilla.Append(FormatInitialVerb(chunk.Actor, "draw", "draws", "drawing"));
                if (chunk.AddedCards > 1)
                {
                    vanilla.AppendFormat("<run>{0} cards</run>", chunk.AddedCards);
                }
                else
                {
                    vanilla.Append("<run>a card</run>");
                }
            }

            if (chunk.AddedActions > 0 || chunk.AddedBuys > 0 || chunk.AddedCoins > 0 || chunk.AddedPotions > 0)
            {
                if (chunk.AddedCards > 0)
                {
                    vanilla.Append("<run>and</run>");
                    vanilla.Append(FormatVerb(chunk.Actor, "get", "gets", "getting"));
                }
                else
                {
                    vanilla.Append(FormatInitialVerb(chunk.Actor, "get", "gets", "getting"));
                }

                var got = new List<string>();
                if (chunk.AddedActions > 0) got.Add($"+{chunk.AddedActions} {(chunk.AddedActions > 1 ? "actions" : "action")}");
                if (chunk.AddedBuys > 0) got.Add($"+{chunk.AddedBuys} {(chunk.AddedActions > 1 ? "buys" : "buy")}");
                if (chunk.AddedCoins > 0 || chunk.AddedPotions > 0) got.Add($"+{Cost.Format(chunk.AddedCoins, chunk.AddedPotions)}");
                vanilla.Append(FormatList(got));
            }

            if (vanilla.Length > 0)
            {
                yield return Terminate(vanilla.ToString());
            }
        }
        
        private string Terminate(string spans)
        {
            var temp = $"<root>{spans}</root>";
            var xml = XDocument.Parse(temp).Root;
            var lastRun = xml.Elements().Last();

            if (lastRun.Name == "run")
            {
                lastRun.Value += ".";
            }
            else
            {
                var existingSuffix = xml.Elements().Last().Attribute("suffix")?.Value ?? string.Empty;
                lastRun.SetAttributeValue("suffix", existingSuffix + ".");
            }

            return string.Join(string.Empty, xml.Elements().Select(e => e.ToString()));
        }
        
        private string FormatList(IReadOnlyList<string> elements)
        {
            var builder = new StringBuilder();
            builder.Append("<run>");
            for (var i = 0; i < elements.Count; i++)
            {
                builder.Append(elements[i]);
                if (i < elements.Count - 2)
                {
                    builder.Append(", ");
                }
                else if (i < elements.Count - 1)
                {
                    builder.Append(" and ");
                }
            }
            builder.Append("</run>");
            return builder.ToString();
        }

        private string FormatCardList(string[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return "<run>nothing</run>";
            }

            return string.Join(Environment.NewLine, ids.Select((id, ix) => 
            {
                var suffix = ix == ids.Length -1 ? string.Empty
                    : ix < ids.Length - 2 ? ","
                    : " and";
                return $"<card suffix='{suffix}'>{id}</card>";
            }));
        }

        private string FormatInitialVerb(string player, string secondPerson, string thirdPerson, string continuous)
        {
            if (player == getActivePlayer())
            {
                return $"<if you='you {secondPerson}' them='{continuous}'>{player}</if>";
            }
            else
            {
                return $"<player>{player}</player><if you='{secondPerson}' them='{thirdPerson}'>{player}</if>";
            }
        }

        private string FormatVerb(string player, string secondPerson, string thirdPerson, string continuous)
        {
            if (player == getActivePlayer())
            {
                return $"<if you='{secondPerson}' them='{continuous}'>{player}</if>";
            }
            else
            {
                return $"<if you='{secondPerson}' them='{thirdPerson}'>{player}</if>";
            }
        }

        private string FormatZone(string player, Zone zone)
        {
            switch (zone.Name)
            {
                case ZoneName.DeckBottom:
                    return $@"<run>the bottom of</run>
                    <if you='your' them='their'>{player}</if>
                    <run>deck</run>";

                case ZoneName.Deck:
                case ZoneName.DeckTop:
                    return $@"<run>the top of</run>
                    <if you='your' them='their'>{player}</if>
                    <run>deck</run>";

                case ZoneName.Discard:
                    return $@"<if you='your' them='their'>{player}</if>
                    <run>discard pile</run>";

                case ZoneName.Hand:
                    return $@"<if you='your' them='their'>{player}</if>
                    <run>hand</run>";

                case ZoneName.InPlay:
                    return "<run>in play</run>";

                case ZoneName.Revealed:
                    return "<run>the revealed cards</run>";

                case ZoneName.Trash:
                    return $@"<run>the trash</run>";

                default:
                    throw new NotSupportedException($"Unknown log zone {zone}");
            }
        }
    }
}