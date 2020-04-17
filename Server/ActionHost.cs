using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Server
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

        private string LogCardList(string[] ids, bool terminal = true)
        {
            if (!ids.Any())
            {
                return terminal ? "<run>nothing.</run>" : "<run>nothing</run>";
            }

            return string.Join(Environment.NewLine, ids.Select((id, ix) => 
            {
                var suffix = ix == ids.Length -1 ? (terminal ? "." : "")
                    : ix < ids.Length - 2 ? ","
                    : " and";
                return $"<card suffix='{suffix}'>{id}</card>";
            }));
        }

        private string LogSource(Zone from)
        {
            switch (from)
            {
                case Zone.Trash:
                    return $@"<run>from the trash.</run>";

                default:
                    throw new CommandException($"Unknown zone {from}");
            }
        }

        private string LogDestination(Zone to)
        {
            switch (to)
            {
                case Zone.DeckTop1:
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

        ICard[] IActionHost.GetHand()
        {
            return engine.GetCards(Player, Zone.Hand, NoteReshuffle).Select(All.Cards.ByName).ToArray();
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

        ICard[] IActionHost.DrawCards(int n)
        {
            var drawn = new List<string>();

            var reshuffled = false;
            for (var i = 0; i < n; i++)
            {
                reshuffled = reshuffled | engine.ReshuffleIfEmpty(Player);
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

            return drawn.Select(All.Cards.ByName).ToArray();
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

        void IActionHost.DiscardEntireDeck()
        {
            var deck = engine.Model.Decks[Player];
            while (deck.Any())
            {
                engine.MoveCard(Player, deck[0], Zone.DeckTop1, Zone.Discard);
            }

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("discard", "discards", "discarding")}
                <if you='your' them='their'>{Player}</if>
                <run>deck.</run>
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
            if (engine.Model.Supply[id] == 0)
            {
                engine.LogPartialEvent($@"<spans>
                    <indent level='{level}' />
                    {LogVerbInitial("don&apos;t gain", "doesn&apos;t gain", "not gaining")}
                    <card suffix=','>{id}</card>
                    <run>because there are none available.</run>
                </spans>");
                return;
            }

            engine.MoveCard(Player, id, Zone.Supply, to);

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

        void IActionHost.GainFrom(string[] cards, Zone from)
        {
            foreach (var id in cards)
            {
                engine.MoveCard(Player, id, from, Zone.Discard);
            }
            
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("gain", "gains", "gaining")}
                {LogCardList(cards, terminal: false)}
                {LogSource(from)}
            </spans>");
        }

        void IActionHost.Draw(string card)
        {
            engine.MoveCard(Player, card, Zone.DeckTop1, Zone.Hand);

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("draw", "draws", "drawing")}
                <card suffix='.'>{card}</card>
            </spans>");        
        }

        void IActionHost.Reveal(string card)
        {
            engine.LogPartialEvent($@"<spans>
                <player>{Player}</player>
                <if you='reveal' them='reveals'>{Player}</if>
                <card suffix='!'>{card}</card>
            </spans>");
        }

        ICard[] IActionHost.RevealAll(Zone source)
        {
            var sourceCards = engine.GetCards(Player, source, NoteReshuffle);

            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("reveal", "reveals", "revealing")}
                {LogCardList(sourceCards)}
            </spans>");

            return sourceCards.Select(All.Cards.ByName).ToArray();
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

        async Task IActionHost.PlayCard(string card, Zone from)
        {
            engine.LogPartialEvent($@"<spans>
                <indent level='{level}' />
                {LogVerbInitial("play", "plays", "playing")}
                <card suffix='.'>{card}</card>
            </spans>");

            await engine.PlayCardAsync(level + 1, Player, card, from);
        }

        async Task<T[]> IActionHost.SelectCards<T>(string prompt, Zone source, Func<IEnumerable<ICard>, IEnumerable<T>> filter, int? min, int? max)
        {            
            var sourceCards = engine.GetCards(Player, source, NoteReshuffle);

            var filteredCards = filter(sourceCards.Select(All.Cards.ByName));
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

            return ids.Select(All.Cards.ByName).Cast<T>().ToArray();
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

        async Task IActionHost.Attack(Func<IActionHost, bool> filter, Func<IActionHost, Task> act, bool benign)
        {
            var targetPlayers = engine.Model.Players
                .Except(new[]{ Player })
                .Select(player => new ActionHost(1, engine, player))
                .Where(filter)
                .ToList();
            
            foreach (var target in targetPlayers)
            {
                await engine.Act(level, target.Player, Trigger.Attack, Player, async () =>
                {
                    await act(target);
                });
            }
        }
    
        void IActionHost.AddEffect(string effect)
        {
            engine.Model.ActiveEffects.Add(effect);
        }

        void IActionHost.RemoveEffect(string effect)
        {
            engine.Model.ActiveEffects.Remove(effect);
        }
    }
}