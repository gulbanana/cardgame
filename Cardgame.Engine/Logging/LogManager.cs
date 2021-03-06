using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Cardgame.API;
using Cardgame.Model;

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

        public Record LogComplexEvent(string actor, Event @event)
        {
            var record = new Record(managedEntries.Count, actor, @event, UpdateEntry);
            managedEntries.Add(GetRecordHeader(record));
            return record;
        }
        
        public void ClearEntry(Record record)
        {
            managedEntries.RemoveAt(record.Index);
        }

        public void UpdateEntry(Record record)
        {
            var lines = new List<string>();
            lines.Add(GetRecordHeader(record));
            lines.AddRange(GetRecordLines(record, 1));

            var partialXML = string.Join(Environment.NewLine, lines);
            var finalXML = $@"<lines>
                {partialXML}
            </lines>";            
            managedEntries[record.Index] = finalXML.ToString();
        }
        
        public void Save(Record record)
        {
            //var options = new JsonSerializerOptions 
            //{ 
            //    Converters = { new JsonStringEnumConverter(), new EventConverter(), new ChunkConverter() },
            //    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //    IgnoreNullValues = true,
            //    WriteIndented = true,
            //};
            //var json = JsonSerializer.Serialize(record, options);
            //Console.WriteLine(json);
        }

        private string GetRecordHeader(Record record)
        {
            return record.Event switch {
                BeginTurn { TurnNumber: var turnNumber, Controller: var controller } when controller != record.Actor => $@"<bold>
                    <run>---</run>
                    <if you='Your' them='{record.Actor}&apos;s'>{record.Actor}</if>
                    <run>turn {turnNumber} (controlled by</run>
                    <if you='you' them='{controller}' suffix=')'>{controller}</if>
                    <run>---</run>
                </bold>",

                BeginTurn { TurnNumber: var turnNumber } => $@"<bold>
                    <run>---</run>
                    <if you='Your' them='{record.Actor}&apos;s'>{record.Actor}</if>
                    <run>turn {turnNumber} ---</run>
                </bold>",

                BuyCard { Card: var card, Controller: var controller } when controller != record.Actor => $@"<spans>
                    <player>{record.Actor}</player>
                    <if you='buy' them='buys'>{record.Actor}</if>
                    <card>{card}</card>
                    <run>for</run>
                    <player nonterminal='true' suffix='.'>{controller}</player>
                </spans>",

                BuyCard { Card: var card } => $@"<spans>
                    <player>{record.Actor}</player>
                    <if you='buy' them='buys'>{record.Actor}</if>
                    <card suffix='.'>{card}</card>
                </spans>",

                PlayCards { Cards: var cards } => $@"<spans>
                    <player>{record.Actor}</player>
                    <if you='play' them='plays'>{record.Actor}</if>
                    {Terminate(FormatCardList(cards))}
                </spans>",
                
                Cleanup _ => $@"<spans>
                    <player>{record.Actor}</player>
                    <if you='end your' them='ends their'>{record.Actor}</if>
                    <run>turn.</run>
                </spans>",

                _ => throw new NotSupportedException(record.Event.ToString())
            };
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
            var builder = new StringBuilder();
            var lastActor = chunk.Actor;
            for (var i = 0; i < chunk.Movements.Count; i++)
            {
                // actors can switch back and forth due to possession. if that happens, flush the line and stop "and"ing
                var hasPrevious = i > 0;
                var actor = chunk.Movements[i].Type == Motion.Gain ? chunk.GainActor : chunk.Actor;
                if (i > 0 && actor != lastActor)
                {
                    yield return Terminate(builder.ToString());
                    builder.Clear();
                    hasPrevious = false;
                }                

                builder.Append(FormatMovement(actor, chunk.Movements[i], hasPrevious ? chunk.Movements[i-1] : null));
            }

            // vanilla bonuses, potentially consequences of the movements
            if (chunk.Reshuffled)
            {
                builder.Append(FormatVerb(chunk.Actor, "reshuffle", "reshuffles", "reshuffling", !chunk.Movements.Any()));
            }

            if (chunk.AddedCards.Count > 0)
            {
                builder.Append(FormatVerb(chunk.Actor, "draw", "draws", "drawing", !chunk.Movements.Any() && !chunk.Reshuffled));
                var altText = chunk.AddedCards.Count > 1 ? $"{chunk.AddedCards.Count} cards" : "a card";
                builder.Append($"<private owner='{chunk.Actor}' alt='{altText}'>");
                builder.Append(FormatCardList(chunk.AddedCards));
                builder.Append("</private>");
            }

            if (chunk.AddedActions > 0 || chunk.AddedBuys > 0 || chunk.AddedCoins > 0 || chunk.AddedPotions > 0)
            {
                builder.Append(FormatVerb(chunk.Actor, "get", "gets", "getting", !chunk.Movements.Any() && !chunk.Reshuffled && !chunk.AddedCards.Any()));

                var got = new List<string>();
                if (chunk.AddedActions > 0) got.Add($"+{chunk.AddedActions} {(chunk.AddedActions > 1 ? "actions" : "action")}");
                if (chunk.AddedBuys > 0) got.Add($"+{chunk.AddedBuys} {(chunk.AddedActions > 1 ? "buys" : "buy")}");
                if (chunk.AddedCoins > 0 || chunk.AddedPotions > 0) got.Add($"+{Cost.Format(chunk.AddedCoins, chunk.AddedPotions)}");
                builder.Append(FormatList(got));
            }

            if (builder.Length > 0)
            {
                yield return Terminate(builder.ToString());
            }
        }
        
        private string Terminate(string spans)
        {
            var temp = $"<root>{spans}</root>";
            var xml = XDocument.Parse(temp).Root;
            var lastRun = xml.Elements().Last();

            while (lastRun.Name == "private")
            {
                lastRun = lastRun.Elements().Last();
            }

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

        private string FormatCardList(IReadOnlyList<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return "<run>nothing</run>";
            }

            return string.Join(Environment.NewLine, ids.Select((id, ix) => 
            {
                var suffix = ix == ids.Count -1 ? string.Empty
                    : ix < ids.Count - 2 ? ","
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

        private string FormatSubsequentVerb(string player, string secondPerson, string thirdPerson, string continuous)
        {
            if (player == getActivePlayer())
            {
                return $"<run>and</run><if you='{secondPerson}' them='{continuous}'>{player}</if>";
            }
            else
            {
                return $"<run>and</run><if you='{secondPerson}' them='{thirdPerson}'>{player}</if>";
            }
        }

        private string FormatVerb(string player, string secondPerson, string thirdPerson, string continuous, bool initial)
        {
            return initial ? FormatInitialVerb(player, secondPerson, thirdPerson, continuous) : FormatSubsequentVerb(player, secondPerson, thirdPerson, continuous);
        }

        private string FormatZone(string actor, Zone zone)
        {
            switch (zone.Name)
            {
                case ZoneName.Attached when zone.Param is Instance instance:
                    return $"<run>under</run><card>{instance.Id}</card>";

                case ZoneName.DeckBottom:
                    return $@"<run>the bottom of</run>
                              <if you='your' them='their'>{actor}</if>
                              <run>deck</run>";

                case ZoneName.Deck:
                case ZoneName.DeckTop:
                    return $"<if you='your' them='their'>{actor}</if><run>deck</run>";

                case ZoneName.Discard:
                    return $"<if you='your' them='their'>{actor}</if><run>discard pile</run>";

                case ZoneName.Hand:
                    return $"<if you='your' them='their'>{actor}</if><run>hand</run>";

                case ZoneName.InPlay:
                    return "<run>play</run>";

                case ZoneName.PlayerMat when zone.Param is string id:
                    var mat = All.Mats.ByName(id);
                    return $"<if you='your' them='their'>{actor}</if><run>{mat.Label} mat</run>";

                case ZoneName.Revealed:
                    return "<run>the revealed cards</run>";

                case ZoneName.Stash:
                    return "<run>somewhere</run>";

                case ZoneName.Supply:
                    return "<run>the supply</run>";

                case ZoneName.Trash:
                    return "<run>the trash</run>";

                default:
                    throw new NotSupportedException($"Unknown log zone {zone}");
            }
        }

        private string FormatMovement(string actor, Movement movement, Movement previous)
        {
            var builder = new StringBuilder();            
            var first = previous == null;
            
            // the motion kind, unless we're repeating one
            if (previous != null && previous.Type == movement.Type)
            {
                builder.Append("<run>and</run>");
            }
            else
            {
                switch (movement.Type)
                {
                    case Motion.Discard:
                        builder.Append(FormatVerb(actor, "discard", "discards", "discarding", first));
                        break;

                    case Motion.Gain:
                        builder.Append(FormatVerb(actor, "gain", "gains", "gaining", first));
                        break;

                    case Motion.Play:
                        builder.Append(FormatVerb(actor, "play", "plays", "playing", first));
                        break;

                    case Motion.Put:
                        builder.Append(FormatVerb(actor, "put", "puts", "putting", first));
                        break;

                    case Motion.Reveal:
                        builder.Append(FormatVerb(actor, "reveal", "reveals", "revealing", first));
                        break;

                    case Motion.Return:
                        builder.Append(FormatVerb(actor, "return", "returns", "returning", first));
                        break;

                    case Motion.Trash:
                        builder.Append(FormatVerb(actor, "trash", "trashes", "trashing", first));
                        break;

                    default:
                        throw new NotSupportedException($"Unknown log motion {movement.Type}");
                }
            }

            // the card list and source, unless we're referring to a previous set
            if (previous != null && previous.Cards.SequenceEqual(movement.Cards) && (previous.Type == Motion.Reveal || movement.Type == Motion.Play))
            {
                if (movement.Cards.Length > 1)
                {
                    builder.Append("<run>them</run>");
                }
                else
                {
                    builder.Append("<run>it</run>");
                }
            }
            else
            {
                var cardList = FormatCardList(movement.Cards);
                if (movement.From.IsPrivate() && movement.To.IsPrivate())
                {
                    var altText = movement.Cards.Length > 1 ? $"{movement.Cards.Length} cards" : "a card";
                    builder.Append($"<private owner='{actor}' alt='{altText}'>");
                    builder.Append(cardList);
                    builder.Append("</private>");
                }
                else
                {
                    builder.Append(cardList);
                }

                if (movement.From != Zone.Create) switch (movement.Type)
                {
                    // discards from hand are normal
                    case Motion.Discard:
                        if (movement.From != Zone.Hand)
                        {
                            builder.Append("<run>from</run>");
                            builder.Append(FormatZone(actor, movement.From));
                        }
                        break;

                    // gains are always either from supply or to discard, frequently both
                    case Motion.Gain:
                        if (movement.From.Name != ZoneName.Supply)
                        {
                            builder.Append("<run>from</run>");
                            builder.Append(FormatZone(actor, movement.From));
                        }
                        break;

                    // sources with no special interpretation 
                    case Motion.Play:
                    case Motion.Put:
                    case Motion.Reveal:
                    case Motion.Return:
                        builder.Append("<run>from</run>");
                        builder.Append(FormatZone(actor, movement.From));
                        break;

                    // only note trashes from odd places
                    case Motion.Trash:                    
                        switch (movement.From.Name)
                        {
                            case ZoneName.Supply:
                                builder.Append("<run>from</run>");
                                builder.Append(FormatZone(actor, movement.From));
                                break;
                        }
                        break;

                    default:
                        throw new NotSupportedException($"Unknown log motion {movement.Type}");
                }
            }

            // the card destination, if worth noting
            switch (movement.Type)
            {
                case Motion.Discard:
                case Motion.Play:
                case Motion.Reveal:
                case Motion.Trash:
                    break;             

                // Gain and Return motions have an implied destination, but you never know
                case Motion.Gain:
                    if (movement.To != Zone.Discard)
                    {
                        builder.Append("<run>to</run>");
                        builder.Append(FormatZone(actor, movement.To));
                    }
                    break;

                case Motion.Return:
                    builder.Append("<run>to</run>");
                    builder.Append(FormatZone(actor, movement.To));
                    break;

                // "put" has adaptive grammar
                case Motion.Put:
                    var preposition = movement.To.Name switch {                        
                        ZoneName.Attached => string.Empty,
                        ZoneName.Deck => "onto",
                        ZoneName.DeckTop => "onto",
                        ZoneName.DeckBottom => "on the bottom of",
                        ZoneName.Discard => "onto",
                        ZoneName.Hand => "onto",
                        ZoneName.InPlay => "into",
                        ZoneName.PlayerMat => "onto",
                        ZoneName.Supply => "into",
                        ZoneName.Trash => "into",
                        _ => throw new NotSupportedException("Unknown log put destination {movement.To.Name}")
                    };

                    builder.Append($"<run>{preposition}</run>");
                    builder.Append(FormatZone(actor, movement.To));
                    break;

                default:
                    throw new NotSupportedException($"Unknown log motion {movement.Type}");
            }

            return builder.ToString();
        }
    }
}