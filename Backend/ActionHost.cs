using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame
{
    internal class ActionHost : IActionHost
    {
        private readonly GameEngine engine;
        private readonly int level;
        public string Player { get; }
        public int ShuffleCount { get; private set; }

        public ActionHost(int level, GameEngine engine, string player)
        {
            this.level = level;
            this.engine = engine;
            this.Player = player;
        }

        private string LogVerbInitial(string secondPerson, string thirdPerson, string continuous)
        {
            if (Player == engine.Model.ActivePlayer)
            {
                return $"<if you='you {secondPerson}' them='{continuous}'>{Player}</if>";
            }
            else
            {
                return $"<player>{Player}</player><if you='{secondPerson}' them='{thirdPerson}'>{Player}</if>";
            }
        }

        private string LogVerb(string secondPerson, string thirdPerson, string continuous)
        {
            if (Player == engine.Model.ActivePlayer)
            {
                return $"<if you='{secondPerson}' them='{continuous}'>{Player}</if>";
            }
            else
            {
                return $"<if you='{secondPerson}' them='{thirdPerson}'>{Player}</if>";
            }
        }

        private string LogCardList(string[] ids)
        {
            return string.Join(Environment.NewLine, ids.Select((id, ix) => 
            {
                var suffix = ix == ids.Length -1 ? "."
                    : ix < ids.Length - 2 ? ","
                    : " and";
                return $"<card suffix='{suffix}'>{id}</card>";
            }));
        }

        private string LogDestination(Zone to)
        {
            switch (to)
            {
                case Zone.TopDeck:
                    return $@"{LogVerb("put", "puts", "putting")}
                              <run>it on top of</run>
                              <if you='your' them='their'>{Player}</if>
                              <run>deck.</run>";

                case Zone.Hand:
                    return $@"{LogVerb("put", "puts", "putting")}
                              <run>it into</run>
                              <if you='your' them='their'>{Player}</if>
                              <run>hand.</run>";

                case Zone.Discard:
                    return $@"{LogVerb("discard", "discards", "discarding")}
                              <run>it.</run>";

                case Zone.Trash:
                    return $@"{LogVerb("trash", "trashes", "trashing")}
                              <run>it.</run>";

                default:
                    throw new CommandException($"Unknown zone {to}");
            }
        }

        private void NoteReshuffle()
        {
            ShuffleCount++;
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                <if you='(you reshuffle.)' them='(reshuffling.)'>{Player}</if>
            </spans>");
        }

        Cards.CardModel[] IActionHost.GetHand()
        {
            return engine.Model.Hands[Player].Select(Cards.All.ByName).ToArray();
        }

        void IActionHost.AddActions(int n)
        {
            engine.Model.ActionsRemaining += n;

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                <if you='you get' them='getting'>{engine.Model.ActivePlayer}</if>
                <run>+{n} actions.</run>
            </spans>");
        }

        void IActionHost.AddBuys(int n)
        {
            engine.Model.BuysRemaining += n;

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                <if you='you get' them='getting'>{engine.Model.ActivePlayer}</if>
                <run>+{n} buys.</run>
            </spans>");
        }

        void IActionHost.AddMoney(int n)
        {
            engine.Model.MoneyRemaining += n;

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                <if you='you get' them='getting'>{engine.Model.ActivePlayer}</if>
                <run>+${n}.</run>
            </spans>");
        }

        Cards.CardModel[] IActionHost.DrawCards(int n)
        {
            var drawn = new List<string>();

            var reshuffled = false;
            for (var i = 0; i < n; i++)
            {
                reshuffled = reshuffled | engine.EnsureDeck(Player);
                var id = engine.DrawCardIfAny(Player);
                if (id != null)
                {
                    drawn.Add(id);
                }
                else
                {
                    break;
                }
            }
            if (reshuffled)
            {
                NoteReshuffle();
            }

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("draw", "draws", "drawing")}
                <run>{(n == 1 ? "a card." : $"{n} cards.")}</run>
            </spans>");

            return drawn.Select(Cards.All.ByName).ToArray();
        }

        void IActionHost.Discard(string[] cards, Zone from)
        {
            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Discard);
            }

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("discard", "discards", "discarding")}
                {LogCardList(cards)}
            </spans>");
        }

        void IActionHost.Trash(string[] cards, Zone from)
        {
            foreach (var card in cards)
            {
                engine.MoveCard(Player, card, from, Zone.Trash);
            }

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("trash", "trashes", "trashing")}
                {LogCardList(cards)}
            </spans>");        
        }

        void IActionHost.Gain(string id, Zone to)
        {
            engine.MoveCard(Player, id, Zone.Kingdom, to);

            if (to == Zone.Discard)
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("gain", "gains", "gaining")}
                    <card suffix='.'>{id}</card>
                </spans>");
            }
            else
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("gain", "gains", "gaining")}
                    <card>{id}</card>
                    <run>and</run>
                    {LogDestination(to)}
                </spans>");
            }
        }

        void IActionHost.Draw(string card)
        {
            engine.MoveCard(Player, card, Zone.TopDeck, Zone.Hand);

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("draw", "draws", "drawing")}
                <card suffix='.'>{card}</card>
            </spans>");        
        }

        Cards.CardModel[] IActionHost.RevealAll(Zone from)
        {
            if (from == Zone.TopDeck)
            {
                if (engine.EnsureDeck(Player))
                {
                    NoteReshuffle();
                }
                engine.DrawCardIfAny(Player, to: Zone.TopDeck);
            }

            var revealed = from switch {
                Zone.TopDeck => engine.Model.Decks[Player].Take(1),
                Zone.Hand => engine.Model.Hands[Player],
                Zone other => throw new CommandException($"Unknown zone {other}")
            };

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("reveal", "reveals", "revealing")}
                {LogCardList(revealed.ToArray())}
            </spans>");

            return revealed.Select(Cards.All.ByName).ToArray();
        }
        
        void IActionHost.RevealAndMove(string id, Zone from, Zone to)
        {
            engine.MoveCard(Player, id, from, to);

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("reveal", "reveals", "revealing")}
                <card>{id}</card>
                <run>and</run>
                {LogDestination(to)}
            </spans>");   
        }

        void IActionHost.PlayAction(Cards.ActionCardModel card, Zone from)
        {
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("play", "plays", "playing")}
                <card suffix='.'>{card.Name}</card>
            </spans>");

            engine.BeginAction(level + 1, Player, card, from);
        }

        async Task<T[]> IActionHost.SelectCards<T>(string prompt, Zone source, Func<IEnumerable<Cards.CardModel>, IEnumerable<T>> filter, int? min, int? max)
        {
            var sourceCards = source switch 
            {
                Zone.Hand => engine.Model.Hands[Player],
                Zone.Kingdom => engine.Model.KingdomCards.Concat(new[]{"Estate", "Duchy", "Province", "Copper", "Silver", "Gold"}).Where(id => engine.Model.Stacks[id] > 0),
                Zone other => throw new CommandException($"Unknown CardSource {other}")
            };

            var filteredCards = filter(sourceCards.Select(Cards.All.ByName));
            if (!filteredCards.Any())
            {
                return Array.Empty<T>();
            }

            var ids = await engine.Choose<SelectCards, string[]>(
                Player,
                ChoiceType.SelectCards, 
                prompt,
                new SelectCards
                {
                    Choices = filteredCards.Select(card => card.Name).ToArray(),
                    Min = min,
                    Max = max
                }
            );

            return ids.Select(Cards.All.ByName).Cast<T>().ToArray();
        }

        Task<bool> IActionHost.YesNo(string prompt, string message)
        {
            return engine.Choose<string, bool>(
                Player,
                ChoiceType.YesNo,
                prompt,
                message
            );
        }

        async Task IActionHost.Attack(Func<IActionHost, bool> filter, Func<IActionHost, Task> act)
        {
            var targetPlayers = engine.Model.Players
                .Except(new[]{ Player })
                .Select(player => new ActionHost(1, engine, player))
                .Where(filter)
                .ToList();
            
            foreach (var target in targetPlayers)
            {
                var hand = target.GetHand().ToList();
                var reactions = hand
                    .OfType<Cards.ActionCardModel>()
                    .Where(card => card.ReactionTrigger == TriggerType.Attack);

                var replaced = false;
                while (reactions.Any())
                {
                    var potentialReaction = reactions.First();
                    hand.RemoveAt(hand.FindIndex(e => e.Equals(potentialReaction.Name)));

                    var targetHost = new ActionHost(1, engine, target.Player);
                    var reaction = await potentialReaction.ExecuteReactionAsync(targetHost, Player);          
                    if (reaction.Type == ReactionType.Replace)
                    {
                        engine.LogPartialEvent($@"<spans>
                            <player>{target.Player}</player>
                            <if you='reveal' them='reveals'>{target.Player}</if>
                            <card suffix='!'>{potentialReaction.Name}</card>
                        </spans>");

                        replaced = true;
                        reaction.Enact(targetHost, this);
                    }
                }

                if (!replaced)
                {
                    await act(target);
                }
            }
        }
    }
}